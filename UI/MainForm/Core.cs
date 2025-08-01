using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Services;
using StreamingPlayerNET.Source.Base.Interfaces;
using StreamingPlayerNET.Source.YoutubeDL;
using StreamingPlayerNET.Source.Spotify;
using StreamingPlayerNET.Source.YouTubeMusic;
using NLog;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private async Task InitializeServicesAsync()
    {
        Logger.Info("Initializing services...");
        
        try
        {
            // Initialize configuration service first
            _configService = new ConfigurationService();
            
            // Initialize individual services
            // Create and register source providers
            var youtubeProvider = new YouTubeSourceProvider(ConfigurationService.Current);
            var youtubeMusicProvider = new YouTubeMusicSourceProvider(ConfigurationService.Current);
            var spotifyProvider = new SpotifySourceProvider(ConfigurationService.Current);
            
            SourceManager.Instance.RegisterSourceProvider(youtubeProvider);
            SourceManager.Instance.RegisterSourceProvider(youtubeMusicProvider);
            SourceManager.Instance.RegisterSourceProvider(spotifyProvider);
            
            // Initialize all source providers with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                await SourceManager.Instance.InitializeAllAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Logger.Warn("Service initialization timed out after 10 seconds. Continuing with limited functionality.");
                
            }
            
            // Create multi-source services that aggregate results from all enabled sources
            _searchService = new MultiSourceSearchService(SourceManager.Instance);
            _metadataService = new MultiSourceMetadataService(SourceManager.Instance);
            
            // Load playlists after services are initialized
            LoadPlaylists();
            _downloadService = new MultiSourceDownloadService(SourceManager.Instance);
            _playbackService = new NAudioPlaybackService();
            
            // Wire up download progress events
            _downloadService.DownloadProgressChanged += OnDownloadProgressChanged;
            
            // Create main music player service
            _musicPlayerService = new MusicPlayerService(_searchService, _metadataService, _downloadService, _playbackService);
            
            // Initialize Windows Media Service for system media controls
            try
            {
                _windowsMediaService = new WindowsMediaService(_musicPlayerService, _configService, this.Handle);
                
                // Wire up media command handling
                _windowsMediaService.MediaCommandReceived += OnMediaCommandReceived;
                
                Logger.Info("Windows Media Service initialized");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to initialize Windows Media Service - system media controls will not be available");
            }
            
            // Wire up music player events
            _musicPlayerService.SongChanged += OnSongChanged;
            _musicPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
            _musicPlayerService.PositionChanged += OnPositionChanged;
            _musicPlayerService.PlaybackCompleted += OnPlaybackCompleted;
            
            // Wire up queue events
            _queue.OnRepeatModeChanged += OnQueueRepeatModeChanged;
            _queue.OnShuffleChanged += OnQueueShuffleChanged;
            _queue.OnSongsChanged += OnQueueSongsChanged;
            _queue.OnCurrentIndexChanged += OnQueueCurrentIndexChanged;
            
            // Load cached queue if available
            LoadCachedQueue();
            

            
            // Initialize toast notification service
            InitializeToastNotifications();
                
            Logger.Info("All services initialized successfully");
            Logger.Info("Ready! You can now search for music across all enabled sources.");
            
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize services");
            MessageBox.Show($"Failed to initialize services: {ex.Message}", "Initialization Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            
        }
    }

    private async Task PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(searchTextBox.Text))
        {
            Logger.Debug("Search skipped: empty search text");
            return;
        }
        
        var query = searchTextBox.Text.Trim();
        Logger.Info($"Starting search for: '{query}'");
        
        try
        {
            Logger.Info("Searching across all enabled sources...");
            
            var songs = await (_musicPlayerService?.SearchAsync(query, 50) ?? Task.FromResult(new List<Song>()));
            
            // Update the search results using data binding
            UpdateSearchResults(songs);
            
            // Restore the search text box placeholder
            searchTextBox.PlaceholderText = "Search for songs, artists, albums...";
            
            // Get unique sources from the results
            var sources = songs.Where(s => !string.IsNullOrEmpty(s.Source)).Select(s => s.Source).Distinct().ToList();
            var sourceText = sources.Count > 0 ? $" from {string.Join(", ", sources)}" : "";
            
            Logger.Info($"Search completed: Found {songs.Count} songs{sourceText}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Search failed for query: '{query}'");
            MessageBox.Show($"Search failed: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            
        }
    }

    private void LoadCachedQueue()
    {
        try
        {
            var queueCache = QueueCacheService.LoadQueue();
            if (queueCache != null && queueCache.Songs.Count > 0)
            {
                // Clear current queue
                _queue.Clear();
                
                // Add cached songs
                foreach (var song in queueCache.Songs)
                {
                    _queue.AddSong(song);
                }
                
                // Restore queue state
                _queue.CurrentIndex = queueCache.CurrentIndex;
                _queue.RepeatMode = queueCache.RepeatMode;
                _queue.ShuffleEnabled = queueCache.ShuffleEnabled;
                
                Logger.Info($"Restored queue with {queueCache.Songs.Count} songs from cache");
                
                // Update UI to reflect the restored queue
                UpdateQueueDisplay();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load cached queue");
        }
    }

    private void SaveCachedQueue()
    {
        try
        {
            QueueCacheService.SaveQueue(_queue);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save cached queue");
        }
    }

    private void OnQueueSongsChanged()
    {
        // Save queue when songs change
        SaveCachedQueue();
    }

    private void OnQueueCurrentIndexChanged(object? sender, int currentIndex)
    {
        var indexId = Guid.NewGuid().ToString("N")[..8];
        Logger.Debug($"[QueueIndex-{indexId}] *** QUEUE CURRENT INDEX CHANGED to {currentIndex}, Thread: {Thread.CurrentThread.ManagedThreadId}");
        
        // Save queue when current index changes
        SaveCachedQueue();
        
        Logger.Debug($"[QueueIndex-{indexId}] Queue index change handling completed");
    }



    private void InitializeToastNotifications()
    {
        try
        {
            _toastNotificationService = new ToastNotificationService();
            Logger.Info("Toast notification service initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize toast notification service");
        }
    }

    private void OnMediaCommandReceived(object? sender, MediaCommand command)
    {
        try
        {
            // Handle media commands from Windows Media Session
            switch (command)
            {
                case MediaCommand.Play:
                case MediaCommand.Pause:
                    this.Invoke(OnPlayPauseButtonClick);
                    break;
                    
                case MediaCommand.Stop:
                    this.Invoke(() => _musicPlayerService?.Stop());
                    break;
                    
                case MediaCommand.Next:
                    this.Invoke(async () => await PlayNextSong());
                    break;
                    
                case MediaCommand.Previous:
                    this.Invoke(async () => await PlayPreviousSong());
                    break;
                    
                case MediaCommand.VolumeUp:
                    this.Invoke(() => AdjustVolume(10));
                    break;
                    
                case MediaCommand.VolumeDown:
                    this.Invoke(() => AdjustVolume(-10));
                    break;
                    
                default:
                    Logger.Warn($"Unhandled media command received: {command}");
                    break;
            }
            
            Logger.Debug($"Processed media command from Windows Media Session: {command}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to handle media command: {command}");
        }
    }
}