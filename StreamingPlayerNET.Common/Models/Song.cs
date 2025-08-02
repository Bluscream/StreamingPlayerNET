using System.Diagnostics;

namespace StreamingPlayerNET.Common.Models;

public class Song
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string ChannelTitle { get; set; } = string.Empty;
    public string? Album { get; set; }
    public string? PlaylistName { get; set; }
    public string? Url { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
    public DateTime? UploadDate { get; set; }
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public string? Source { get; set; }
    
    // Playback state
    public float Volume { get; set; } = 1.0f;
    
    // Performance tracking - single Stopwatch for entire song lifecycle
    public Stopwatch? SongStopwatch { get; private set; }
    
    // Audio stream information
    public AudioStreamInfo? SelectedStream { get; set; }
    
    // Download information for download events
    public DownloadInfo? DownloadInfo { get; set; }
    
    // Queue-specific properties (moved from QueueSong)
    /// <summary>
    /// The position in the song where playback was last stopped (for resuming mid-track)
    /// </summary>
    public TimeSpan SavedPosition { get; set; }
    
    /// <summary>
    /// The current playback position
    /// </summary>
    public TimeSpan CurrentPosition { get; set; }
    
    /// <summary>
    /// The current playback state
    /// </summary>
    public PlaybackState State { get; set; } = PlaybackState.Stopped;
    
    /// <summary>
    /// Whether the song was playing when it was paused/stopped
    /// </summary>
    public bool WasPlaying { get; set; }
    
    /// <summary>
    /// Timestamp when the song was added to the queue
    /// </summary>
    public DateTime AddedToQueueAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Timestamp when the song started playing
    /// </summary>
    public DateTime? StartedPlayingAt { get; set; }
    
    /// <summary>
    /// Timestamp when the song was paused or stopped
    /// </summary>
    public DateTime? PausedAt { get; set; }
    
    /// <summary>
    /// Total time the song has been played (excluding pauses)
    /// </summary>
    public TimeSpan TotalPlayTime { get; set; }
    
    public void StartSongTimer()
    {
        SongStopwatch = Stopwatch.StartNew();
    }
    
    public void StopSongTimer()
    {
        SongStopwatch?.Stop();
    }
    
    public TimeSpan? GetCurrentSongTime()
    {
        return SongStopwatch?.Elapsed;
    }
    
    /// <summary>
    /// Saves the current playback position for later resumption
    /// </summary>
    public void SaveCurrentPosition()
    {
        if (SongStopwatch?.IsRunning == true)
        {
            SavedPosition = SongStopwatch.Elapsed;
            WasPlaying = State == PlaybackState.Playing;
            PausedAt = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Restores the saved position and continues playback from where it left off
    /// </summary>
    public void RestorePosition()
    {
        if (WasPlaying && SavedPosition > TimeSpan.Zero)
        {
            CurrentPosition = SavedPosition;
            StartSongTimer();
            // Advance the stopwatch to the saved position
            if (SongStopwatch != null)
            {
                SongStopwatch.Restart();
                // Note: We can't directly set the elapsed time, so we'll track it separately
            }
        }
    }
    
    /// <summary>
    /// Records the start of playback
    /// </summary>
    public void RecordPlaybackStart()
    {
        StartedPlayingAt = DateTime.Now;
        StartSongTimer();
    }
    
    /// <summary>
    /// Records when playback is paused or stopped
    /// </summary>
    public void RecordPlaybackPause()
    {
        if (SongStopwatch?.IsRunning == true)
        {
            TotalPlayTime += SongStopwatch.Elapsed;
            PausedAt = DateTime.Now;
            StopSongTimer();
        }
    }
    
    /// <summary>
    /// Gets the total time this song has been in the queue
    /// </summary>
    public TimeSpan GetTimeInQueue()
    {
        return DateTime.Now - AddedToQueueAt;
    }
    
    /// <summary>
    /// Gets the total time this song has been actively playing
    /// </summary>
    public TimeSpan GetTotalPlayTime()
    {
        var currentPlayTime = SongStopwatch?.IsRunning == true ? SongStopwatch.Elapsed : TimeSpan.Zero;
        return TotalPlayTime + currentPlayTime;
    }
    
    public override string ToString()
    {
        return $"{Title} - {Artist}";
    }
}

public enum PlaybackState
{
    Stopped,
    Playing,
    Paused,
    Loading
}