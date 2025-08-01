using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Services;
using NLog;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private async void OnPlaylistDoubleClick()
    {
        if (playlistsListBox.SelectedItem != null)
        {
            try
            {
                var selectedPlaylistName = playlistsListBox.SelectedItem.ToString();
                var selectedPlaylist = _playlists.FirstOrDefault(p => p.Name == selectedPlaylistName);
                
                if (selectedPlaylist != null)
                {
                    Logger.Info($"Loading playlist: {selectedPlaylist.Name}");
                    
                    
                    // Load playlist songs
                    var songs = await LoadPlaylistSongsAsync(selectedPlaylist);
                    
                    if (songs.Count > 0)
                    {
                        // Display songs in playlist view
                        UpdatePlaylistDisplay(songs);
                        
                        // Switch to playlist tab
                        mainTabControl.SelectedTab = playlistTabPage;
                        
                        Logger.Info($"Loaded {songs.Count} songs from playlist: {selectedPlaylist.Name}");
                        
                    }
                    else
                    {
                        Logger.Warn($"No songs found in playlist: {selectedPlaylist.Name}");
                    }
                }
                else
                {
                    Logger.Warn($"Selected playlist not found: {selectedPlaylistName}");
                    
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load playlist");
                
            }
        }
    }

    private async Task<List<Song>> LoadPlaylistSongsAsync(Playlist playlist)
    {
        try
        {
            // Extract source provider name from playlist name
            var playlistName = playlist.Name;
            var sourceName = "";
            
            if (playlistName.StartsWith("[") && playlistName.Contains("]"))
            {
                var endBracket = playlistName.IndexOf("]");
                sourceName = playlistName.Substring(1, endBracket - 1);
                playlist.Name = playlistName.Substring(endBracket + 2); // Remove "[Source] " prefix
            }
            
            // Find the source provider by short name
            var sourceProvider = SourceManager.Instance.GetSourceProviders()
                .FirstOrDefault(sp => sp.ShortName == sourceName);
            
            Logger.Debug($"Looking for source provider with short name: '{sourceName}', found: {(sourceProvider?.Name ?? "null")}");
            
            if (sourceProvider != null && sourceProvider.PlaylistService.IsAvailable)
            {
                // Load songs from the playlist
                var songs = await sourceProvider.PlaylistService.GetPlaylistSongsAsync(playlist.Id);
                
                // Restore the original playlist name with source prefix
                playlist.Name = playlistName;
                
                return songs;
            }
            else
            {
                // If we can't find the source provider, return the songs that are already in the playlist
                return playlist.Songs;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to load songs for playlist: {playlist.Name}");
            return new List<Song>();
        }
    }

    private async void LoadPlaylists()
    {
        try
        {
            Logger.Info("Loading playlists...");
            playlistsListBox.Items.Clear();
            _playlists.Clear();
            
            // Get all source providers that have playlist services
            var sourceProviders = SourceManager.Instance.GetSourceProviders();
            
            foreach (var provider in sourceProviders)
            {
                try
                {
                    // Check if provider is available and has a playlist service
                    if (provider.IsAvailable && provider.PlaylistService?.IsAvailable == true)
                    {
                        Logger.Info($"Loading playlists from {provider.Name}...");
                        var providerPlaylists = await provider.PlaylistService.LoadUserPlaylistsAsync();
                        
                        foreach (var playlist in providerPlaylists)
                        {
                            // Add source prefix to playlist name for identification
                            playlist.Name = $"[{provider.ShortName}] {playlist.Name}";
                            _playlists.Add(playlist);
                        }
                        
                        Logger.Info($"Loaded {providerPlaylists.Count} playlists from {provider.Name}");
                    }
                    else
                    {
                        Logger.Debug($"Skipping {provider.Name} - not available or no playlist service");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Failed to load playlists from {provider.Name}");
                }
            }
            
            // Also search for public playlists if no user playlists were found
            if (_playlists.Count == 0)
            {
                Logger.Info("No user playlists found, searching for public playlists...");
                await LoadPublicPlaylists();
            }

            // Sort playlists by name
            var sortedPlaylists = _playlists.OrderBy(p => p.Name).ToList();
            playlistsListBox.Items.Clear();
            foreach (var playlist in sortedPlaylists)
            {
                playlistsListBox.Items.Add(playlist.Name);
            }
            // Replace _playlists with sorted list
            _playlists.Clear();
            _playlists.AddRange(sortedPlaylists);

            Logger.Info($"Total playlists loaded: {_playlists.Count}");
            
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load playlists");
            
        }
    }

    private async Task LoadPublicPlaylists()
    {
        try
        {
            var sourceProviders = SourceManager.Instance.GetSourceProviders();
            
            foreach (var provider in sourceProviders)
            {
                try
                {
                    // Check if provider is available and has a playlist service
                    if (provider.IsAvailable && provider.PlaylistService?.IsAvailable == true)
                    {
                        // Search for popular playlists
                        var searchTerms = new[] { "top hits", "popular", "trending", "best" };
                        
                        foreach (var searchTerm in searchTerms)
                        {
                            var publicPlaylists = await provider.PlaylistService.SearchPlaylistsAsync(searchTerm, 5);
                            
                            foreach (var playlist in publicPlaylists)
                            {
                                // Add source prefix to playlist name for identification
                                playlist.Name = $"[{provider.ShortName}] {playlist.Name}";
                                
                                // Avoid duplicates
                                if (!_playlists.Any(p => p.Id == playlist.Id))
                                {
                                    _playlists.Add(playlist);
                                    playlistsListBox.Items.Add(playlist.Name);
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.Debug($"Skipping {provider.Name} for public playlists - not available or no playlist service");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Failed to search public playlists from {provider.Name}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load public playlists");
        }
    }

    private async Task ReloadPlaylists()
    {
        try
        {
            Logger.Info("Reloading playlists...");
            
            
            // Call the existing LoadPlaylists method
            LoadPlaylists();
            
            Logger.Info("Playlists reloaded successfully");
            
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to reload playlists");
            
        }
    }
}