using NAudio.Wave;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;
using System.Diagnostics;
using Humanizer;
using static StreamingPlayerNET.Common.Models.PlaybackState;

namespace StreamingPlayerNET.Services;

public class NAudioPlaybackService : IPlaybackService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private WaveOutEvent? _audioOutput;
    private IWaveProvider? _audioProvider;
    private string? _currentTempFile;
    private System.Windows.Forms.Timer? _progressTimer;
    private IDownloadService? _downloadService;
    private CachingService? _cachingService;
    private bool _isManualStop = false;
    
    public event EventHandler<StreamingPlayerNET.Common.Models.PlaybackState>? PlaybackStateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler? PlaybackCompleted;
    
    public bool IsPlaying => _audioOutput?.PlaybackState == NAudio.Wave.PlaybackState.Playing;
    public bool IsPaused => _audioOutput?.PlaybackState == NAudio.Wave.PlaybackState.Paused;
    public bool IsStopped => _audioOutput?.PlaybackState == NAudio.Wave.PlaybackState.Stopped;
    
    public NAudioPlaybackService()
    {
        Logger.Info("NAudio Playback Service initialized");
        SetupProgressTimer();
    }
    
    public void SetDownloadService(IDownloadService downloadService)
    {
        _downloadService = downloadService;
        
        // If we have a caching service, set the download service on it too
        if (_cachingService != null)
        {
            _cachingService.SetDownloadService(_downloadService);
        }
        
        Logger.Debug("Download service set for NAudio playback service");
    }
    
    public void SetCachingService(CachingService cachingService)
    {
        _cachingService = cachingService;
        
        // If we have a download service, set it on the caching service too
        if (_downloadService != null)
        {
            _cachingService.SetDownloadService(_downloadService);
        }
        
        Logger.Debug("Caching service set for NAudio playback service");
    }
    
    private void SetupProgressTimer()
    {
        _progressTimer = new System.Windows.Forms.Timer();
        _progressTimer.Interval = 100; // Update every 100ms for smoother progress
        _progressTimer.Tick += ProgressTimer_Tick;
    }
    
    private void ProgressTimer_Tick(object? sender, EventArgs e)
    {
        if (_audioProvider is AudioFileReader audioFile && _audioOutput != null)
        {
            var position = audioFile.CurrentTime;
            PositionChanged?.Invoke(this, position);
            
            if (_audioOutput.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
            {
                PlaybackCompleted?.Invoke(this, EventArgs.Empty);
                Stop();
            }
        }
    }
    
    public async Task PlayAsync(Song song, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Info($"Starting playback for song: {song.Title}");
        
        if (song.SelectedStream == null)
        {
            stopwatch.Stop();
            Logger.Error($"Song has no selected audio stream after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            throw new InvalidOperationException("Song has no selected audio stream");
        }
        
        try
        {
            await PlayAsync(song.SelectedStream, song, cancellationToken);
            stopwatch.Stop();
            Logger.Info($"Playback setup completed for song: {song.Title} in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, $"Playback setup failed for song: {song.Title} after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            throw;
        }
    }
    
    public async Task PlayAsync(AudioStreamInfo streamInfo, CancellationToken cancellationToken = default)
    {
        await PlayAsync(streamInfo, null, cancellationToken);
    }
    
    public async Task PlayAsync(AudioStreamInfo streamInfo, Song? song = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Info($"Starting playback for audio stream: {streamInfo}");
        
        try
        {
            StopForNewSong(); // Stop any current playback (manual stop)
            
            string filePath;
            
            // Try to get cached file first
            if (_cachingService != null && song != null)
            {
                var cachedPath = _cachingService.GetCachedFilePath(song, streamInfo);
                if (cachedPath != null)
                {
                    Logger.Debug("Using cached file for playback");
                    filePath = cachedPath;
                }
                else
                {
                    // Download and cache the file
                    Logger.Debug("Downloading and caching file for playback");
                    filePath = await _cachingService.DownloadAndCacheAsync(song, streamInfo, cancellationToken);
                }
            }
            else if (_downloadService != null)
            {
                // Use download service if caching service is not available
                try
                {
                    Logger.Debug("Using download service for playback");
                    var songTitle = song?.Title;
                    var tempFilePath = await _downloadService.DownloadAudioAsync(streamInfo, songTitle, cancellationToken);
                    
                    // Cache the downloaded file if caching service is available
                    if (_cachingService != null && song != null)
                    {
                        filePath = await _cachingService.CacheFileAsync(song, streamInfo, tempFilePath, cancellationToken);
                        
                        // Clean up temp file
                        try
                        {
                            if (File.Exists(tempFilePath))
                            {
                                File.Delete(tempFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex, $"Failed to clean up temp file: {tempFilePath}");
                        }
                    }
                    else
                    {
                        filePath = tempFilePath;
                        _currentTempFile = tempFilePath;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Download service failed, falling back to direct streaming");
                    filePath = await DownloadDirectlyAsync(streamInfo.Url, cancellationToken);
                }
            }
            else
            {
                // Fallback to direct download
                Logger.Debug("Using direct download fallback");
                filePath = await DownloadDirectlyAsync(streamInfo.Url, cancellationToken);
            }
            
            await PlayAsync(filePath, cancellationToken);
            stopwatch.Stop();
            Logger.Info($"Playback setup completed in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, $"Playback failed for stream: {streamInfo} after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            throw;
        }
    }
    
    public Task PlayAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Info($"Starting playback for file: {filePath}");
        
        try
        {
            StopForNewSong(); // Stop any current playback (manual stop)
            
            // Check if file exists and has content
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Audio file not found: {filePath}");
            }
            
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                throw new InvalidOperationException($"Audio file is empty: {filePath}");
            }
            
            Logger.Debug($"Audio file size: {fileInfo.Length} bytes");
            
            _audioProvider = new AudioFileReader(filePath);
            _audioOutput = new WaveOutEvent();
            _audioOutput.Init(_audioProvider);
            
            _audioOutput.PlaybackStopped += (s, e) => 
            {
                Logger.Debug($"Playback stopped. Exception: {e?.Exception?.Message ?? "None"}, Manual: {_isManualStop}");
                if (e?.Exception != null)
                {
                    Logger.Error(e.Exception, "Playback stopped due to exception");
                }
                
                // Only fire PlaybackCompleted if this was NOT a manual stop
                if (!_isManualStop)
                {
                    Logger.Debug("Natural playback completion - firing PlaybackCompleted event");
                    PlaybackCompleted?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Logger.Debug("Manual stop - NOT firing PlaybackCompleted event");
                }
                
                // Reset the flag
                _isManualStop = false;
            };
            
            _audioOutput.Play();
            _progressTimer?.Start();
            
            PlaybackStateChanged?.Invoke(this, StreamingPlayerNET.Common.Models.PlaybackState.Playing);
            stopwatch.Stop();
            Logger.Info($"File playback started: {filePath} in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, $"Failed to play file: {filePath} after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            throw;
        }
    }
    
    public async Task PlayAsync(Stream audioStream, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Info("Starting playback for audio stream");
        
        try
        {
            StopForNewSong(); // Stop any current playback (manual stop)
            
            // Create a temporary file for the stream
            var tempFile = Path.GetTempFileName();
            await using (var fileStream = File.Create(tempFile))
            {
                await audioStream.CopyToAsync(fileStream, cancellationToken);
            }
            
            await PlayAsync(tempFile, cancellationToken);
            _currentTempFile = tempFile;
            stopwatch.Stop();
            Logger.Info($"Stream playback setup completed in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, $"Failed to play audio stream after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            throw;
        }
    }
    
    private async Task<string> DownloadDirectlyAsync(string url, CancellationToken cancellationToken = default)
    {
        Logger.Debug("Downloading file directly");
        
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        
        var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var tempFile = Path.GetTempFileName();
        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
        
        await contentStream.CopyToAsync(fileStream, cancellationToken);
        _currentTempFile = tempFile;
        
        return tempFile;
    }
    
    public void Pause()
    {
        if (_audioOutput?.PlaybackState == NAudio.Wave.PlaybackState.Playing)
        {
            Logger.Info("Pausing playback");
            _audioOutput.Pause();
            _progressTimer?.Stop();
            PlaybackStateChanged?.Invoke(this, StreamingPlayerNET.Common.Models.PlaybackState.Paused);
        }
    }
    
    public void Resume()
    {
        if (_audioOutput?.PlaybackState == NAudio.Wave.PlaybackState.Paused)
        {
            Logger.Info("Resuming playback");
            _audioOutput.Play();
            _progressTimer?.Start();
            PlaybackStateChanged?.Invoke(this, StreamingPlayerNET.Common.Models.PlaybackState.Playing);
        }
    }
    
    public void Stop()
    {
        Logger.Info("Stopping playback");
        
        _progressTimer?.Stop();
        _audioOutput?.Stop();
        _audioOutput?.Dispose();
        
        if (_audioProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
        
        // Clean up temp file
        if (!string.IsNullOrEmpty(_currentTempFile) && File.Exists(_currentTempFile))
        {
            try
            {
                File.Delete(_currentTempFile);
                Logger.Debug($"Cleaned up temp file: {_currentTempFile}");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to delete temp file: {_currentTempFile}");
            }
            _currentTempFile = null;
        }
        
        _audioOutput = null;
        _audioProvider = null;
        
        PlaybackStateChanged?.Invoke(this, StreamingPlayerNET.Common.Models.PlaybackState.Stopped);
    }
    
    private void StopForNewSong()
    {
        Logger.Debug("Stopping current playback for new song (manual stop)");
        _isManualStop = true;
        Stop();
    }
    
    public void SetVolume(float volume)
    {
        if (_audioOutput != null)
        {
            _audioOutput.Volume = Math.Clamp(volume, 0.0f, 1.0f);
            Logger.Debug($"Volume set to: {_audioOutput.Volume:P}");
        }
    }
    
    public void SetPosition(TimeSpan position)
    {
        if (_audioProvider is AudioFileReader audioFile)
        {
            audioFile.CurrentTime = position;
            Logger.Debug($"Position set to: {position}");
        }
    }
    
    public StreamingPlayerNET.Common.Models.PlaybackState GetPlaybackState()
    {
        return _audioOutput?.PlaybackState switch
        {
            NAudio.Wave.PlaybackState.Playing => StreamingPlayerNET.Common.Models.PlaybackState.Playing,
            NAudio.Wave.PlaybackState.Paused => StreamingPlayerNET.Common.Models.PlaybackState.Paused,
            NAudio.Wave.PlaybackState.Stopped => StreamingPlayerNET.Common.Models.PlaybackState.Stopped,
            _ => StreamingPlayerNET.Common.Models.PlaybackState.Stopped
        };
    }
    
    public TimeSpan GetCurrentPosition()
    {
        if (_audioProvider is AudioFileReader audioFile)
        {
            return audioFile.CurrentTime;
        }
        return TimeSpan.Zero;
    }
    
    public TimeSpan? GetTotalDuration()
    {
        if (_audioProvider is AudioFileReader audioFile)
        {
            return audioFile.TotalTime;
        }
        return null;
    }
    
    public float GetVolume()
    {
        return _audioOutput?.Volume ?? 1.0f;
    }
    
    public void Dispose()
    {
        Stop();
        _progressTimer?.Dispose();
    }
} 