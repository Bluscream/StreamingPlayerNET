using StreamingPlayerNET.Common.Models;
using NLog;

namespace StreamingPlayerNET.UI;

public enum SongContextMenuType
{
    Search,
    Queue,
    Playlist
}

public partial class MainForm
{
    private void SetupSongContextMenu(SongContextMenuType contextMenuType)
    {
        var contextMenu = new ContextMenuStrip();
        
        // Play menu item
        var playMenuItem = new ToolStripMenuItem("Play");
        playMenuItem.Click += async (s, e) => await OnContextMenuPlay();
        contextMenu.Items.Add(playMenuItem);
        
        // Add to queue menu items (only for search and playlist)
        if (contextMenuType != SongContextMenuType.Queue)
        {
            var addToQueueMenuItem = new ToolStripMenuItem("Add to Queue");
            addToQueueMenuItem.Click += async (s, e) => await OnContextMenuAddToQueue();
            contextMenu.Items.Add(addToQueueMenuItem);
            
            // Add to queue next menu item (only for search)
            if (contextMenuType == SongContextMenuType.Search)
            {
                var addToQueueNextMenuItem = new ToolStripMenuItem("Add to Queue (Next)");
                addToQueueNextMenuItem.Click += async (s, e) => await OnContextMenuAddToQueueNext();
                contextMenu.Items.Add(addToQueueNextMenuItem);
                
                // Add multiple to queue menu item (only for search)
                var addMultipleToQueueMenuItem = new ToolStripMenuItem("Add Selected to Queue");
                addMultipleToQueueMenuItem.Click += async (s, e) => await OnContextMenuAddMultipleToQueue();
                contextMenu.Items.Add(addMultipleToQueueMenuItem);
            }
        }
        
        // Download menu item (available for all context menu types)
        var downloadMenuItem = new ToolStripMenuItem("Download");
        downloadMenuItem.Click += (s, e) => Task.Run(async () => await OnContextMenuDownload());
        contextMenu.Items.Add(downloadMenuItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        // Queue management items (only for queue)
        if (contextMenuType == SongContextMenuType.Queue)
        {
            var removeFromQueueMenuItem = new ToolStripMenuItem("Remove from Queue");
            removeFromQueueMenuItem.Click += (s, e) => OnContextMenuRemoveFromQueue();
            contextMenu.Items.Add(removeFromQueueMenuItem);
            
            var moveUpMenuItem = new ToolStripMenuItem("Move Up");
            moveUpMenuItem.Click += (s, e) => OnContextMenuMoveUp();
            contextMenu.Items.Add(moveUpMenuItem);
            
            var moveDownMenuItem = new ToolStripMenuItem("Move Down");
            moveDownMenuItem.Click += (s, e) => OnContextMenuMoveDown();
            contextMenu.Items.Add(moveDownMenuItem);
            
            contextMenu.Items.Add(new ToolStripSeparator());
        }
        
        // Copy URL menu item
        var copyUrlMenuItem = new ToolStripMenuItem("Copy URL");
        copyUrlMenuItem.Click += (s, e) => OnContextMenuCopyUrl();
        contextMenu.Items.Add(copyUrlMenuItem);
        
        // Copy title menu item
        var copyTitleMenuItem = new ToolStripMenuItem("Copy Title");
        copyTitleMenuItem.Click += (s, e) => OnContextMenuCopyTitle();
        contextMenu.Items.Add(copyTitleMenuItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        // View on YouTube menu item
        var viewOnYouTubeMenuItem = new ToolStripMenuItem(contextMenuType == SongContextMenuType.Queue ? "View on YouTube" : "Open URL");
        viewOnYouTubeMenuItem.Click += (s, e) => OnContextMenuViewOnYouTube();
        contextMenu.Items.Add(viewOnYouTubeMenuItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        // Open File menu item
        var openFileMenuItem = new ToolStripMenuItem("Open File");
        openFileMenuItem.Click += (s, e) => OnContextMenuOpenFile();
        contextMenu.Items.Add(openFileMenuItem);
        
        // Show in Explorer menu item
        var showInExplorerMenuItem = new ToolStripMenuItem("Show in Explorer");
        showInExplorerMenuItem.Click += (s, e) => OnContextMenuShowInExplorer();
        contextMenu.Items.Add(showInExplorerMenuItem);
        
        // Assign context menu to appropriate list view
        switch (contextMenuType)
        {
            case SongContextMenuType.Search:
                searchListView.ContextMenuStrip = contextMenu;
                searchListView.SelectedIndexChanged += (s, e) => UpdateSearchContextMenuItems(contextMenu);
                break;
            case SongContextMenuType.Queue:
                queueListView.ContextMenuStrip = contextMenu;
                queueListView.SelectedIndexChanged += (s, e) => UpdateQueueContextMenuItems(contextMenu);
                break;
            case SongContextMenuType.Playlist:
                playlistListView.ContextMenuStrip = contextMenu;
                playlistListView.SelectedIndexChanged += (s, e) => UpdatePlaylistContextMenuItems(contextMenu);
                break;
        }
    }



    private void UpdateSearchContextMenuItems(ContextMenuStrip contextMenu)
    {
        UpdateContextMenuItems(contextMenu, searchListView);
    }

    private void UpdateQueueContextMenuItems(ContextMenuStrip contextMenu)
    {
        UpdateContextMenuItems(contextMenu, queueListView);
    }

    private void UpdatePlaylistContextMenuItems(ContextMenuStrip contextMenu)
    {
        UpdateContextMenuItems(contextMenu, playlistListView);
    }

    private async Task OnContextMenuPlay()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count == 1)
        {
            var selectedItem = activeListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                await PlaySong(song);
            }
        }
    }

