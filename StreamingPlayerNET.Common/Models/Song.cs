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