using StreamingPlayerNET.Common.Models;

namespace StreamingPlayerNET.Services;

public interface IPlaybackService
{
    event EventHandler<PlaybackState>? PlaybackStateChanged;
    event EventHandler<TimeSpan>? PositionChanged;
    event EventHandler? PlaybackCompleted;
    
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
} 