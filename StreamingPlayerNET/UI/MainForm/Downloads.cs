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
        
        Logger.Info("Downloads tab setup completed");
    }
    
    private void SetupDownloadsListView()
    {
        downloadsListView.View = View.Details;
        downloadsListView.FullRowSelect = true;
        downloadsListView.GridLines = true;
        downloadsListView.MultiSelect = false;
        
        // Adjust columns
        AdjustDownloadsListViewColumns();
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
    
    public void AddDownload(Song song, AudioStreamInfo streamInfo)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => AddDownload(song, streamInfo));
            return;
        }
        
        try
        {
            var downloadInfo = new DownloadInfo
            {
                Title = song.Title ?? "Unknown",
                Artist = song.Artist ?? "Unknown",
                Status = "Starting...",
                BytesDownloaded = 0,
                TotalBytes = 0,
                StartTime = DateTime.Now,
                EstimatedTimeRemaining = TimeSpan.Zero,
                Song = song,
                StreamInfo = streamInfo
            };
            
            _downloads.Add(downloadInfo);
            UpdateDownloadsDisplay();
            
            Logger.Debug($"Added download: {song.Title}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to add download to UI");
        }
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
            string songTitle = e.SongTitle;
            string? cacheKey = null;
            
            if (e.SongTitle.Contains("|"))
            {
                var parts = e.SongTitle.Split('|', 2);
                songTitle = parts[0];
                cacheKey = parts[1];
            }
            
            // Try to find the download by cache key first (most reliable)
            DownloadInfo? download = null;
            if (!string.IsNullOrEmpty(cacheKey))
            {
                download = _downloads.FirstOrDefault(d => d.CacheKey == cacheKey);
            }
            
            // Fallback to finding by song title if cache key matching failed
            if (download == null)
            {
                var matchingDownloads = _downloads
                    .Where(d => d.Title == songTitle && d.Status != "Completed" && d.Status != "Failed")
                    .OrderByDescending(d => d.StartTime)
                    .ToList();
                    
                if (matchingDownloads.Count > 0)
                {
                    // If multiple downloads match, try to find the one that's most likely to be the current one
                    // by checking if it has any progress already or if it's the most recent
                    download = matchingDownloads.FirstOrDefault(d => d.BytesDownloaded > 0) ?? matchingDownloads.First();
                }
            }
                
            if (download != null)
            {
                // Only update if this download doesn't already have more progress (to avoid overwriting with older progress)
                if (download.BytesDownloaded <= e.BytesReceived || download.TotalBytes == 0)
                {
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
                    
                    Logger.Debug($"Updated progress for {download.Title}: {e.BytesReceived}/{e.TotalBytes} bytes ({e.BytesReceived * 100.0 / e.TotalBytes:F1}%)");
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