
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;
using Humanizer;

namespace StreamingPlayerNET.Services;

public class MusicPlayerService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly ISearchService _searchService;
    private readonly IMetadataService _metadataService;
    private readonly IDownloadService _downloadService;
    private readonly IPlaybackService _playbackService;
    private readonly FileAssociationService _fileAssociationService;
    
    private bool _wasManuallyStopped = false;
    
    public Song? CurrentSong { get; private set; }
    public Playlist? CurrentPlaylist { get; private set; }
    public float CurrentVolume { get; private set; } = 1.0f;
    public int CurrentPlaylistIndex { get; private set; } = -1;
    
    public event EventHandler<Song>? SongChanged;
    public event EventHandler<PlaybackState>? PlaybackStateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler<float>? VolumeChanged;
    public event EventHandler? PlaybackCompleted;
    public event EventHandler<PlaybackErrorEventArgs>? PlaybackError;
    
    public bool WasManuallyStopped => _wasManuallyStopped;
    
    public MusicPlayerService(
        ISearchService searchService,
        IMetadataService metadataService,
        IDownloadService downloadService,
        IPlaybackService playbackService)
    {
        _searchService = searchService;
        _metadataService = metadataService;
        _downloadService = downloadService;
        _playbackService = playbackService;
        _fileAssociationService = new FileAssociationService();
        
        // Set download service on playback service if it supports it
        if (_playbackService is NAudioPlaybackService naudioService)
        {
            naudioService.SetDownloadService(_downloadService);
            
            // Create and set caching service
            var cachingService = new CachingService();
            naudioService.SetCachingService(cachingService);
        }
        
        // Wire up playback service events
        _playbackService.PlaybackStateChanged += (s, state) => PlaybackStateChanged?.Invoke(this, state);
        _playbackService.PositionChanged += (s, position) => PositionChanged?.Invoke(this, position);
        _playbackService.PlaybackCompleted += (s, e) => 
        {
            if (CurrentSong != null)
            {
                CurrentSong.StopSongTimer();
                var totalTime = CurrentSong.GetCurrentSongTime();
                Logger.Info($"Song playback completed: {CurrentSong.Title} - Total time from start to finish: {totalTime?.TotalMilliseconds.Milliseconds()}");
            }
            PlaybackCompleted?.Invoke(this, e);
        };
        _playbackService.PlaybackError += OnPlaybackError;
        
        Logger.Info("Music Player Service initialized");
    }
    
    public async Task<List<Song>> SearchAsync(string query, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Searching for: '{query}'");
        return await _searchService.SearchAsync(query, maxResults, cancellationToken);
    }
    
    public async Task<List<Playlist>> SearchPlaylistsAsync(string query, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Searching playlists for: '{query}'");
        return await _searchService.SearchPlaylistsAsync(query, maxResults, cancellationToken);
    }
    
    public async Task PlaySongAsync(Song song, CancellationToken cancellationToken = default)
    {
        var playId = Guid.NewGuid().ToString("N")[..8];
        Logger.Info($"[MusicPlayer-{playId}] Starting complete playback process for: {song.Title}, Thread: {Thread.CurrentThread.ManagedThreadId}");
        
        try
        {
            // Start the song timer at the very beginning
            Logger.Debug($"[MusicPlayer-{playId}] Starting song timer");
            song.StartSongTimer();
            song.State = PlaybackState.Loading;
            
            // Record playback start
            song.RecordPlaybackStart();
            
            // Step 1: Get metadata if not already available
            if (string.IsNullOrEmpty(song.Title) || song.SelectedStream == null)
            {
                Logger.Info($"[MusicPlayer-{playId}] Step 1: Getting song metadata and audio streams");
                var metadataStartTime = song.GetCurrentSongTime();
                
                Logger.Debug($"[MusicPlayer-{playId}] About to call _metadataService.GetSongMetadataAsync");
                var metadata = await _metadataService.GetSongMetadataAsync(song.Id, cancellationToken);
                Logger.Debug($"[MusicPlayer-{playId}] _metadataService.GetSongMetadataAsync completed");
                
                // Update song with metadata
                song.Title = metadata.Title;
                song.Artist = metadata.Artist;
                song.Duration = metadata.Duration;
                song.ThumbnailUrl = metadata.ThumbnailUrl;
                song.Description = metadata.Description;
                song.UploadDate = metadata.UploadDate;
                song.ViewCount = metadata.ViewCount;
                song.LikeCount = metadata.LikeCount;
                
                var metadataEndTime = song.GetCurrentSongTime();
                var metadataDuration = metadataEndTime - metadataStartTime;
                Logger.Info($"[MusicPlayer-{playId}] Metadata retrieval completed in {metadataDuration?.TotalMilliseconds.Milliseconds()}");
                
                // Get best audio stream
                Logger.Info($"[MusicPlayer-{playId}] Step 2: Getting best audio stream");
                var streamStartTime = song.GetCurrentSongTime();
                
                Logger.Debug($"[MusicPlayer-{playId}] About to call _metadataService.GetBestAudioStreamAsync");
                song.SelectedStream = await _metadataService.GetBestAudioStreamAsync(song.Id, cancellationToken);
                Logger.Debug($"[MusicPlayer-{playId}] _metadataService.GetBestAudioStreamAsync completed");
                
                var streamEndTime = song.GetCurrentSongTime();
                var streamDuration = streamEndTime - streamStartTime;
                Logger.Info($"[MusicPlayer-{playId}] Audio stream selection completed in {streamDuration?.TotalMilliseconds.Milliseconds()}");
                Logger.Info($"[MusicPlayer-{playId}] Selected audio stream: {song.SelectedStream}");
            }
            else
            {
                Logger.Info($"[MusicPlayer-{playId}] Metadata and audio stream already available, skipping retrieval");
            }
            
            // Step 3: Start playback
            Logger.Info($"[MusicPlayer-{playId}] Step 3: Starting audio playback");
            var playbackStartTime = song.GetCurrentSongTime();
            
            Logger.Debug($"[MusicPlayer-{playId}] About to call _playbackService.PlayAsync");
            await _playbackService.PlayAsync(song, cancellationToken);
            Logger.Debug($"[MusicPlayer-{playId}] _playbackService.PlayAsync completed");
            
            var playbackEndTime = song.GetCurrentSongTime();
            var playbackSetupDuration = playbackEndTime - playbackStartTime;
            Logger.Info($"[MusicPlayer-{playId}] Audio playback setup completed in {playbackSetupDuration?.TotalMilliseconds.Milliseconds()}");
            
            CurrentSong = song;
            song.State = PlaybackState.Playing;
            _wasManuallyStopped = false; // Clear manual stop flag when starting new playback
            
            Logger.Info($"[MusicPlayer-{playId}] *** FIRING SongChanged event for: {song.Title} by {song.Artist} on thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            SongChanged?.Invoke(this, song);
            Logger.Debug($"[MusicPlayer-{playId}] *** SongChanged event completed");
            
            var totalSetupTime = song.GetCurrentSongTime();
            Logger.Info($"[MusicPlayer-{playId}] Complete playback setup process completed in {totalSetupTime?.TotalMilliseconds.Milliseconds()}");
            Logger.Info($"[MusicPlayer-{playId}] Song '{song.Title}' is now playing - timer continues until playback ends");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"[MusicPlayer-{playId}] Failed to play song: {song.Title}");
            song.State = PlaybackState.Stopped;
            song.StopSongTimer();
            var failedTime = song.GetCurrentSongTime();
            Logger.Error(ex, $"[MusicPlayer-{playId}] Failed to play song: {song.Title} after {failedTime?.TotalMilliseconds.Milliseconds()}");
            throw;
        }
    }
    
    public async Task PlaySongByIdAsync(string songId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Playing song by ID: {songId}");
        
        var song = new Song { Id = songId };
        await PlaySongAsync(song, cancellationToken);
    }
    
    public async Task PlayPlaylistAsync(Playlist playlist, int startIndex = 0, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Playing playlist: {playlist.Name} (starting at index {startIndex})");
        
        CurrentPlaylist = playlist;
        CurrentPlaylistIndex = startIndex - 1; // Will be incremented in PlayNextSongAsync
        
        await PlayNextSongAsync(cancellationToken);
    }
    
    public async Task PlayNextSongAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentPlaylist == null || CurrentPlaylist.Songs.Count == 0)
        {
            Logger.Warn("No current playlist or playlist is empty");
            return;
        }
        
        CurrentPlaylistIndex++;
        
        if (CurrentPlaylistIndex >= CurrentPlaylist.Songs.Count)
        {
            Logger.Info("Reached end of playlist");
            CurrentPlaylistIndex = 0; // Loop back to beginning
        }
        
        var song = CurrentPlaylist.Songs[CurrentPlaylistIndex];
        Logger.Info($"Playing next song in playlist: {song.Title} (index {CurrentPlaylistIndex + 1}/{CurrentPlaylist.Songs.Count})");
        
        await PlaySongAsync(song, cancellationToken);
    }
    
    public async Task PlayPreviousSongAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentPlaylist == null || CurrentPlaylist.Songs.Count == 0)
        {
            Logger.Warn("No current playlist or playlist is empty");
            return;
        }
        
        CurrentPlaylistIndex--;
        
        if (CurrentPlaylistIndex < 0)
        {
            CurrentPlaylistIndex = CurrentPlaylist.Songs.Count - 1; // Loop to end
        }
        
        var song = CurrentPlaylist.Songs[CurrentPlaylistIndex];
        Logger.Info($"Playing previous song in playlist: {song.Title} (index {CurrentPlaylistIndex + 1}/{CurrentPlaylist.Songs.Count})");
        
        await PlaySongAsync(song, cancellationToken);
    }
    
    public void Pause()
    {
        Logger.Info("Pausing playback");
        _playbackService.Pause();
        if (CurrentSong != null)
        {
            CurrentSong.State = PlaybackState.Paused;
            var pauseTime = CurrentSong.GetCurrentSongTime();
            Logger.Info($"Song paused at {pauseTime?.TotalMilliseconds.Milliseconds()} into the process");
            
            // Save the current position
            CurrentSong.RecordPlaybackPause();
        }
    }
    
    public void Resume()
    {
        Logger.Info("Resuming playback");
        _playbackService.Resume();
        if (CurrentSong != null)
        {
            CurrentSong.State = PlaybackState.Playing;
            var resumeTime = CurrentSong.GetCurrentSongTime();
            Logger.Info($"Song resumed at {resumeTime?.TotalMilliseconds.Milliseconds()} into the process");
            
            // Record playback start
            CurrentSong.RecordPlaybackStart();
        }
    }
    
    public void Stop()
    {
        Logger.Info("Stopping playback");
        _wasManuallyStopped = true;
        _playbackService.Stop();
        if (CurrentSong != null)
        {
            CurrentSong.State = PlaybackState.Stopped;
            CurrentSong.StopSongTimer();
            var stopTime = CurrentSong.GetCurrentSongTime();
            Logger.Info($"Song stopped after {stopTime?.TotalMilliseconds.Milliseconds()} - process terminated early");
            
            // Save the current position for later resumption
            CurrentSong.SaveCurrentPosition();
        }
    }
    
    public void SetVolume(float volume)
    {
        Logger.Debug($"Setting volume to: {volume:P0}");
        _playbackService.SetVolume(volume);
        CurrentVolume = volume;
        if (CurrentSong != null)
        {
            CurrentSong.Volume = volume;
        }
        
        // Fire volume changed event
        VolumeChanged?.Invoke(this, volume);
    }
    
    public void SetPosition(TimeSpan position)
    {
        _playbackService.SetPosition(position);
        if (CurrentSong != null)
        {
            CurrentSong.CurrentPosition = position;
        }
    }
    
    // Getters for current state
    public PlaybackState GetPlaybackState() => _playbackService.GetPlaybackState();
    public TimeSpan GetCurrentPosition() => _playbackService.GetCurrentPosition();
    public TimeSpan? GetTotalDuration() => _playbackService.GetTotalDuration();
    public bool IsPlaying => _playbackService.IsPlaying;
    public bool IsPaused => _playbackService.IsPaused;
    public bool IsStopped => _playbackService.IsStopped;
    
    /// <summary>
    /// Plays a Song with position restoration if available
    /// </summary>
    public async Task PlaySongWithPositionRestorationAsync(Song song, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Playing Song with position restoration: {song.Title} (Saved position: {song.SavedPosition})");
        
        // If there's a saved position and the song was playing, restore it
        if (song.WasPlaying && song.SavedPosition > TimeSpan.Zero)
        {
            Logger.Info($"Restoring playback position to {song.SavedPosition}");
            song.RestorePosition();
        }
        
        await PlaySongAsync(song, cancellationToken);
    }
    
    /// <summary>
    /// Handles playback errors and provides option to open file in associated application
    /// </summary>
    private void OnPlaybackError(object? sender, PlaybackErrorEventArgs e)
    {
        Logger.Warn($"Playback error occurred for file: {e.FilePath}");
        Logger.Warn($"Error details: {e.Exception.Message}");
        
        // Fire the playback error event for UI handling
        PlaybackError?.Invoke(this, e);
    }
} 