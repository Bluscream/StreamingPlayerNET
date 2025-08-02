using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Services;
using NLog;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private void SetupDownloadsTab()
    {
        // Setup downloads update timer
        _downloadsUpdateTimer = new System.Windows.Forms.Timer();
        _downloadsUpdateTimer.Interval = 1000; // Update every second
        _downloadsUpdateTimer.Tick += DownloadsUpdateTimer_Tick;
        _downloadsUpdateTimer.Start();
        
        // Setup downloads ListView
        SetupDownloadsListView();
        
        // Setup downloads context menu
        SetupDownloadsContextMenu();
        
        Logger.Info("Downloads tab setup completed");
    }
    
    private void SetupDownloadsListView()
    {
        downloadsListView.View = View.Details;
        downloadsListView.FullRowSelect = true;
        downloadsListView.GridLines = true;
        downloadsListView.MultiSelect = true; // Enable multiselect
        
        // Adjust columns
        AdjustDownloadsListViewColumns();
    }
    
    private void SetupDownloadsContextMenu()
    {
        var contextMenu = new ContextMenuStrip();
        
        // Cancel download menu item
        var cancelMenuItem = new ToolStripMenuItem("Cancel Download");
        cancelMenuItem.Click += (s, e) => OnContextMenuCancelDownload();
        contextMenu.Items.Add(cancelMenuItem);
        
        // Cancel multiple downloads menu item
        var cancelMultipleMenuItem = new ToolStripMenuItem("Cancel Selected Downloads");
        cancelMultipleMenuItem.Click += (s, e) => OnContextMenuCancelMultipleDownloads();
        contextMenu.Items.Add(cancelMultipleMenuItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        // Copy title menu item
        var copyTitleMenuItem = new ToolStripMenuItem("Copy Title");
        copyTitleMenuItem.Click += (s, e) => OnContextMenuCopyDownloadTitle();
        contextMenu.Items.Add(copyTitleMenuItem);
        
        // Copy artist menu item
        var copyArtistMenuItem = new ToolStripMenuItem("Copy Artist");
        copyArtistMenuItem.Click += (s, e) => OnContextMenuCopyDownloadArtist();
        contextMenu.Items.Add(copyArtistMenuItem);
        
        // Assign context menu to downloads ListView
        downloadsListView.ContextMenuStrip = contextMenu;
        downloadsListView.SelectedIndexChanged += (s, e) => UpdateDownloadsContextMenuItems(contextMenu);
    }
    
    private void UpdateDownloadsContextMenuItems(ContextMenuStrip contextMenu)
    {
        var selectedCount = downloadsListView.SelectedItems.Count;
        
        // Enable/disable menu items based on selection
        foreach (ToolStripItem item in contextMenu.Items)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                switch (menuItem.Text)
                {
                    case "Cancel Download":
                        menuItem.Enabled = selectedCount == 1;
                        break;
                    case "Cancel Selected Downloads":
                        menuItem.Enabled = selectedCount > 1;
                        break;
                    case "Copy Title":
                    case "Copy Artist":
                        menuItem.Enabled = selectedCount == 1;
                        break;
                }
            }
        }
    }
    
    private void OnContextMenuCancelDownload()
    {
        if (downloadsListView.SelectedItems.Count == 1)
        {
            var selectedItem = downloadsListView.SelectedItems[0];
            if (selectedItem.Tag is DownloadInfo downloadInfo)
            {
                CancelDownload(downloadInfo);
            }
        }
    }
    
    private void OnContextMenuCancelMultipleDownloads()
    {
        if (downloadsListView.SelectedItems.Count > 1)
        {
            var selectedDownloads = new List<DownloadInfo>();
            
            foreach (ListViewItem item in downloadsListView.SelectedItems)
            {
                if (item.Tag is DownloadInfo downloadInfo)
                {
                    selectedDownloads.Add(downloadInfo);
                }
            }
            
            if (selectedDownloads.Count > 0)
            {
                CancelMultipleDownloads(selectedDownloads);
            }
        }
    }
    
    private void OnContextMenuCopyDownloadTitle()
    {
        if (downloadsListView.SelectedItems.Count == 1)
        {
            var selectedItem = downloadsListView.SelectedItems[0];
            if (selectedItem.Tag is DownloadInfo downloadInfo)
            {
                Clipboard.SetText(downloadInfo.Title);
                Logger.Info("Download title copied to clipboard");
            }
        }
    }
    
    private void OnContextMenuCopyDownloadArtist()
    {
        if (downloadsListView.SelectedItems.Count == 1)
        {
            var selectedItem = downloadsListView.SelectedItems[0];
            if (selectedItem.Tag is DownloadInfo downloadInfo)
            {
                Clipboard.SetText(downloadInfo.Artist);
                Logger.Info("Download artist copied to clipboard");
            }
        }
    }
    
    private void CancelDownload(DownloadInfo downloadInfo)
    {
        try
        {
            CancelDownloadByCacheKey(downloadInfo.CacheKey);
            _toastNotificationService?.ShowGenericNotification("Download Cancelled", $"Cancelled download: {downloadInfo.Title}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to cancel download: {downloadInfo.Title}");
        }
    }
    
    private void CancelMultipleDownloads(List<DownloadInfo> downloads)
    {
        try
        {
            var cancelledCount = 0;
            
            foreach (var downloadInfo in downloads)
            {
                if (_activeDownloads.TryGetValue(downloadInfo.CacheKey, out var cancellationTokenSource))
                {
                    cancellationTokenSource.Cancel();
                    downloadInfo.Status = "Cancelling...";
                    cancelledCount++;
                }
            }
            
            UpdateDownloadsDisplay();
            
            Logger.Info($"Cancelled {cancelledCount} downloads");
            _toastNotificationService?.ShowGenericNotification("Downloads Cancelled", $"Cancelled {cancelledCount} downloads");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to cancel multiple downloads");
        }
    }
    
    public void CancelDownloadByCacheKey(string cacheKey)
    {
        try
        {
            if (_activeDownloads.TryGetValue(cacheKey, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                
                var download = _downloads.FirstOrDefault(d => d.CacheKey == cacheKey);
                if (download != null)
                {
                    download.Status = "Cancelled";
                }
                
                UpdateDownloadsDisplay();
                Logger.Info($"Cancelled download with cache key: {cacheKey}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to cancel download with cache key: {cacheKey}");
        }
    }
    
    public void MarkDownloadAsCancelled(string cacheKey)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => MarkDownloadAsCancelled(cacheKey));
            return;
        }
        
        try
        {
            var download = _downloads.FirstOrDefault(d => d.CacheKey == cacheKey);
            if (download != null)
            {
                download.Status = "Cancelled";
                
                // Remove from active downloads
                _activeDownloads.Remove(cacheKey);
                
                UpdateDownloadsDisplay();
                Logger.Info($"Marked download as cancelled: {download.Title}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to mark download as cancelled: {cacheKey}");
        }
    }
    
    private void AdjustDownloadsListViewColumns()
    {
        if (downloadsListView.Columns.Count > 0)
        {
            var totalWidth = downloadsListView.Width - 20; // Account for scrollbar
            
            // Calculate proportional widths
            downloadTitleColumn.Width = (int)(totalWidth * 0.35); // 35%
            downloadArtistColumn.Width = (int)(totalWidth * 0.20); // 20%
            downloadStatusColumn.Width = (int)(totalWidth * 0.15); // 15%
            downloadProgressColumn.Width = (int)(totalWidth * 0.20); // 20%
            downloadTimeColumn.Width = (int)(totalWidth * 0.10); // 10%
        }
    }
    
    private void DownloadsUpdateTimer_Tick(object? sender, EventArgs e)
    {
        UpdateDownloadsDisplay();
    }
    
    private void UpdateDownloadsDisplay()
    {
        if (InvokeRequired)
        {
            SafeInvoke(UpdateDownloadsDisplay);
            return;
        }
        
        try
        {
            // Use the actual downloads list instead of creating a generic entry
            var currentDownloads = new List<DownloadInfo>(_downloads);
            
            Logger.Debug($"Updating downloads display with {currentDownloads.Count} downloads");
            foreach (var download in currentDownloads)
            {
                Logger.Debug($"  - {download.Title} by {download.Artist}: {download.Status} ({download.FormattedProgress})");
            }
            
            // Update the display
            PopulateDownloadsListView(currentDownloads);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to update downloads display");
        }
    }
    
    private void PopulateDownloadsListView(List<DownloadInfo> downloads)
    {
        downloadsListView.Items.Clear();
        
        foreach (var download in downloads)
        {
            var item = new ListViewItem(download.Title);
            item.SubItems.Add(download.Artist);
            item.SubItems.Add(download.Status);
            item.SubItems.Add(download.FormattedProgress);
            item.SubItems.Add(download.ElapsedTime);
            item.Tag = download;
            downloadsListView.Items.Add(item);
        }
        
        // Adjust columns after populating
        AdjustDownloadsListViewColumns();
    }
    
    public void AddDownload(Song song, AudioStreamInfo streamInfo, CancellationTokenSource? cancellationTokenSource = null)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => AddDownload(song, streamInfo, cancellationTokenSource));
            return;
        }
        
        try
        {
            // Generate cache key to match the one used by CachingService
            var cacheKey = GenerateCacheKey(song, streamInfo);
            
            var downloadInfo = new DownloadInfo
            {
                Title = song.Title ?? "Unknown",
                Artist = song.Artist ?? "Unknown",
                Status = "Starting...",
                BytesDownloaded = 0,
                TotalBytes = 0,
                StartTime = DateTime.Now,
                EstimatedTimeRemaining = TimeSpan.Zero,
                CacheKey = cacheKey,
                Song = song,
                StreamInfo = streamInfo
            };
            
            _downloads.Add(downloadInfo);
            
            // Track the download with its cancellation token if provided
            if (cancellationTokenSource != null)
            {
                _activeDownloads[cacheKey] = cancellationTokenSource;
            }
            
            UpdateDownloadsDisplay();
            
            Logger.Debug($"Added download: {song.Title} with cache key: {cacheKey}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to add download to UI");
        }
    }
    
    private string GenerateCacheKey(Song song, AudioStreamInfo streamInfo)
    {
        // Create a unique cache key based on song metadata and codec
        // Use a more stable key that doesn't depend on stream-specific info
        var keyData = $"{song.Title}_{song.Artist}_{song.Album}";
        
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(keyData));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
    
    public void UpdateDownloadProgress(string cacheKey, long bytesDownloaded, long totalBytes, string status = "Downloading")
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => UpdateDownloadProgress(cacheKey, bytesDownloaded, totalBytes, status));
            return;
        }
        
        try
        {
            var download = _downloads.FirstOrDefault(d => d.CacheKey == cacheKey);
            if (download != null)
            {
                download.BytesDownloaded = bytesDownloaded;
                download.TotalBytes = totalBytes;
                download.Status = status;
                
                // Calculate estimated time remaining
                if (bytesDownloaded > 0 && totalBytes > 0)
                {
                    var elapsed = DateTime.Now - download.StartTime;
                    var bytesPerSecond = bytesDownloaded / elapsed.TotalSeconds;
                    if (bytesPerSecond > 0)
                    {
                        var remainingBytes = totalBytes - bytesDownloaded;
                        download.EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingBytes / bytesPerSecond);
                    }
                }
                
                UpdateDownloadsDisplay();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to update download progress");
        }
    }
    
    public void CompleteDownload(string cacheKey, bool success = true)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => CompleteDownload(cacheKey, success));
            return;
        }
        
        try
        {
            var download = _downloads.FirstOrDefault(d => d.CacheKey == cacheKey);
            if (download != null)
            {
                download.Status = success ? "Completed" : "Failed";
                
                // Remove from active downloads
                _activeDownloads.Remove(cacheKey);
                
                // Remove completed downloads after a delay
                if (success)
                {
                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        if (InvokeRequired)
                        {
                            SafeInvoke(() => _downloads.Remove(download));
                        }
                        else
                        {
                            _downloads.Remove(download);
                        }
                        UpdateDownloadsDisplay();
                    });
                }
                
                UpdateDownloadsDisplay();
                Logger.Debug($"Download completed: {download.Title} (Success: {success})");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to complete download");
        }
    }
    
    public void FailDownload(string cacheKey, string errorMessage)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => FailDownload(cacheKey, errorMessage));
            return;
        }
        
        try
        {
            var download = _downloads.FirstOrDefault(d => d.CacheKey == cacheKey);
            if (download != null)
            {
                download.Status = "Failed";
                download.ErrorMessage = errorMessage;
                
                // Remove from active downloads
                _activeDownloads.Remove(cacheKey);
                
                UpdateDownloadsDisplay();
                
                Logger.Warn($"Download failed: {download.Title} - {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to mark download as failed");
        }
    }
    
    private void OnDownloadStarted(object? sender, DownloadInfo downloadInfo)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => OnDownloadStarted(sender, downloadInfo));
            return;
        }
        
        try
        {
            _downloads.Add(downloadInfo);
            
            // Create a cancellation token source for this download
            var cancellationTokenSource = new CancellationTokenSource();
            _activeDownloads[downloadInfo.CacheKey] = cancellationTokenSource;
            
            UpdateDownloadsDisplay();
            Logger.Debug($"Download started: {downloadInfo.Title} by {downloadInfo.Artist} (CacheKey: {downloadInfo.CacheKey})");
            Logger.Debug($"Total downloads in list: {_downloads.Count}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to handle download started event");
        }
    }
    
    private void OnCachingServiceDownloadProgressChanged(object? sender, DownloadProgressEventArgs e)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => OnCachingServiceDownloadProgressChanged(sender, e));
            return;
        }
        
        try
        {
            // Parse the song title to extract the cache key if present
            string songTitle = e.Song.Title;
            string? cacheKey = null;
            
            if (e.Song.Title.Contains("|"))
            {
                var parts = e.Song.Title.Split('|', 2);
                songTitle = parts[0];
                cacheKey = parts[1];
            }
            
            Logger.Debug($"Progress event received for '{songTitle}' with cache key: {cacheKey}");
            Logger.Debug($"Current downloads in list: {_downloads.Count}");
            foreach (var d in _downloads)
            {
                Logger.Debug($"  - {d.Title} (CacheKey: {d.CacheKey}, Status: {d.Status}, Progress: {d.BytesDownloaded}/{d.TotalBytes})");
            }
            
            // Try to find the download by cache key first (most reliable)
            DownloadInfo? download = null;
            if (!string.IsNullOrEmpty(cacheKey))
            {
                download = _downloads.FirstOrDefault(d => d.CacheKey == cacheKey);
                if (download != null)
                {
                    Logger.Debug($"Found download by cache key: {download.Title}");
                }
            }
            
            // Fallback to finding by song title if cache key matching failed
            if (download == null)
            {
                var matchingDownloads = _downloads
                    .Where(d => d.Title == songTitle && d.Status != "Completed" && d.Status != "Failed")
                    .OrderByDescending(d => d.StartTime)
                    .ToList();
                    
                Logger.Debug($"Found {matchingDownloads.Count} downloads matching title '{songTitle}'");
                
                if (matchingDownloads.Count > 0)
                {
                    // If multiple downloads match, try to find the one that's most likely to be the current one
                    // by checking if it has any progress already or if it's the most recent
                    download = matchingDownloads.FirstOrDefault(d => d.BytesDownloaded > 0) ?? matchingDownloads.First();
                    Logger.Debug($"Selected download: {download.Title} (CacheKey: {download.CacheKey})");
                }
            }
                
            if (download != null)
            {
                // Only update if this download doesn't already have more progress (to avoid overwriting with older progress)
                if (download.BytesDownloaded <= e.BytesReceived || download.TotalBytes == 0)
                {
                    var oldProgress = download.BytesDownloaded;
                    download.BytesDownloaded = e.BytesReceived;
                    download.TotalBytes = e.TotalBytes;
                    download.Status = "Downloading";
                    
                    // Calculate estimated time remaining
                    if (e.BytesReceived > 0 && e.TotalBytes > 0)
                    {
                        var elapsed = DateTime.Now - download.StartTime;
                        var bytesPerSecond = e.BytesReceived / elapsed.TotalSeconds;
                        if (bytesPerSecond > 0)
                        {
                            var remainingBytes = e.TotalBytes - e.BytesReceived;
                            download.EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingBytes / bytesPerSecond);
                        }
                    }
                    
                    Logger.Debug($"Updated progress for {download.Title}: {oldProgress} -> {e.BytesReceived}/{e.TotalBytes} bytes ({e.BytesReceived * 100.0 / e.TotalBytes:F1}%)");
                    UpdateDownloadsDisplay();
                }
                else
                {
                    Logger.Debug($"Skipped progress update for {download.Title} - already has more progress ({download.BytesDownloaded}/{download.TotalBytes} vs {e.BytesReceived}/{e.TotalBytes})");
                }
            }
            else
            {
                Logger.Debug($"Could not find download for progress update: {songTitle} (CacheKey: {cacheKey})");
                Logger.Debug("Available downloads:");
                foreach (var d in _downloads)
                {
                    Logger.Debug($"  - {d.Title} (CacheKey: {d.CacheKey}, Status: {d.Status})");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to handle download progress event");
        }
    }
    
    private void OnDownloadCompleted(object? sender, DownloadCompletedEventArgs e)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => OnDownloadCompleted(sender, e));
            return;
        }
        
        try
        {
            var download = _downloads.FirstOrDefault(d => d.CacheKey == e.CacheKey);
            if (download != null)
            {
                download.Status = e.Success ? "Completed" : "Failed";
                if (!e.Success && !string.IsNullOrEmpty(e.ErrorMessage))
                {
                    download.ErrorMessage = e.ErrorMessage;
                }
                
                // Remove from active downloads
                _activeDownloads.Remove(e.CacheKey);
                
                // Remove completed downloads after a delay
                if (e.Success)
                {
                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        if (InvokeRequired)
                        {
                            SafeInvoke(() => _downloads.Remove(download));
                        }
                        else
                        {
                            _downloads.Remove(download);
                        }
                        UpdateDownloadsDisplay();
                    });
                }
                
                UpdateDownloadsDisplay();
                Logger.Debug($"Download completed: {download.Title} (Success: {e.Success})");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to handle download completed event");
        }
    }
} 