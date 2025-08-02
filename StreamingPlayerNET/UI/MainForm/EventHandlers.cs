using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Common.Utils;
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
            volumeLabel.Text = $"🔊 {volumeTrackBar.Value}%";
        };
        
        // Search tab events
        searchListView.DoubleClick += (s, e) => Task.Run(async () => await OnSearchResultDoubleClick());
        SetupSongContextMenu(SongContextMenuType.Search);
        
        // Queue tab events
        queueListView.DoubleClick += (s, e) => Task.Run(async () => await OnQueueItemDoubleClick());
        queueListView.ItemDrag += OnQueueItemDrag;
        queueListView.DragEnter += OnQueueDragEnter;
        queueListView.DragOver += OnQueueDragOver;
        queueListView.DragDrop += OnQueueDragDrop;
        queueListView.KeyDown += OnQueueKeyDown;
        SetupSongContextMenu(SongContextMenuType.Queue);
        
        // Playlist tab events
        playlistsListBox.DoubleClick += (s, e) => OnPlaylistDoubleClick();
        playlistListView.DoubleClick += (s, e) => Task.Run(async () => await OnPlaylistItemDoubleClick());
        SetupSongContextMenu(SongContextMenuType.Playlist);
        

        
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
        
                    // Setup downloads tab
            SetupDownloadsTab();
            
            // Setup logs tab
            SetupLogsTab();
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
            var currentSong = _musicPlayerService?.CurrentSong;
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
            var currentSong = _musicPlayerService?.CurrentSong;
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
        var clickId = Guid.NewGuid().ToString("N")[..8];
        Logger.Info($"[SearchClick-{clickId}] *** SEARCH RESULT DOUBLE-CLICKED, Thread: {Thread.CurrentThread.ManagedThreadId}");
        
        if (searchListView.SelectedItems.Count > 0)
        {
            var selectedItem = searchListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                Logger.Info($"[SearchClick-{clickId}] Selected song from search: {song.Title} by {song.Artist}");
                Logger.Debug($"[SearchClick-{clickId}] About to call PlaySong, Thread: {Thread.CurrentThread.ManagedThreadId}");
                await PlaySong(song);
                Logger.Debug($"[SearchClick-{clickId}] PlaySong call completed");
            }
            else
            {
                Logger.Warn($"[SearchClick-{clickId}] Selected item has no song tag");
            }
        }
        else
        {
            Logger.Warn($"[SearchClick-{clickId}] No items selected in search");
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

    private void OnQueueItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (queueListView.SelectedItems.Count > 0)
        {
            // Create a list of selected songs
            var selectedSongs = new List<Song>();
            foreach (ListViewItem item in queueListView.SelectedItems)
            {
                if (item.Tag is Song song)
                {
                    selectedSongs.Add(song);
                }
            }
            
            if (selectedSongs.Count > 0)
            {
                // Store the selected songs in the drag data
                var dragData = new DataObject();
                dragData.SetData("QueueSongs", selectedSongs);
                dragData.SetData("QueueIndices", queueListView.SelectedIndices.Cast<int>().ToList());
                
                queueListView.DoDragDrop(dragData, DragDropEffects.Move);
            }
        }
    }

    private void OnQueueDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent("QueueSongs") == true)
        {
            e.Effect = DragDropEffects.Move;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void OnQueueDragOver(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent("QueueSongs") == true)
        {
            e.Effect = DragDropEffects.Move;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void OnQueueDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent("QueueSongs") == true)
        {
            var selectedSongs = e.Data.GetData("QueueSongs") as List<Song>;
            var selectedIndices = e.Data.GetData("QueueIndices") as List<int>;
            
            if (selectedSongs != null && selectedIndices != null)
            {
                // Get the drop target index
                var dropPoint = queueListView.PointToClient(new Point(e.X, e.Y));
                var dropItem = queueListView.GetItemAt(dropPoint.X, dropPoint.Y);
                int targetIndex = dropItem?.Index ?? queueListView.Items.Count;
                
                // Remove the songs from their original positions
                var sortedIndices = selectedIndices.OrderByDescending(i => i).ToList();
                foreach (int index in sortedIndices)
                {
                    _queue.RemoveSong(index);
                }
                
                // Insert the songs at the target position
                for (int i = 0; i < selectedSongs.Count; i++)
                {
                    int insertIndex = targetIndex + i;
                    if (insertIndex > _queue.Songs.Count)
                    {
                        insertIndex = _queue.Songs.Count;
                    }
                    _queue.InsertSong(insertIndex, selectedSongs[i]);
                }
                
                // Update the display
                UpdateQueueDisplay();
                
                // Select the moved items
                queueListView.SelectedItems.Clear();
                for (int i = 0; i < selectedSongs.Count; i++)
                {
                    int newIndex = targetIndex + i;
                    if (newIndex < queueListView.Items.Count)
                    {
                        queueListView.Items[newIndex].Selected = true;
                    }
                }
            }
        }
    }

    private void OnQueueKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete && queueListView.SelectedItems.Count > 0)
        {
            e.Handled = true;
            RemoveSelectedSongsFromQueue();
        }
    }

    private void RemoveSelectedSongsFromQueue()
    {
        if (queueListView.SelectedItems.Count == 0) return;
        
        // Get selected indices in descending order to avoid index shifting issues
        var selectedIndices = queueListView.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
        
        // Remove songs from queue
        foreach (int index in selectedIndices)
        {
            if (index >= 0 && index < _queue.Songs.Count)
            {
                _queue.RemoveSong(index);
            }
        }
        
        // Update the display
        UpdateQueueDisplay();
        
        // Show notification
        var count = selectedIndices.Count;
        var message = count == 1 ? "Song removed from queue" : $"{count} songs removed from queue";
        _toastNotificationService?.ShowGenericNotification("Queue Updated", message);
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
            // Run on background thread to avoid blocking UI
            Task.Run(async () =>
            {
                try
                {
                    await PlayPreviousSong();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to play previous song in background task");
                }
            });
            e.Handled = true;
            return;
        }
        
        if (IsHotkeyMatch(e, config.NextTrackHotkey))
        {
            // Run on background thread to avoid blocking UI
            Task.Run(async () =>
            {
                try
                {
                    await PlayNextSong();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to play next song in background task");
                }
            });
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
        openFileMenuItem.Click += (s, e) => Task.Run(async () => await OpenLocalAudioFile());
        reloadPlaylistsMenuItem.Click += (s, e) => Task.Run(async () => await ReloadPlaylists());
        exitMenuItem.Click += (s, e) => Close();
        
        // Playback menu
        playMenuItem.Click += (s, e) => OnPlayPauseButtonClick();
        pauseMenuItem.Click += (s, e) => OnPlayPauseButtonClick();
        stopMenuItem.Click += (s, e) => _musicPlayerService.Stop();
        nextMenuItem.Click += (s, e) => Task.Run(async () => await PlayNextSong());
        previousMenuItem.Click += (s, e) => Task.Run(async () => await PlayPreviousSong());
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
    
    private async Task OpenLocalAudioFile()
    {
        try
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Audio File";
            openFileDialog.Filter = "Audio Files|*.mp3;*.m4a;*.wav;*.flac;*.aac;*.ogg;*.opus;*.webm|All Files|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog.FileName;
                Logger.Info($"Opening local audio file: {filePath}");
                
                // Validate the file format
                if (!AudioFormatUtils.IsSupportedAudioFile(filePath))
                {
                    var extension = AudioFormatUtils.GetFileExtension(filePath);
                    MessageBox.Show(
                        $"Unsupported audio format: {extension}\n\nSupported formats: {string.Join(", ", AudioFormatUtils.SupportedExtensions)}",
                        "Unsupported Format",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
                
                // Create a temporary song object for the local file
                var localSong = new Song
                {
                    Id = $"local_{Path.GetFileNameWithoutExtension(filePath)}",
                    Title = Path.GetFileNameWithoutExtension(filePath),
                    Artist = "Local File",
                    Album = "Local Files",
                    Source = "Local",
                    Duration = null, // Will be determined during playback
                    SelectedStream = new AudioStreamInfo
                    {
                        Url = filePath,
                        FormatId = "local",
                        AudioCodec = AudioFormatUtils.GetCodecFromExtension(AudioFormatUtils.GetFileExtension(filePath)),
                        Extension = AudioFormatUtils.GetFileExtension(filePath),
                        Container = AudioFormatUtils.GetFileExtension(filePath)
                    }
                };
                
                // Add to queue and play
                _queue.AddSong(localSong);
                await _musicPlayerService?.PlaySongAsync(localSong);
                
                // Switch to queue tab to show the added song
                mainTabControl.SelectedTab = queueTabPage;
                
                Logger.Info($"Successfully added local file to queue: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to open local audio file");
            MessageBox.Show(
                $"Failed to open audio file: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}