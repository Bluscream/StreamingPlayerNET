using System;
using System.Threading;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Services;
using NLog;
using Humanizer;
using static StreamingPlayerNET.Common.Models.PlaybackState;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private async Task PlaySong(Song song)
    {
        var playId = Guid.NewGuid().ToString("N")[..8];
        Logger.Info($"[Play-{playId}] *** USER REQUESTED TO PLAY SONG: {song.Title} by {song.Artist}, Thread: {Thread.CurrentThread.ManagedThreadId}");
        
        try
        {
            Logger.Info($"[Play-{playId}] Loading: {song.Title}...");
            
            // Add song to queue if not already there
            if (!_queue.Songs.Contains(song))
            {
                Logger.Debug($"[Play-{playId}] Adding song to queue: {song.Title}");
                _queue.AddSong(song);
            }
            else
            {
                Logger.Debug($"[Play-{playId}] Song already in queue: {song.Title}");
            }
            
            // Set current index to this song
            var songIndex = _queue.Songs.IndexOf(song);
            Logger.Debug($"[Play-{playId}] Song index in queue: {songIndex}, Current queue index: {_queue.CurrentIndex}");
            if (songIndex >= 0)
            {
                Logger.Debug($"[Play-{playId}] Moving queue to index {songIndex}");
                _queue.MoveToIndex(songIndex);
            }
            
            // Start progress timer for loading state
            _progressTimer?.Start();
            
            Logger.Info($"[Play-{playId}] Calling MusicPlayerService.PlaySongAsync for: {song.Title}");
            await _musicPlayerService.PlaySongAsync(song);
            
            Logger.Info($"[Play-{playId}] *** SUCCESSFULLY STARTED PLAYING: {song.Title}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"[Play-{playId}] *** FAILED TO PLAY SONG: {song.Title}");
            MessageBox.Show($"Failed to play song: {ex.Message}\n\nTry selecting a different song.", "Playback Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
        }
    }

    private void OnPlayPauseButtonClick()
    {
        if (_musicPlayerService.IsPlaying)
        {
            Logger.Info("Pausing playback");
            _musicPlayerService.Pause();
        }
        else if (_musicPlayerService.IsPaused)
        {
            Logger.Info("Resuming paused playback");
            _musicPlayerService.Resume();
        }
        else if (searchListView.SelectedItems.Count > 0)
        {
            var selectedItem = searchListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                _ = PlaySong(song);
            }
        }
        else
        {
            Logger.Debug("Play/Pause button clicked but no item selected");
        }
    }

    private void OnSongChanged(object? sender, Song song)
    {
        var eventId = Guid.NewGuid().ToString("N")[..8];
        Logger.Info($"[{eventId}] OnSongChanged triggered - Song: {song.Title} by {song.Artist}, Thread: {Thread.CurrentThread.ManagedThreadId}, InvokeRequired: {InvokeRequired}");
        
        // Update UI on UI thread
        if (InvokeRequired)
        {
            Logger.Debug($"[{eventId}] Marshalling to UI thread via SafeInvoke");
            SafeInvoke(() => OnSongChanged(sender, song));
            return;
        }
        
        Logger.Debug($"[{eventId}] Processing on UI thread {Thread.CurrentThread.ManagedThreadId}");
        
        // Use FormatStatusString to format the current song label
        var config = ConfigurationService.Current;
        currentSongLabel.Text = FormatStatusString(config.StatusStringFormat, song, _musicPlayerService.GetPlaybackState());
        
        // Hide download progress when song starts playing
        HideDownloadProgress();
        
        // Reset progress for new song
        seekBar.Value = 0;
        
        // Set seek bar to normal colors for new song
        seekBar.BackColor = SystemColors.Control;
        seekBar.ForeColor = SystemColors.Highlight;
        
        // Update window title
        UpdateWindowTitle(song, _musicPlayerService.GetPlaybackState());
        
        // Show toast notification for track change
        Logger.Debug($"[{eventId}] About to call ShowTrackChangeNotification");
        ShowTrackChangeNotification(song);
        Logger.Debug($"[{eventId}] Finished calling ShowTrackChangeNotification");
        
        // Log current song timing if available
        var currentTime = song.GetCurrentSongTime();
        if (currentTime.HasValue)
        {
            Logger.Info($"Current song time for {song.Title}: {currentTime.Value.TotalMilliseconds.Milliseconds()}");
        }
    }

    private void OnPlaybackStateChanged(object? sender, StreamingPlayerNET.Common.Models.PlaybackState state)
    {
        Logger.Debug($"Playback state changed to: {state}");
        
        if (InvokeRequired)
        {
            SafeInvoke(() => OnPlaybackStateChanged(sender, state));
            return;
        }
        
        switch (state)
        {
            case StreamingPlayerNET.Common.Models.PlaybackState.Playing:
                _progressTimer?.Start();
                playPauseButton.Text = "⏸";
                // Set seek bar to normal color when playing
                seekBar.BackColor = SystemColors.Control;
                seekBar.ForeColor = SystemColors.Highlight;
                break;
            case StreamingPlayerNET.Common.Models.PlaybackState.Paused:
            case StreamingPlayerNET.Common.Models.PlaybackState.Stopped:
                _progressTimer?.Stop();
                playPauseButton.Text = "▶";
                // Set seek bar to muted color when paused/stopped
                seekBar.BackColor = Color.LightGray;
                seekBar.ForeColor = Color.Gray;
                break;
        }
        
        // Update window title
        var currentSong = _musicPlayerService.GetCurrentSong();
        UpdateWindowTitle(currentSong, state);
    }

    private void OnPositionChanged(object? sender, TimeSpan position)
    {
        // This is called frequently, so we don't log it
        // The progress timer handles UI updates
    }

    private async void OnPlaybackCompleted(object? sender, EventArgs e)
    {
        var completedId = Guid.NewGuid().ToString("N")[..8];
        Logger.Info($"[Completed-{completedId}] *** PLAYBACK COMPLETED EVENT TRIGGERED ***");
        
        if (InvokeRequired)
        {
            Logger.Debug($"[Completed-{completedId}] Marshalling to UI thread");
            SafeInvoke(() => OnPlaybackCompleted(sender, e));
            return;
        }
        
        var currentSong = _musicPlayerService.GetCurrentSong();
        var currentPosition = _musicPlayerService.GetCurrentPosition();
        var totalDuration = _musicPlayerService.GetTotalDuration();
        
        Logger.Info($"[Completed-{completedId}] Current song: {currentSong?.Title ?? "None"}");
        Logger.Info($"[Completed-{completedId}] Position: {currentPosition} / {totalDuration}");
        Logger.Info($"[Completed-{completedId}] Manually stopped: {_musicPlayerService.WasManuallyStopped}");
        Logger.Info($"[Completed-{completedId}] Repeat mode: {_queue.RepeatMode}");
        
        seekBar.Value = 0;
        timingLabel.Text = "00:00 / 00:00";
        HideDownloadProgress();
        
        // If playback was manually stopped, don't auto-play next song
        if (_musicPlayerService.WasManuallyStopped)
        {
            Logger.Info($"[Completed-{completedId}] SKIPPING auto-play - manually stopped");
            UpdateWindowTitle(null, StreamingPlayerNET.Common.Models.PlaybackState.Stopped);
            return;
        }
        
        // SAFETY CHECK: Prevent cascade if song completed too quickly (likely indicates an issue)
        if (totalDuration.HasValue && totalDuration.Value.TotalSeconds > 0)
        {
            var playedPercentage = (currentPosition.TotalSeconds / totalDuration.Value.TotalSeconds) * 100;
            if (playedPercentage < 5.0) // Song completed before playing 5% - suspicious
            {
                Logger.Warn($"[Completed-{completedId}] *** SUSPICIOUS COMPLETION *** Song completed after only {playedPercentage:F1}% played - NOT auto-playing next song to prevent cascade");
                UpdateWindowTitle(null, StreamingPlayerNET.Common.Models.PlaybackState.Stopped);
                return;
            }
            Logger.Info($"[Completed-{completedId}] Song naturally completed at {playedPercentage:F1}%");
        }
        else
        {
            Logger.Warn($"[Completed-{completedId}] *** NO POSITION/DURATION INFO *** - NOT auto-playing next song to prevent cascade");
            UpdateWindowTitle(null, StreamingPlayerNET.Common.Models.PlaybackState.Stopped);
            return;
        }
        
        // Handle repeat modes for natural song completion
        if (_queue.RepeatMode == RepeatMode.One)
        {
            // Repeat the same song
            var repeatSong = _queue.CurrentSong;
            if (repeatSong != null)
            {
                Logger.Info($"[Completed-{completedId}] REPEATING current song: {repeatSong.Title}");
                await PlaySong(repeatSong);
                return;
            }
        }
        else if (_queue.RepeatMode == RepeatMode.All && _queue.HasNext)
        {
            // Move to next song (will loop back to beginning if at end)
            Logger.Info($"[Completed-{completedId}] AUTO-PLAYING next song (Repeat All mode)");
            _queue.MoveToNext();
            var nextSong = _queue.CurrentSong;
            if (nextSong != null)
            {
                Logger.Info($"[Completed-{completedId}] Next song: {nextSong.Title}");
                await PlaySong(nextSong);
                return;
            }
        }
        else
        {
            Logger.Info($"[Completed-{completedId}] NOT auto-playing next song - RepeatMode: {_queue.RepeatMode}, HasNext: {_queue.HasNext}");
        }
        
        // Clear window title
        UpdateWindowTitle(null, StreamingPlayerNET.Common.Models.PlaybackState.Stopped);
        
        Logger.Info($"[Completed-{completedId}] Playback completed handling finished");
    }

    private async Task PlayNextSong()
    {
        if (_queue.HasNext)
        {
            _queue.MoveToNext();
            var nextSong = _queue.CurrentSong;
            if (nextSong != null)
            {
                await PlaySong(nextSong);
            }
        }
        else
        {
            Logger.Info("No more songs in queue");
        }
    }

    private async Task PlayPreviousSong()
    {
        if (_queue.HasPrevious)
        {
            _queue.MoveToPrevious();
            var previousSong = _queue.CurrentSong;
            if (previousSong != null)
            {
                await PlaySong(previousSong);
            }
        }
        else
        {
            // If no previous song, restart current song from beginning
            var currentSong = _queue.CurrentSong ?? _musicPlayerService.GetCurrentSong();
            if (currentSong != null)
            {
                Logger.Info("No previous songs in queue - restarting current song from beginning");
                await PlaySong(currentSong);
            }
            else
            {
                Logger.Info("No previous songs in queue and no current song");
            }
        }
    }

    private void OnRepeatButtonClick()
    {
        _queue.ToggleRepeatMode();
        Logger.Info($"Repeat Mode: {_queue.GetRepeatModeText()}");
    }

    private void OnShuffleButtonClick()
    {
        _queue.ToggleShuffle();
        Logger.Info($"Shuffle: {_queue.GetShuffleText()}");
    }

    private void OnQueueRepeatModeChanged(object? sender, RepeatMode repeatMode)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => OnQueueRepeatModeChanged(sender, repeatMode));
            return;
        }
        
        // Update button appearance based on repeat mode
        repeatButton.Text = repeatMode switch
        {
            RepeatMode.None => "🔁",
            RepeatMode.One => "🔂",
            RepeatMode.All => "🔁",
            _ => "🔁"
        };
        repeatButton.BackColor = repeatMode switch
        {
            RepeatMode.None => SystemColors.Control,
            RepeatMode.One => Color.LightGreen,
            RepeatMode.All => Color.LightBlue,
            _ => SystemColors.Control
        };
        
        // Update menu item text
        repeatMenuItem.Text = $"&Repeat Mode ({repeatMode})";
        
        // Save queue cache when repeat mode changes
        SaveCachedQueue();
        
        Logger.Debug($"Repeat mode changed to: {repeatMode}");
    }

    private void OnQueueShuffleChanged(object? sender, bool shuffleEnabled)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => OnQueueShuffleChanged(sender, shuffleEnabled));
            return;
        }
        
        // Update button appearance based on shuffle state
        shuffleButton.Text = shuffleEnabled ? "🔀" : "🔀";
        shuffleButton.BackColor = shuffleEnabled ? Color.LightBlue : SystemColors.Control;
        
        // Update menu item text
        shuffleMenuItem.Text = shuffleEnabled ? "&Shuffle (On)" : "&Shuffle (Off)";
        
        // Save queue cache when shuffle state changes
        SaveCachedQueue();
        
        Logger.Debug($"Shuffle changed to: {shuffleEnabled}");
    }

    private void AdjustVolume(int delta)
    {
        var newValue = Math.Max(0, Math.Min(100, volumeTrackBar.Value + delta));
        volumeTrackBar.Value = newValue;
        var volume = newValue / 100f;
        _musicPlayerService.SetVolume(volume);
        volumeLabel.Text = $"{newValue}%";
        Logger.Debug($"Volume adjusted to: {newValue}%");
    }

    private void HideDownloadProgress()
    {
        if (InvokeRequired)
        {
            SafeInvoke(HideDownloadProgress);
            return;
        }
        
        downloadProgressBar.Visible = false;
        downloadProgressBar.Value = 0;
        
        // Reset status label if it's showing download info
        if (statusLabel.Text.Contains("Download", StringComparison.OrdinalIgnoreCase))
        {
            statusLabel.Text = "Ready";
        }
    }

    private void ShowTrackChangeNotification(Song song)
    {
        try
        {
            // Show toast notification on a background thread to avoid blocking UI
            Task.Run(() =>
            {
                _toastNotificationService?.ShowTrackChangeNotification(song);
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error showing track change notification");
        }
    }
}