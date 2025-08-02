using System;
using System.Threading;
using NLog;
using StreamingPlayerNET.Common.Models;
using Microsoft.Toolkit.Uwp.Notifications;

namespace StreamingPlayerNET.Services;

public class ToastNotificationService : IDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private bool _disposed = false;
    private Song? _lastNotifiedSong = null;
    private readonly object _notificationLock = new object();

    public void ShowTrackChangeNotification(Song song)
    {
        var callId = Guid.NewGuid().ToString("N")[..8];
        var threadId = Thread.CurrentThread.ManagedThreadId;
        
        Logger.Info($"[Toast-{callId}] ShowTrackChangeNotification called on thread {threadId} for: {song.Title} by {song.Artist}");
        
        try
        {
            // Use lock to prevent multiple simultaneous notifications
            Logger.Debug($"[Toast-{callId}] Attempting to acquire notification lock...");
            lock (_notificationLock)
            {
                Logger.Debug($"[Toast-{callId}] Lock acquired on thread {threadId}");
                
                // Check if this is actually a different song from the last one we notified about
                if (_lastNotifiedSong != null)
                {
                    var isSame = IsSameSong(_lastNotifiedSong, song);
                    Logger.Debug($"[Toast-{callId}] Comparing with last notified song: '{_lastNotifiedSong.Title}' by '{_lastNotifiedSong.Artist}' - Same: {isSame}");
                    
                    if (isSame)
                    {
                        Logger.Info($"[Toast-{callId}] SKIPPING notification - same song: {song.Title} - {song.Artist}");
                        return;
                    }
                }
                else
                {
                    Logger.Debug($"[Toast-{callId}] No previous notification to compare against");
                }

                var title = song.Title ?? "Unknown Title";
                var artist = song.Artist ?? "Unknown Artist";
                var album = song.Album ?? "";

                Logger.Info($"[Toast-{callId}] SHOWING notification for: {title} - {artist}");

                // Create toast notification using the modern toolkit
                var builder = new ToastContentBuilder()
                    .AddText("Now Playing", AdaptiveTextStyle.Header)
                    .AddText(title, AdaptiveTextStyle.Title)
                    .AddText(!string.IsNullOrEmpty(album) ? $"{artist} • {album}" : artist, AdaptiveTextStyle.Subtitle)
                    .SetToastScenario(ToastScenario.Default)
                    .SetToastDuration(ToastDuration.Short)
                    .AddAudio(null, silent: true); // Silent notification
                
                // Add app logo if available
                try
                {
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    using var stream = assembly.GetManifestResourceStream("StreamingPlayerNET.logo.png");
                    if (stream != null)
                    {
                        var tempPath = Path.GetTempFileName() + ".png";
                        using (var fileStream = File.Create(tempPath))
                        {
                            stream.CopyTo(fileStream);
                        }
                        builder.AddAppLogoOverride(new Uri(tempPath), ToastGenericAppLogoCrop.Circle);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Failed to add app logo to notification");
                }
                
                builder.Show(); // Show the toast
                
                // Update the last notified song
                _lastNotifiedSong = song;
                
                Logger.Info($"[Toast-{callId}] SUCCESS - Windows toast notification shown for: {title} - {artist}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"[Toast-{callId}] ERROR showing track change notification");
        }
        finally
        {
            Logger.Debug($"[Toast-{callId}] ShowTrackChangeNotification completed");
        }
    }

    /// <summary>
    /// Compares two songs to determine if they represent the same track
    /// </summary>
    /// <param name="song1">First song to compare</param>
    /// <param name="song2">Second song to compare</param>
    /// <returns>True if the songs are considered the same track</returns>
    private bool IsSameSong(Song song1, Song song2)
    {
        // Compare title, artist, and album (case-insensitive)
        var isSameTitle = string.Equals(song1.Title?.Trim(), song2.Title?.Trim(), StringComparison.OrdinalIgnoreCase);
        var isSameArtist = string.Equals(
            song1.Artist?.Trim(),
            song2.Artist?.Trim(),
            StringComparison.OrdinalIgnoreCase
        );
        var isSameAlbum = string.Equals(song1.Album?.Trim(), song2.Album?.Trim(), StringComparison.OrdinalIgnoreCase);
        
        // Songs are considered the same if title and artist match (album is optional)
        return isSameTitle && isSameArtist && isSameAlbum;
    }

    public void ShowGenericNotification(string title, string message)
    {
        try
        {
            // Create toast notification using the modern toolkit
            new ToastContentBuilder()
                .AddText(title, AdaptiveTextStyle.Header)
                .AddText(message, AdaptiveTextStyle.Body)
                .SetToastScenario(ToastScenario.Default)
                .SetToastDuration(ToastDuration.Short)
                .AddAudio(null, silent: true) // Silent notification
                .Show(); // Show the toast
            
            Logger.Debug($"Showed generic notification: {title}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error showing generic notification");
        }
    }

    /// <summary>
    /// Test method to verify the notification system is working
    /// </summary>
    public void ShowTestNotification()
    {
        try
        {
            var testSong = new Song
            {
                Title = "Test Track",
                Artist = "Test Artist",
                Album = "Test Album"
            };
            
            ShowTrackChangeNotification(testSong);
            Logger.Info("Test notification displayed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to show test notification");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Toast notifications are managed by Windows, no cleanup needed
            _lastNotifiedSong = null;
            _disposed = true;
        }
    }
} 