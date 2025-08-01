using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Services;
using NLog;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private void PopulateListViewWithSongs(ListView listView, List<Song> songs)
    {
        listView.Items.Clear();
        
        foreach (var song in songs)
        {
            var item = new ListViewItem(song.Title);
            item.SubItems.Add(song.Artist);
            item.SubItems.Add(song.Duration?.ToString(@"mm\:ss") ?? "Unknown");
            item.SubItems.Add(song.Source ?? "Unknown");
            item.Tag = song;
            listView.Items.Add(item);
        }
        
        // Adjust columns after populating
        AdjustListViewColumns(listView);
    }
    
    private void UpdateSearchResults(List<Song> songs)
    {
        _searchResults.Clear();
        songs = songs
            .OrderBy(s => s.Title ?? string.Empty)
            .ThenBy(s => s.Artist ?? string.Empty)
            .ToList();
        _searchResults.AddRange(songs);
        PopulateListViewWithSongs(searchListView, songs);
    }
    
    private void UpdateQueueDisplay()
    {
        PopulateListViewWithSongs(queueListView, _queue.Songs);
    }
    
    private void UpdatePlaylistDisplay(List<Song> songs)
    {
        PopulateListViewWithSongs(playlistListView, songs);
    }

    private void UpdateStatus(string message)
    {
        Logger.Info($"Status: {message}");
        // Only update status label for critical messages
        if (message.Contains("Failed") || message.Contains("Error") || message.Contains("Ready") || message.Contains("Initializing"))
        {
            statusLabel.Text = message;
        }
    }

    private void UpdateWindowTitle(Song? song, StreamingPlayerNET.Common.Models.PlaybackState state)
    {
        if (InvokeRequired)
        {
            SafeInvoke(() => UpdateWindowTitle(song, state));
            return;
        }
        
        var baseTitle = "Simple Music Player";
        if (song == null || state == StreamingPlayerNET.Common.Models.PlaybackState.Stopped)
        {
            Text = baseTitle;
            return;
        }
        
        try
        {
            var config = ConfigurationService.Current;
            var statusText = FormatStatusString(config.StatusStringFormat, song, state);
            Text = $"{baseTitle} - {statusText}";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to format window title");
            Text = baseTitle;
        }
    }

    public string FormatStatusString(string format, Song song, StreamingPlayerNET.Common.Models.PlaybackState state)
    {
        try
        {
            var playlist = _musicPlayerService?.GetCurrentPlaylist();
            var playlistIndex = _musicPlayerService?.GetCurrentPlaylistIndex() ?? 0;
            var position = _musicPlayerService?.GetCurrentPosition() ?? TimeSpan.Zero;
            var duration = _musicPlayerService?.GetTotalDuration();
            
            var replacements = new Dictionary<string, string>
            {
                ["{song}"] = song.Title ?? "Unknown Song",
                ["{artist}"] = song.Artist ?? "Unknown Artist",
                ["{album}"] = "Unknown Album", // Album not available in current Song model
                ["{playlist}"] = playlist?.Name ?? "No Playlist",
                ["{song_elapsed}"] = FormatTimeSpan(position),
                ["{song_total}"] = FormatTimeSpan(duration),
                ["{playlist_index}"] = (playlistIndex + 1).ToString(),
                ["{playlist_total}"] = playlist?.Songs?.Count.ToString() ?? "0",
                ["{status}"] = GetStatusText(state)
            };
            
            var result = format;
            foreach (var replacement in replacements)
            {
                result = result.Replace(replacement.Key, replacement.Value);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to format status string");
            return "Playing";
        }
    }

    private string FormatTimeSpan(TimeSpan? timeSpan)
    {
        if (!timeSpan.HasValue) return "00:00";
        return $"{timeSpan.Value.Minutes:D2}:{timeSpan.Value.Seconds:D2}";
    }

    private string GetStatusText(StreamingPlayerNET.Common.Models.PlaybackState state)
    {
        return state switch
        {
            StreamingPlayerNET.Common.Models.PlaybackState.Playing => "Playing",
            StreamingPlayerNET.Common.Models.PlaybackState.Paused => "Paused",
            StreamingPlayerNET.Common.Models.PlaybackState.Stopped => "Stopped",
            StreamingPlayerNET.Common.Models.PlaybackState.Loading => "Loading",
            _ => "Unknown"
        };
    }

    private void UpdateContextMenuItems(ContextMenuStrip contextMenu, ListView listView)
    {
        var hasSelection = listView.SelectedItems.Count > 0;
        var hasSingleSelection = listView.SelectedItems.Count == 1;
        
        foreach (ToolStripItem item in contextMenu.Items)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                switch (menuItem.Text)
                {
                    case "Play":
                    case "Copy URL":
                    case "Copy Title":
                    case "View on YouTube":
                    case "Open URL":
                    case "Add to Queue":
                    case "Add to Queue (Next)":
                    case "Remove from Queue":
                    case "Move Up":
                    case "Move Down":
                        menuItem.Enabled = hasSingleSelection;
                        break;
                    case "Add Selected to Queue":
                        menuItem.Enabled = hasSelection;
                        break;
                }
            }
        }
    }

    private ListView? GetActiveListView()
    {
        return mainTabControl.SelectedTab switch
        {
            var tab when tab == searchTabPage => searchListView,
            var tab when tab == queueTabPage => queueListView,
            var tab when tab == playlistTabPage => playlistListView,
            _ => null
        };
    }

    private void SafeInvoke(Action action)
    {
        if (InvokeRequired)
        {
            Invoke(action);
        }
        else
        {
            action();
        }
    }

    private void ToggleTabVisibility(TabPage tabPage, bool show)
    {
        if (show)
        {
            if (!mainTabControl.TabPages.Contains(tabPage))
            {
                mainTabControl.TabPages.Add(tabPage);
            }
        }
        else
        {
            if (mainTabControl.TabPages.Contains(tabPage))
            {
                mainTabControl.TabPages.Remove(tabPage);
            }
        }
    }

    private void TogglePlaylistsVisibility()
    {
        ToggleTabVisibility(playlistTabPage, showPlaylistsMenuItem.Checked);
    }

    private void ToggleSearchVisibility()
    {
        ToggleTabVisibility(searchTabPage, showSearchMenuItem.Checked);
    }
}