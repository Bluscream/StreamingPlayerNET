using StreamingPlayerNET.Common.Models;

namespace StreamingPlayerNET.Services;

public interface IPlaybackService
{
    event EventHandler<PlaybackState>? PlaybackStateChanged;
    event EventHandler<TimeSpan>? PositionChanged;
    event EventHandler? PlaybackCompleted;
    event EventHandler<PlaybackErrorEventArgs>? PlaybackError;
    
    Task PlayAsync(Song song, CancellationToken cancellationToken = default);
    Task PlayAsync(AudioStreamInfo streamInfo, CancellationToken cancellationToken = default);
    Task PlayAsync(string filePath, CancellationToken cancellationToken = default);
    Task PlayAsync(Stream audioStream, CancellationToken cancellationToken = default);
    
    void Pause();
    void Resume();
    void Stop();
    void SetVolume(float volume);
    void SetPosition(TimeSpan position);
    
    PlaybackState GetPlaybackState();
    TimeSpan GetCurrentPosition();
    TimeSpan? GetTotalDuration();
    float GetVolume();
    
    bool IsPlaying { get; }
    bool IsPaused { get; }
    bool IsStopped { get; }
    
    CachingService? GetCachingService();
}

public class PlaybackErrorEventArgs : EventArgs
{
    public string FilePath { get; }
    public Exception Exception { get; }
    public Song? Song { get; }
    
    public PlaybackErrorEventArgs(string filePath, Exception exception, Song? song = null)
    {
        FilePath = filePath;
        Exception = exception;
        Song = song;
    }
} 