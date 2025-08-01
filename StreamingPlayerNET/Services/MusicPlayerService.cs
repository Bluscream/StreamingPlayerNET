
using System.Diagnostics;
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
    
    private Song? _currentSong;
    private Playlist? _currentPlaylist;
    private int _currentPlaylistIndex = -1;
    private bool _wasManuallyStopped = false;
    
    public event EventHandler<Song>? SongChanged;
    public event EventHandler<PlaybackState>? PlaybackStateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler? PlaybackCompleted;
    
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
            if (_currentSong != null)
            {
                _currentSong.StopSongTimer();
                var totalTime = _currentSong.GetCurrentSongTime();
                Logger.Info($"Song playback completed: {_currentSong.Title} - Total time from start to finish: {totalTime?.TotalMilliseconds.Milliseconds()}");
            }
            PlaybackCompleted?.Invoke(this, e);
        };
        
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
        Logger.Info($"Starting complete playback process for: {song.Title}");
        
        try
        {
            // Start the song timer at the very beginning
            song.StartSongTimer();
            song.State = PlaybackState.Loading;
            
            // Step 1: Get metadata if not already available
            if (string.IsNullOrEmpty(song.Title) || song.SelectedStream == null)
            {
                Logger.Info("Step 1: Getting song metadata and audio streams");
                var metadataStartTime = song.GetCurrentSongTime();
                
                var metadata = await _metadataService.GetSongMetadataAsync(song.Id, cancellationToken);
                
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
                Logger.Info($"Metadata retrieval completed in {metadataDuration?.TotalMilliseconds.Milliseconds()}");
                
                // Get best audio stream
                Logger.Info("Step 2: Getting best audio stream");
                var streamStartTime = song.GetCurrentSongTime();
                
                song.SelectedStream = await _metadataService.GetBestAudioStreamAsync(song.Id, cancellationToken);
                
                var streamEndTime = song.GetCurrentSongTime();
                var streamDuration = streamEndTime - streamStartTime;
                Logger.Info($"Audio stream selection completed in {streamDuration?.TotalMilliseconds.Milliseconds()}");
                Logger.Info($"Selected audio stream: {song.SelectedStream}");
            }
            else
            {
                Logger.Info("Metadata and audio stream already available, skipping retrieval");
            }
            
            // Step 3: Start playback
            Logger.Info("Step 3: Starting audio playback");
            var playbackStartTime = song.GetCurrentSongTime();
            
            await _playbackService.PlayAsync(song, cancellationToken);
            
            var playbackEndTime = song.GetCurrentSongTime();
            var playbackSetupDuration = playbackEndTime - playbackStartTime;
            Logger.Info($"Audio playback setup completed in {playbackSetupDuration?.TotalMilliseconds.Milliseconds()}");
            
            _currentSong = song;
            song.State = PlaybackState.Playing;
            _wasManuallyStopped = false; // Clear manual stop flag when starting new playback
            
            Logger.Info($"*** FIRING SongChanged event for: {song.Title} by {song.Artist} on thread {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            SongChanged?.Invoke(this, song);
            Logger.Debug($"*** SongChanged event completed");
            
            var totalSetupTime = song.GetCurrentSongTime();
            Logger.Info($"Complete playback setup process completed in {totalSetupTime?.TotalMilliseconds.Milliseconds()}");
            Logger.Info($"Song '{song.Title}' is now playing - timer continues until playback ends");
        }
        catch (Exception ex)
        {
            song.State = PlaybackState.Stopped;
            song.StopSongTimer();
            var failedTime = song.GetCurrentSongTime();
            Logger.Error(ex, $"Failed to play song: {song.Title} after {failedTime?.TotalMilliseconds.Milliseconds()}");
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
        
        _currentPlaylist = playlist;
        _currentPlaylistIndex = startIndex - 1; // Will be incremented in PlayNextSongAsync
        
        await PlayNextSongAsync(cancellationToken);
    }
    
    public async Task PlayNextSongAsync(CancellationToken cancellationToken = default)
    {
        if (_currentPlaylist == null || _currentPlaylist.Songs.Count == 0)
        {
            Logger.Warn("No current playlist or playlist is empty");
            return;
        }
        
        _currentPlaylistIndex++;
        
        if (_currentPlaylistIndex >= _currentPlaylist.Songs.Count)
        {
            Logger.Info("Reached end of playlist");
            _currentPlaylistIndex = 0; // Loop back to beginning
        }
        
        var song = _currentPlaylist.Songs[_currentPlaylistIndex];
        Logger.Info($"Playing next song in playlist: {song.Title} (index {_currentPlaylistIndex + 1}/{_currentPlaylist.Songs.Count})");
        
        await PlaySongAsync(song, cancellationToken);
    }
    
    public async Task PlayPreviousSongAsync(CancellationToken cancellationToken = default)
    {
        if (_currentPlaylist == null || _currentPlaylist.Songs.Count == 0)
        {
            Logger.Warn("No current playlist or playlist is empty");
            return;
        }
        
        _currentPlaylistIndex--;
        
        if (_currentPlaylistIndex < 0)
        {
            _currentPlaylistIndex = _currentPlaylist.Songs.Count - 1; // Loop to end
        }
        
        var song = _currentPlaylist.Songs[_currentPlaylistIndex];
        Logger.Info($"Playing previous song in playlist: {song.Title} (index {_currentPlaylistIndex + 1}/{_currentPlaylist.Songs.Count})");
        
        await PlaySongAsync(song, cancellationToken);
    }
    
    public void Pause()
    {
        Logger.Info("Pausing playback");
        _playbackService.Pause();
        if (_currentSong != null)
        {
            _currentSong.State = PlaybackState.Paused;
            var pauseTime = _currentSong.GetCurrentSongTime();
            Logger.Info($"Song paused at {pauseTime?.TotalMilliseconds.Milliseconds()} into the process");
        }
    }
    
    public void Resume()
    {
        Logger.Info("Resuming playback");
        _playbackService.Resume();
        if (_currentSong != null)
        {
            _currentSong.State = PlaybackState.Playing;
            var resumeTime = _currentSong.GetCurrentSongTime();
            Logger.Info($"Song resumed at {resumeTime?.TotalMilliseconds.Milliseconds()} into the process");
        }
    }
    
    public void Stop()
    {
        Logger.Info("Stopping playback");
        _wasManuallyStopped = true;
        _playbackService.Stop();
        if (_currentSong != null)
        {
            _currentSong.State = PlaybackState.Stopped;
            _currentSong.StopSongTimer();
            var stopTime = _currentSong.GetCurrentSongTime();
            Logger.Info($"Song stopped after {stopTime?.TotalMilliseconds.Milliseconds()} - process terminated early");
        }
    }
    
    public void SetVolume(float volume)
    {
        _playbackService.SetVolume(volume);
        if (_currentSong != null)
        {
            _currentSong.Volume = volume;
        }
    }
    
    public void SetPosition(TimeSpan position)
    {
        _playbackService.SetPosition(position);
        if (_currentSong != null)
        {
            _currentSong.CurrentPosition = position;
        }
    }
    
    // Getters for current state
    public Song? GetCurrentSong() => _currentSong;
    public Playlist? GetCurrentPlaylist() => _currentPlaylist;
    public int GetCurrentPlaylistIndex() => _currentPlaylistIndex;
    public PlaybackState GetPlaybackState() => _playbackService.GetPlaybackState();
    public TimeSpan GetCurrentPosition() => _playbackService.GetCurrentPosition();
    public TimeSpan? GetTotalDuration() => _playbackService.GetTotalDuration();
    public float GetVolume() => _playbackService.GetVolume();
    public bool IsPlaying => _playbackService.IsPlaying;
    public bool IsPaused => _playbackService.IsPaused;
    public bool IsStopped => _playbackService.IsStopped;
} 