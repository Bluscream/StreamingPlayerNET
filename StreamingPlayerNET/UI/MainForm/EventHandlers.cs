using System;
using System.Threading;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Services;
using NLog;
using static StreamingPlayerNET.Common.Models.PlaybackState;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private void SetupEventHandlers()
    {
        // Search events
        searchButton.Click += async (s, e) => await PerformSearch();
        searchTextBox.KeyPress += async (s, e) => 
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                await PerformSearch();
            }
        };
        
        // Playback control events
        playPauseButton.Click += (s, e) => OnPlayPauseButtonClick();
        stopButton.Click += (s, e) => _musicPlayerService?.Stop();
        previousButton.Click += async (s, e) => await PlayPreviousSong();
        nextButton.Click += async (s, e) => await PlayNextSong();
        repeatButton.Click += (s, e) => OnRepeatButtonClick();
        shuffleButton.Click += (s, e) => OnShuffleButtonClick();
        
        // Volume control
        volumeTrackBar.Scroll += (s, e) => 
        {
            var volume = volumeTrackBar.Value / 100f;
            _musicPlayerService?.SetVolume(volume);
            volumeLabel.Text = $"{volumeTrackBar.Value}%";
        };
        
        // Search tab events
        searchListView.DoubleClick += async (s, e) => await OnSearchResultDoubleClick();
        SetupSearchContextMenu();
        
        // Queue tab events
        queueListView.DoubleClick += async (s, e) => await OnQueueItemDoubleClick();
        SetupQueueContextMenu();
        
        // Playlist tab events
        playlistsListBox.DoubleClick += (s, e) => OnPlaylistDoubleClick();
        playlistListView.DoubleClick += async (s, e) => await OnPlaylistItemDoubleClick();
        SetupPlaylistContextMenu();
        

        
        // Seekbar click event
        seekBar.MouseClick += SeekBar_MouseClick;
        
        // Time label click events
        remainingTimeLabel.Click += OnRemainingTimeLabelClick;
        
        // Menu events
        SetupMenuEventHandlers();
        
        // Keyboard shortcuts
        this.KeyPreview = true;
        this.KeyDown += Form1_KeyDown;
        
        // Form and control resize events
        this.Resize += OnFormResize;
        searchListView.Resize += OnSearchListViewResize;
        queueListView.Resize += OnQueueListViewResize;
        playlistListView.Resize += OnPlaylistListViewResize;

    }

    private void SetupDataBinding()
    {
        // Subscribe to queue changes to update the display
        _queue.OnSongsChanged += () => SafeInvoke(UpdateQueueDisplay);
    }

    private void SetupProgressTimer()
    {
        _progressTimer = new System.Windows.Forms.Timer();
        _progressTimer.Interval = 1000; // Update every second
        _progressTimer.Tick += ProgressTimer_Tick;
    }

    private void ProgressTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            var currentSong = _musicPlayerService?.GetCurrentSong();
            if (currentSong != null && (_musicPlayerService?.IsPlaying ?? false))
            {
                var position = _musicPlayerService?.GetCurrentPosition() ?? TimeSpan.Zero;
                var duration = _musicPlayerService?.GetTotalDuration();
                
                if (duration.HasValue && duration.Value.TotalSeconds > 0)
                {
                    var progress = (int)((position.TotalSeconds / duration.Value.TotalSeconds) * 100);
                    seekBar.Value = Math.Min(progress, 100);
                    
                    // Update time labels
                    elapsedTimeLabel.Text = $"{position:mm\\:ss}";
                    
                    // Update remaining time label based on configuration
                    var config = ConfigurationService.Current;
                    if (config.ShowRemainingTime)
                    {
                        remainingTimeLabel.Text = $"-{duration.Value - position:mm\\:ss}";
                    }
                    else
                    {
                        remainingTimeLabel.Text = $"{duration.Value:mm\\:ss}";
                    }
                    
                    // Update status strip timing
                    timingLabel.Text = $"{position:mm\\:ss} / {duration.Value:mm\\:ss}";
                }
            }
            else
            {
                seekBar.Value = 0;
                elapsedTimeLabel.Text = "00:00";
                remainingTimeLabel.Text = "00:00";
                timingLabel.Text = "00:00 / 00:00";
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error updating progress bar");
        }
    }

    private void SeekBar_MouseClick(object? sender, MouseEventArgs e)
    {
        try
        {
            if (seekBar.Width <= 0) return;
            
            // Calculate the percentage clicked
            var clickPercentage = (double)e.X / seekBar.Width;
            var duration = _musicPlayerService?.GetTotalDuration();
            
            if (duration.HasValue && duration.Value.TotalSeconds > 0)
            {
                // Calculate new position
                var newPosition = TimeSpan.FromSeconds(duration.Value.TotalSeconds * clickPercentage);
                
                // Set the new position
                _musicPlayerService?.SetPosition(newPosition);
                
                Logger.Debug($"Seeked to {newPosition:mm\\:ss} ({clickPercentage:P0} of song)");
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error during seek operation");
        }
    }

    private void OnRemainingTimeLabelClick(object? sender, EventArgs e)
    {
        try
        {
            var config = ConfigurationService.Current;
            
            // Toggle the setting
            config.ShowRemainingTime = !config.ShowRemainingTime;
            
            // Save the configuration
            ConfigurationService.SaveConfiguration();
            
            // Update the display immediately if a song is playing
            var currentSong = _musicPlayerService?.GetCurrentSong();
            if (currentSong != null && (_musicPlayerService?.IsPlaying ?? false))
            {
                var position = _musicPlayerService?.GetCurrentPosition() ?? TimeSpan.Zero;
                var duration = _musicPlayerService?.GetTotalDuration();
                
                if (duration.HasValue && duration.Value.TotalSeconds > 0)
                {
                    if (config.ShowRemainingTime)
                    {
                        remainingTimeLabel.Text = $"{duration.Value - position:mm\\:ss}";
                    }
                    else
                    {
                        remainingTimeLabel.Text = $"{duration.Value:mm\\:ss}";
                    }
                }
            }
            
            Logger.Debug($"Time display mode toggled to: {(config.ShowRemainingTime ? "Remaining" : "Total")}");
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error toggling time display mode");
        }
    }

    private async Task OnSearchResultDoubleClick()
    {
        if (searchListView.SelectedItems.Count > 0)
        {
            var selectedItem = searchListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                await PlaySong(song);
            }
        }
    }

    private async Task OnQueueItemDoubleClick()
    {
        var clickId = Guid.NewGuid().ToString("N")[..8];
        Logger.Info($"[QueueClick-{clickId}] *** QUEUE ITEM DOUBLE-CLICKED, Thread: {Thread.CurrentThread.ManagedThreadId}");
        
        if (queueListView.SelectedItems.Count > 0)
        {
            var selectedItem = queueListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                Logger.Info($"[QueueClick-{clickId}] Selected song from queue: {song.Title} by {song.Artist}");
                await PlaySong(song);
                Logger.Debug($"[QueueClick-{clickId}] PlaySong call completed");
            }
            else
            {
                Logger.Warn($"[QueueClick-{clickId}] Selected item has no song tag");
            }
        }
        else
        {
            Logger.Warn($"[QueueClick-{clickId}] No items selected in queue");
        }
    }

    private async Task OnPlaylistItemDoubleClick()
    {
        if (playlistListView.SelectedItems.Count > 0)
        {
            var selectedItem = playlistListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                await PlaySong(song);
            }
        }
    }

    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        var config = ConfigurationService.Current;
        
        if (IsHotkeyMatch(e, config.PlayPauseHotkey))
        {
            OnPlayPauseButtonClick();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.StopHotkey))
        {
            _musicPlayerService?.Stop();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.PreviousTrackHotkey))
        {
            _ = PlayPreviousSong();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.NextTrackHotkey))
        {
            _ = PlayNextSong();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.RepeatModeHotkey))
        {
            OnRepeatButtonClick();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.ShuffleHotkey))
        {
            OnShuffleButtonClick();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.VolumeUpHotkey))
        {
            AdjustVolume(10);
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.VolumeDownHotkey))
        {
            AdjustVolume(-10);
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.TogglePlaylistsHotkey))
        {
            TogglePlaylistsVisibility();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.ToggleSearchHotkey))
        {
            ToggleSearchVisibility();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.SettingsHotkey))
        {
            ShowSettings();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.HelpHotkey))
        {
            ShowHelp();
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.ThemeCycleHotkey))
        {
            // Trigger the cycle theme menu item
            var cycleMenuItem = themeMenu.DropDownItems.OfType<ToolStripMenuItem>()
                .FirstOrDefault(item => item.Name == "cycleThemeMenuItem");
            cycleMenuItem?.PerformClick();
            e.Handled = true;
            return;
        }
    }

    private bool IsHotkeyMatch(KeyEventArgs e, KeyBind keyBind)
    {
        if (keyBind == null || !keyBind.Enabled || string.IsNullOrEmpty(keyBind.Combo))
            return false;
            
        var parts = keyBind.Combo.Split('+');
        var keyPart = parts[^1]; // Last part is the key
        var modifiers = parts.Take(parts.Length - 1);
        
        // Parse the key
        if (!Enum.TryParse<Keys>(keyPart, true, out var key))
            return false;
            
        if (e.KeyCode != key)
            return false;
            
        // Check modifiers
        var hasCtrl = modifiers.Contains("Ctrl", StringComparer.OrdinalIgnoreCase);
        var hasAlt = modifiers.Contains("Alt", StringComparer.OrdinalIgnoreCase);
        var hasShift = modifiers.Contains("Shift", StringComparer.OrdinalIgnoreCase);
        
        return e.Control == hasCtrl && e.Alt == hasAlt && e.Shift == hasShift;
    }

    private void SetupMenuEventHandlers()
    {
        // File menu
        reloadPlaylistsMenuItem.Click += async (s, e) => await ReloadPlaylists();
        exitMenuItem.Click += (s, e) => Close();
        
        // Playback menu
        playMenuItem.Click += (s, e) => OnPlayPauseButtonClick();
        pauseMenuItem.Click += (s, e) => OnPlayPauseButtonClick();
        stopMenuItem.Click += (s, e) => _musicPlayerService.Stop();
        nextMenuItem.Click += async (s, e) => await PlayNextSong();
        previousMenuItem.Click += async (s, e) => await PlayPreviousSong();
        volumeUpMenuItem.Click += (s, e) => AdjustVolume(10);
        volumeDownMenuItem.Click += (s, e) => AdjustVolume(-10);
        repeatMenuItem.Click += (s, e) => OnRepeatButtonClick();
        shuffleMenuItem.Click += (s, e) => OnShuffleButtonClick();
        
        // View menu
        showPlaylistsMenuItem.CheckedChanged += (s, e) => TogglePlaylistsVisibility();
        showSearchMenuItem.CheckedChanged += (s, e) => ToggleSearchVisibility();
        
        // Setup theme menu programmatically
        SetupThemeMenu();
        
        // Help menu
        helpMenuItem.Click += (s, e) => ShowHelp();
        aboutMenuItem.Click += (s, e) => ShowAbout();
        
        // Settings menu
        settingsMenu.Click += (s, e) => ShowSettings();
    }

    private void SetupThemeMenu()
    {
        // Clear existing theme menu items
        themeMenu.DropDownItems.Clear();
        
        // Get all theme values from the AppTheme enum
        var themes = Enum.GetValues<AppTheme>();
        
        foreach (var theme in themes)
        {
            var menuItem = new ToolStripMenuItem();
            menuItem.Name = $"{theme.ToString().ToLowerInvariant()}ThemeMenuItem";
            menuItem.Text = GetThemeDisplayName(theme);
            menuItem.Tag = theme;
            menuItem.ToolTipText = GetThemeDescription(theme);
            
            // Add keyboard shortcut for cycling (Ctrl+T cycles through themes)
            if (theme == AppTheme.Light)
            {
                menuItem.ShortcutKeys = Keys.Control | Keys.T;
                menuItem.ShortcutKeyDisplayString = "Ctrl+T";
            }
            
            menuItem.Click += (s, e) => 
            {
                if (s is ToolStripMenuItem item && item.Tag is AppTheme themeValue)
                {
                    SetTheme(themeValue);
                }
            };
            
            themeMenu.DropDownItems.Add(menuItem);
        }
        
        // Add separator and cycle option
        themeMenu.DropDownItems.Add(new ToolStripSeparator());
        
        var cycleMenuItem = new ToolStripMenuItem();
        cycleMenuItem.Name = "cycleThemeMenuItem";
        cycleMenuItem.Text = "&Cycle Theme";
        cycleMenuItem.ShortcutKeys = Keys.Control | Keys.T;
        cycleMenuItem.ShortcutKeyDisplayString = "Ctrl+T";
        cycleMenuItem.ToolTipText = "Cycle through all available themes";
        cycleMenuItem.Click += (s, e) => 
        {
            var config = ConfigurationService.Current;
            var nextTheme = config.Theme switch
            {
                AppTheme.Light => AppTheme.Dark,
                AppTheme.Dark => AppTheme.Black,
                AppTheme.Black => AppTheme.Light,
                _ => AppTheme.Light
            };
            SetTheme(nextTheme);
        };
        
        themeMenu.DropDownItems.Add(cycleMenuItem);
        
        // Update initial theme menu state
        UpdateThemeMenuState();
    }

    private string GetThemeDisplayName(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Light => "&Light Theme",
            AppTheme.Dark => "&Dark Theme", 
            AppTheme.Black => "&Black Theme",
            _ => theme.ToString()
        };
    }

    private string GetThemeDescription(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.Light => "Standard Windows system colors",
            AppTheme.Dark => "Dark gray background with white text",
            AppTheme.Black => "Pure black background with white text",
            _ => "Custom theme"
        };
    }

    private void UpdateThemeMenuState()
    {
        var config = ConfigurationService.Current;
        
        // Update check marks for all theme menu items
        foreach (ToolStripItem item in themeMenu.DropDownItems)
        {
            if (item is ToolStripMenuItem menuItem && menuItem.Tag is AppTheme theme)
            {
                menuItem.Checked = config.Theme == theme;
            }
        }
    }

    private void OnDownloadProgressChanged(object? sender, DownloadProgressEventArgs e)
    {
        // Update UI on the UI thread
        if (InvokeRequired)
        {
            SafeInvoke(() => OnDownloadProgressChanged(sender, e));
            return;
        }
        
        if (e.TotalBytes > 0)
        {
            // Show progress bar and update value
            downloadProgressBar.Visible = true;
            downloadProgressBar.Value = Math.Min((int)e.ProgressPercentage, 100);
            
            // Update status text with just the percentage
            statusLabel.Text = $"{e.ProgressPercentage:F1}%";
        }
        else
        {
            // Handle status messages (starting, completed, etc.)
            if (e.Status.Contains("completed", StringComparison.OrdinalIgnoreCase))
            {
                // Download completed - hide progress bar
                downloadProgressBar.Visible = false;
                downloadProgressBar.Value = 0;
                statusLabel.Text = "Ready";
            }
            else if (e.Status.Contains("Starting", StringComparison.OrdinalIgnoreCase))
            {
                // Download starting
                downloadProgressBar.Visible = true;
                downloadProgressBar.Value = 0;
                statusLabel.Text = "0.0%";
            }
        }
    }
}