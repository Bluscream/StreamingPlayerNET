using NLog;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private void AdjustListViewColumns(ListView listView)
    {
        if (listView.Columns.Count != 4) return; // Expecting Title, Artist, Duration, Source
        
        try
        {
            // Set fixed width for duration and source columns (small)
            listView.Columns[2].Width = 80; // Duration column
            listView.Columns[3].Width = 100; // Source column
            
            // Calculate remaining width for Title and Artist columns
            int availableWidth = listView.ClientSize.Width - 80 - 100 - SystemInformation.VerticalScrollBarWidth - 4;
            
            if (availableWidth > 200) // Ensure minimum usable width
            {
                // Split remaining space: 60% Title, 40% Artist
                int titleWidth = (int)(availableWidth * 0.6);
                int artistWidth = availableWidth - titleWidth;
                
                listView.Columns[0].Width = titleWidth;   // Title
                listView.Columns[1].Width = artistWidth;  // Artist
            }
            else
            {
                // Fallback to equal split if very narrow
                int equalWidth = availableWidth / 2;
                listView.Columns[0].Width = equalWidth;   // Title
                listView.Columns[1].Width = equalWidth;   // Artist
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error adjusting ListView columns");
        }
    }



    private void OnFormResize(object? sender, EventArgs e)
    {
        // Adjust columns for all ListViews
        if (searchListView.Items.Count > 0)
        {
            AdjustListViewColumns(searchListView);
        }
        if (queueListView.Items.Count > 0)
        {
            AdjustListViewColumns(queueListView);
        }
        if (playlistListView.Items.Count > 0)
        {
            AdjustListViewColumns(playlistListView);
        }
        

    }

    private void OnSearchListViewResize(object? sender, EventArgs e) => 
        OnListViewResize(searchListView);

    private void OnQueueListViewResize(object? sender, EventArgs e) => 
        OnListViewResize(queueListView);

    private void OnPlaylistListViewResize(object? sender, EventArgs e) => 
        OnListViewResize(playlistListView);

    private void OnListViewResize(ListView listView)
    {
        if (listView.Items.Count > 0)
        {
            AdjustListViewColumns(listView);
        }
    }



    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        Logger.Info("=== Simple Music Player .NET Shutting Down ===");
        Logger.Debug("Cleaning up resources before application exit");
        

        
        // Unsubscribe from download service events
        if (_downloadService != null)
        {
            _downloadService.DownloadProgressChanged -= OnDownloadProgressChanged;
        }
        

        
        // Dispose Windows Media Service
        _windowsMediaService?.Dispose();
        
        // Dispose toast notification service
        _toastNotificationService?.Dispose();
        
        // Save queue cache before shutting down
        SaveCachedQueue();
        
        _musicPlayerService?.Stop();
        _progressTimer?.Stop();
        _progressTimer?.Dispose();
        
        Logger.Info("Application shutdown completed successfully");
        base.OnFormClosing(e);
    }
}