    private async Task OnContextMenuAddToQueue()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count == 1 && activeListView != queueListView)
        {
            var selectedItem = activeListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                _queue.AddSong(song);
                Logger.Info($"Added '{song.Title}' to queue");
            }
        }
    }

    private async Task OnContextMenuAddToQueueNext()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count == 1 && activeListView != queueListView)
        {
            var selectedItem = activeListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                var nextIndex = _queue.CurrentIndex + 1;
                _queue.InsertSong(nextIndex, song);
                Logger.Info($"Added '{song.Title}' to queue (next)");
            }
        }
    }

    private async Task OnContextMenuAddMultipleToQueue()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count > 0 && activeListView != queueListView)
        {
            var selectedSongs = new List<Song>();
            
            foreach (ListViewItem item in activeListView.SelectedItems)
            {
                if (item.Tag is Song song)
                {
                    selectedSongs.Add(song);
                }
            }
            
            if (selectedSongs.Count > 0)
            {
                _queue.AddSongs(selectedSongs);
                Logger.Info($"Added {selectedSongs.Count} songs to queue");
            }
        }
    }

    private async Task OnContextMenuDownload()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count == 1)
        {
            var selectedItem = activeListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                try
                {
                    Logger.Info($"Starting download for song: {song.Title}");
                    
                    // Get metadata and audio stream if not already available
                    if (string.IsNullOrEmpty(song.Title) || song.SelectedStream == null)
                    {
                        Logger.Info("Getting metadata and audio stream for download");
                        var metadata = await _metadataService.GetSongMetadataAsync(song.Id);
                        
                        // Update song with metadata
                        song.Title = metadata.Title;
                        song.Artist = metadata.Artist;
                        song.Duration = metadata.Duration;
                        song.ThumbnailUrl = metadata.ThumbnailUrl;
                        song.Description = metadata.Description;
                        song.UploadDate = metadata.UploadDate;
                        song.ViewCount = metadata.ViewCount;
                        song.LikeCount = metadata.LikeCount;
                        
                        // Get best audio stream
                        song.SelectedStream = await _metadataService.GetBestAudioStreamAsync(song.Id);
                    }
                    
                    if (song.SelectedStream != null)
                    {
                        // Download the audio file
                        var filePath = await _downloadService.DownloadAudioAsync(song, song.SelectedStream);
                        
                        Logger.Info($"Successfully downloaded song to: {filePath}");
                        
                        // Show success message
                        SafeInvoke(() => MessageBox.Show(
                            $"Successfully downloaded '{song.Title}' to:\n{filePath}",
                            "Download Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        ));
                    }
                    else
                    {
                        throw new InvalidOperationException("No audio stream available for download");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Failed to download song: {song.Title}");
                    
                    // Show error message
                    SafeInvoke(() => MessageBox.Show(
                        $"Failed to download '{song.Title}':\n{ex.Message}",
                        "Download Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    ));
                }
            }
        }
    }

    private void OnContextMenuCopyUrl()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count == 1)
        {
            var selectedItem = activeListView.SelectedItems[0];
            if (selectedItem.Tag is Song song && !string.IsNullOrEmpty(song.Url))
            {
                Clipboard.SetText(song.Url);
                Logger.Info("URL copied to clipboard");
            }
        }
    }

    private void OnContextMenuCopyTitle()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count == 1)
        {
            var selectedItem = activeListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                var title = $"{song.Title} - {song.Artist}";
                Clipboard.SetText(title);
                Logger.Info("Title copied to clipboard");
            }
        }
    }

    private void OnContextMenuViewOnYouTube()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count == 1)
        {
            var selectedItem = activeListView.SelectedItems[0];
            if (selectedItem.Tag is Song song && !string.IsNullOrEmpty(song.Url))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = song.Url,
                        UseShellExecute = true
                    });
                    Logger.Info("Opening URL in browser");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to open URL");
                    
                }
            }
        }
    }

    private void OnContextMenuRemoveFromQueue()
    {
        if (queueListView.SelectedItems.Count == 1)
        {
            var selectedItem = queueListView.SelectedItems[0];
            if (selectedItem.Tag is Song song)
            {
                var index = queueListView.SelectedIndices[0];
                _queue.RemoveSongAt(index);
                
                // Refresh the queue display
                UpdateQueueDisplay();
                
                Logger.Info($"Removed '{song.Title}' from queue");
            }
        }
    }

    private void OnContextMenuMoveUp()
    {
        if (queueListView.SelectedItems.Count == 1)
        {
            var selectedIndex = queueListView.SelectedIndices[0];
            if (selectedIndex > 0)
            {
                _queue.MoveSong(selectedIndex, selectedIndex - 1);
                
                // Refresh the queue display
                UpdateQueueDisplay();
                
                // Select the moved item
                if (queueListView.Items.Count > selectedIndex - 1)
                {
                    queueListView.Items[selectedIndex - 1].Selected = true;
                }
                
                Logger.Info("Moved song up in queue");
            }
        }
    }

    private void OnContextMenuMoveDown()
    {
        if (queueListView.SelectedItems.Count == 1)
        {
            var selectedIndex = queueListView.SelectedIndices[0];
            if (selectedIndex < queueListView.Items.Count - 1)
            {
                _queue.MoveSong(selectedIndex, selectedIndex + 1);
                
                // Refresh the queue display
                UpdateQueueDisplay();
                
                // Select the moved item
                if (queueListView.Items.Count > selectedIndex + 1)
                {
                    queueListView.Items[selectedIndex + 1].Selected = true;
                }
                
                Logger.Info("Moved song down in queue");
            }
        }
    }
    
    private void OnContextMenuShowInExplorer()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count == 1)
        {
            var selectedItem = activeListView.SelectedItems[0];
            if (selectedItem.Tag is Song song && song.SelectedStream != null)
            {
                try
                {
                    // Get the caching service from the playback service
                    var cachingService = _playbackService?.GetCachingService();
                    if (cachingService != null)
                    {
                        var filePath = cachingService.GetCachedFilePath(song, song.SelectedStream);
                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            // Show the file in Windows Explorer
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = $"/select,\"{filePath}\"",
                                UseShellExecute = true
                            });
                            Logger.Info($"Opened file in Explorer: {filePath}");
                        }
                        else
                        {
                            Logger.Warn($"File not found in cache for song: {song.Title}");
                            // Show a message to the user that the file is not cached
                            MessageBox.Show(
                                "This song is not currently cached locally. Play the song first to cache it.",
                                "File Not Cached",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                        }
                    }
                    else
                    {
                        Logger.Warn("Caching service not available");
                        MessageBox.Show(
                            "Caching service is not available.",
                            "Service Unavailable",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to show file in Explorer");
                    MessageBox.Show(
                        $"Failed to show file in Explorer: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }
    }
    
    private void OnContextMenuOpenFile()
    {
        var activeListView = GetActiveListView();
        if (activeListView?.SelectedItems.Count == 1)
        {
            var selectedItem = activeListView.SelectedItems[0];
            if (selectedItem.Tag is Song song && song.SelectedStream != null)
            {
                try
                {
                    // Get the caching service from the playback service
                    var cachingService = _playbackService?.GetCachingService();
                    if (cachingService != null)
                    {
                        var filePath = cachingService.GetCachedFilePath(song, song.SelectedStream);
                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            // Open the file with the default application
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                            Logger.Info($"Opened file with default application: {filePath}");
                        }
                        else
                        {
                            Logger.Warn($"File not found in cache for song: {song.Title}");
                            // Show a message to the user that the file is not cached
                            MessageBox.Show(
                                "This song is not currently cached locally. Play the song first to cache it.",
                                "File Not Cached",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                        }
                    }
                    else
                    {
                        Logger.Warn("Caching service not available");
                        MessageBox.Show(
                            "Caching service is not available.",
                            "Service Unavailable",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to open file");
                    MessageBox.Show(
                        $"Failed to open file: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }
    }
}