using System.Diagnostics;

namespace StreamingPlayerNET.Common.Models;

/// <summary>
/// Represents a song in the playback queue with additional queue-specific properties
/// </summary>
public class QueueSong : Song
{
    /// <summary>
    /// The position in the song where playback was last stopped (for resuming mid-track)
    /// </summary>
    public TimeSpan SavedPosition { get; set; }
    
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
    
    /// <summary>
    /// Creates a QueueSong from a regular Song
    /// </summary>
    public static QueueSong FromSong(Song song)
    {
        return new QueueSong
        {
            Id = song.Id,
            Title = song.Title,
            Artist = song.Artist,
            ChannelTitle = song.ChannelTitle,
            Album = song.Album,
            PlaylistName = song.PlaylistName,
            Url = song.Url,
            Duration = song.Duration,
            ThumbnailUrl = song.ThumbnailUrl,
            Description = song.Description,
            UploadDate = song.UploadDate,
            ViewCount = song.ViewCount,
            LikeCount = song.LikeCount,
            Source = song.Source,
            State = song.State,
            CurrentPosition = song.CurrentPosition,
            Volume = song.Volume,
            SelectedStream = song.SelectedStream
        };
    }
} 