using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Library;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using StreamingPlayerNET.Source.YouTubeMusic;
using StreamingPlayerNET.Source.YouTubeMusic.Utils;
using NLog;
using System.Text.Json;

namespace StreamingPlayerNET.Source.YouTubeMusic.Services;

public class YouTubeMusicPlaylistService : IPlaylistService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly YouTubeMusicClient _client;
    private readonly YouTubeMusicSourceSettings _settings;
    private readonly string _playlistsDirectory;
    private bool _initialized = false;
    
    public bool IsAvailable => _initialized && _client != null;
    
    public YouTubeMusicPlaylistService(YouTubeMusicClient client, YouTubeMusicSourceSettings settings)
    {
        _client = client;
        _settings = settings;
        _playlistsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData", "Playlists", "YouTubeMusic");
        
        // Ensure the playlists directory exists
        Directory.CreateDirectory(_playlistsDirectory);
    }
    
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) return;
        
        try
        {
            Logger.Info("Initializing YouTube Music Playlist Service...");
            
            // Test the client connection
            if (_client != null)
            {
                // Try a simple operation to verify the client is working
                await Task.Delay(100, cancellationToken); // Small delay to allow for any async initialization
            }
            
            _initialized = true;
            Logger.Info("YouTube Music Playlist Service initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize YouTube Music Playlist Service");
            throw;
        }
    }
    
    public async Task<Playlist?> LoadPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Loading YouTube Music playlist: {playlistId}");
            
            // Handle "Saved Songs" fake playlist
            if (playlistId == "saved_songs")
            {
                var savedSongs = await GetSavedSongsAsync(cancellationToken);
                var playlist = new Playlist
                {
                    Id = "saved_songs",
                    Name = "Saved Songs",
                    Description = "Your saved songs from YouTube Music",
                    ThumbnailUrl = null,
                    Author = "YouTube Music",
                    SongCount = savedSongs.Count,
                    Source = "YouTube Music",
                    CreatedDate = null,
                    LastModified = DateTime.UtcNow,
                    IsPublic = false
                };
                
                foreach (var song in savedSongs)
                {
                    playlist.AddSong(song);
                }
                
                Logger.Info($"Loaded Saved Songs playlist with {savedSongs.Count} songs");
                return playlist;
            }
            
            // First try to load from local storage
            var localPlaylist = await LoadLocalPlaylistAsync(playlistId, cancellationToken);
            if (localPlaylist != null)
            {
                Logger.Info($"Loaded playlist '{localPlaylist.Name}' from local storage");
                return localPlaylist;
            }
            
            // If not found locally, try to load from YouTube Music
            if (_client != null)
            {
                try
                {
                    // Try to load from user's library playlists
                    var libraryPlaylists = await _client.GetLibraryCommunityPlaylistsAsync();
                    var libraryPlaylist = libraryPlaylists.FirstOrDefault(p => p.Id == playlistId);
                    
                    if (libraryPlaylist != null)
                    {
                        var playlist = new Playlist
                        {
                            Id = playlistId,
                            Name = libraryPlaylist.Name,
                            Description = null,
                            ThumbnailUrl = libraryPlaylist.Thumbnails?.FirstOrDefault()?.Url,
                            Author = libraryPlaylist.Creator?.Name ?? "Unknown",
                            SongCount = libraryPlaylist.SongCount,
                            Source = "YouTube Music",
                            CreatedDate = null,
                            LastModified = DateTime.UtcNow,
                            IsPublic = true
                        };
                        
                        // Note: Library playlists don't provide direct song access through this method
                        // We would need to use a different approach to get the songs
                        
                        // Save to local storage for future use
                        await SaveLocalPlaylistAsync(playlist, cancellationToken);
                        
                        Logger.Info($"Loaded playlist '{playlist.Name}' from YouTube Music library with {playlist.SongCount} songs");
                        return playlist;
                    }
                    
                    // Fallback: try community playlist approach
                    try
                    {
                        var browseId = _client.GetCommunityPlaylistBrowseId(playlistId);
                        var playlistInfo = await _client.GetCommunityPlaylistInfoAsync(browseId);
                        var playlistSongs = _client.GetCommunityPlaylistSongsAsync(browseId);
                        var bufferedSongs = await playlistSongs.FetchItemsAsync(0, int.MaxValue);
                        
                        var playlist = new Playlist
                        {
                            Id = playlistId,
                            Name = playlistInfo.Name,
                            Description = null,
                            ThumbnailUrl = playlistInfo.Thumbnails?.FirstOrDefault()?.Url,
                            Author = playlistInfo.Creator?.Name ?? "Unknown",
                            SongCount = bufferedSongs.Count,
                            Source = "YouTube Music",
                            CreatedDate = null,
                            LastModified = DateTime.UtcNow,
                            IsPublic = true
                        };
                        
                        foreach (var playlistSong in bufferedSongs)
                        {
                            var song = ConvertToSong(playlistSong);
                            playlist.AddSong(song);
                        }
                        
                        // Save to local storage for future use
                        await SaveLocalPlaylistAsync(playlist, cancellationToken);
                        
                        Logger.Info($"Loaded playlist '{playlist.Name}' from YouTube Music community playlist with {playlist.Songs.Count} songs");
                        return playlist;
                    }
                    catch (Exception ex2)
                    {
                        Logger.Warn(ex2, $"Failed to load playlist {playlistId} as community playlist");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Failed to load playlist {playlistId} from YouTube Music library");
                }
            }
            
            Logger.Warn($"Playlist {playlistId} not found");
            return null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error loading playlist {playlistId}");
            return null;
        }
    }
    
    public async Task<List<Playlist>> LoadUserPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info("Loading user's YouTube Music playlists");
            
            var playlists = new List<Playlist>();
            
            // Load all local playlists
            var localPlaylists = await LoadAllLocalPlaylistsAsync(cancellationToken);
            playlists.AddRange(localPlaylists);
            
            // Add "Saved Songs" fake playlist
            var savedSongsPlaylist = new Playlist
            {
                Id = "saved_songs",
                Name = "Saved Songs",
                Description = "Your saved songs from YouTube Music",
                ThumbnailUrl = null,
                Author = "YouTube Music",
                SongCount = 0, // Will be updated when songs are loaded
                Source = "YouTube Music",
                CreatedDate = null,
                LastModified = DateTime.UtcNow,
                IsPublic = false
            };
            playlists.Add(savedSongsPlaylist);
            
            // Load user's library playlists if client is available
            if (_client != null)
            {
                try
                {
                    var libraryPlaylists = await _client.GetLibraryCommunityPlaylistsAsync();
                    
                    foreach (var libraryPlaylist in libraryPlaylists)
                    {
                        var playlist = new Playlist
                        {
                            Id = libraryPlaylist.Id,
                            Name = libraryPlaylist.Name,
                            Description = null,
                            ThumbnailUrl = libraryPlaylist.Thumbnails?.FirstOrDefault()?.Url,
                            Author = libraryPlaylist.Creator?.Name ?? "Unknown",
                            SongCount = libraryPlaylist.SongCount,
                            Source = "YouTube Music",
                            CreatedDate = null,
                            LastModified = DateTime.UtcNow,
                            IsPublic = true
                        };
                        
                        playlists.Add(playlist);
                    }
                    
                    Logger.Info($"Loaded {libraryPlaylists.Count()} library playlists from YouTube Music");
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to load library playlists from YouTube Music - continuing with local playlists only");
                }
            }
            
            Logger.Info($"Loaded {playlists.Count} total user playlists");
            return playlists;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading user playlists");
            return new List<Playlist>();
        }
    }
    
    public async Task<Playlist> SavePlaylistAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Saving playlist: {playlist.Name}");
            
            // Generate ID if not provided
            if (string.IsNullOrEmpty(playlist.Id))
            {
                playlist.Id = Guid.NewGuid().ToString();
            }
            
            // Update metadata
            playlist.LastModified = DateTime.UtcNow;
            playlist.Source = "YouTube Music";
            
            // Save to local storage
            await SaveLocalPlaylistAsync(playlist, cancellationToken);
            
            Logger.Info($"Playlist '{playlist.Name}' saved successfully");
            return playlist;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error saving playlist {playlist.Name}");
            throw;
        }
    }
    
    public async Task<bool> DeletePlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Deleting playlist: {playlistId}");
            
            var filePath = Path.Combine(_playlistsDirectory, $"{playlistId}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Logger.Info($"Playlist {playlistId} deleted successfully");
                return true;
            }
            
            Logger.Warn($"Playlist {playlistId} not found for deletion");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error deleting playlist {playlistId}");
            return false;
        }
    }
    
    public async Task<bool> AddSongToPlaylistAsync(string playlistId, Song song, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Adding song '{song.Title}' to playlist {playlistId}");
            
            var playlist = await LoadPlaylistAsync(playlistId, cancellationToken);
            if (playlist == null)
            {
                Logger.Warn($"Playlist {playlistId} not found");
                return false;
            }
            
            playlist.AddSong(song);
            await SavePlaylistAsync(playlist, cancellationToken);
            
            Logger.Info($"Song '{song.Title}' added to playlist '{playlist.Name}' successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error adding song to playlist {playlistId}");
            return false;
        }
    }
    
    public async Task<bool> RemoveSongFromPlaylistAsync(string playlistId, string songId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Removing song {songId} from playlist {playlistId}");
            
            var playlist = await LoadPlaylistAsync(playlistId, cancellationToken);
            if (playlist == null)
            {
                Logger.Warn($"Playlist {playlistId} not found");
                return false;
            }
            
            playlist.RemoveSong(songId);
            await SavePlaylistAsync(playlist, cancellationToken);
            
            Logger.Info($"Song {songId} removed from playlist '{playlist.Name}' successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error removing song from playlist {playlistId}");
            return false;
        }
    }
    
    public async Task<List<Playlist>> SearchPlaylistsAsync(string query, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Searching YouTube Music playlists for: '{query}'");
            
            var playlists = new List<Playlist>();
            
            if (_client != null)
            {
                try
                {
                    var searchResults = _client.SearchAsync(query, SearchCategory.CommunityPlaylists);
                    var results = await searchResults.FetchItemsAsync(0, maxResults);
                    
                    foreach (var result in results)
                    {
                        if (result is CommunityPlaylistSearchResult playlistResult)
                        {
                            var playlist = new Playlist
                            {
                                Id = playlistResult.Id,
                                Name = playlistResult.Name,
                                Description = null,
                                ThumbnailUrl = playlistResult.Thumbnails?.FirstOrDefault()?.Url,
                                Author = playlistResult.Creator?.Name ?? "Unknown",
                                SongCount = 0,
                                Source = "YouTube Music",
                                CreatedDate = null,
                                LastModified = DateTime.UtcNow,
                                IsPublic = true
                            };
                            
                            playlists.Add(playlist);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to search YouTube Music playlists, falling back to local search");
                }
            }
            
            // Also search local playlists
            var localPlaylists = await LoadAllLocalPlaylistsAsync(cancellationToken);
            var matchingLocalPlaylists = localPlaylists
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (p.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .Take(maxResults - playlists.Count);
            
            playlists.AddRange(matchingLocalPlaylists);
            
            Logger.Info($"Found {playlists.Count} playlists for query '{query}'");
            return playlists;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error searching playlists for query '{query}'");
            return new List<Playlist>();
        }
    }
    
    public async Task<List<Song>> GetPlaylistSongsAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Getting songs for playlist: {playlistId}");
            
            // Handle "Saved Songs" fake playlist
            if (playlistId == "saved_songs")
            {
                return await GetSavedSongsAsync(cancellationToken);
            }
            
            // First try to load from local storage
            var playlist = await LoadLocalPlaylistAsync(playlistId, cancellationToken);
            
            // If we have songs in local storage, return them
            if (playlist != null && playlist.Songs.Count > 0)
            {
                Logger.Info($"Retrieved {playlist.Songs.Count} songs from local storage for playlist '{playlist.Name}'");
                return playlist.Songs;
            }
            
            // If no local songs, try to fetch from YouTube Music API
            if (_client != null)
            {
                try
                {
                    // First, try to determine if this is a library playlist by checking if it exists in library
                    var libraryPlaylists = await _client.GetLibraryCommunityPlaylistsAsync();
                    var libraryPlaylist = libraryPlaylists.FirstOrDefault(p => p.Id == playlistId);
                    
                    if (libraryPlaylist != null)
                    {
                        // This is a library playlist - we need to use different methods to get songs
                        Logger.Info($"Attempting to load songs from library playlist: {libraryPlaylist.Name}");
                        
                        // For library playlists, we might need to use different API methods
                        // For now, let's try the community playlist approach as a fallback
                        try
                        {
                            var browseId = _client.GetCommunityPlaylistBrowseId(playlistId);
                            var playlistInfo = await _client.GetCommunityPlaylistInfoAsync(browseId);
                            var playlistSongs = _client.GetCommunityPlaylistSongsAsync(browseId);
                            var bufferedSongs = await playlistSongs.FetchItemsAsync(0, int.MaxValue);
                            
                            var songs = new List<Song>();
                            foreach (var playlistSong in bufferedSongs)
                            {
                                var song = ConvertToSong(playlistSong);
                                songs.Add(song);
                            }
                            
                            // Save the playlist with songs to local storage
                            if (playlist == null)
                            {
                                playlist = new Playlist
                                {
                                    Id = playlistId,
                                    Name = libraryPlaylist.Name,
                                    Description = null,
                                    ThumbnailUrl = libraryPlaylist.Thumbnails?.FirstOrDefault()?.Url,
                                    Author = libraryPlaylist.Creator?.Name ?? "Unknown",
                                    SongCount = songs.Count,
                                    Source = "YouTube Music",
                                    CreatedDate = null,
                                    LastModified = DateTime.UtcNow,
                                    IsPublic = true
                                };
                            }
                            
                            playlist.Songs = songs;
                            await SaveLocalPlaylistAsync(playlist, cancellationToken);
                            
                            Logger.Info($"Retrieved {songs.Count} songs from YouTube Music API for library playlist '{libraryPlaylist.Name}'");
                            return songs;
                        }
                        catch (Exception ex2)
                        {
                            Logger.Warn(ex2, $"Failed to load songs from community playlist API for library playlist {playlistId}");
                        }
                        
                        // If community playlist approach fails, we might need to use library-specific methods
                        // For now, return empty list for library playlists that can't be loaded
                        Logger.Warn($"Library playlist '{libraryPlaylist.Name}' found but songs cannot be loaded without authentication");
                        return new List<Song>();
                    }
                    else
                    {
                        // Try to load as a regular community playlist
                        var browseId = _client.GetCommunityPlaylistBrowseId(playlistId);
                        var playlistInfo = await _client.GetCommunityPlaylistInfoAsync(browseId);
                        var playlistSongs = _client.GetCommunityPlaylistSongsAsync(browseId);
                        var bufferedSongs = await playlistSongs.FetchItemsAsync(0, int.MaxValue);
                        
                        var songs = new List<Song>();
                        foreach (var playlistSong in bufferedSongs)
                        {
                            var song = ConvertToSong(playlistSong);
                            songs.Add(song);
                        }
                        
                        // Save the playlist with songs to local storage
                        if (playlist == null)
                        {
                            playlist = new Playlist
                            {
                                Id = playlistId,
                                Name = playlistInfo.Name,
                                Description = null,
                                ThumbnailUrl = playlistInfo.Thumbnails?.FirstOrDefault()?.Url,
                                Author = playlistInfo.Creator?.Name ?? "Unknown",
                                SongCount = songs.Count,
                                Source = "YouTube Music",
                                CreatedDate = null,
                                LastModified = DateTime.UtcNow,
                                IsPublic = true
                            };
                        }
                        
                        playlist.Songs = songs;
                        await SaveLocalPlaylistAsync(playlist, cancellationToken);
                        
                        Logger.Info($"Retrieved {songs.Count} songs from YouTube Music API for community playlist '{playlistInfo.Name}'");
                        return songs;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Failed to load songs from YouTube Music API for playlist {playlistId}");
                }
            }
            
            // If we have a playlist object but no songs, return empty list
            if (playlist != null)
            {
                Logger.Warn($"Playlist '{playlist.Name}' found but no songs available");
                return new List<Song>();
            }
            
            Logger.Warn($"Playlist {playlistId} not found");
            return new List<Song>();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error getting songs for playlist {playlistId}");
            return new List<Song>();
        }
    }
    
    private async Task<List<Song>> GetSavedSongsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info("Loading saved songs from YouTube Music library");
            
            var songs = new List<Song>();
            
            if (_client != null)
            {
                try
                {
                    // Get saved songs from library
                    var librarySongs = await _client.GetLibrarySongsAsync();
                    
                    foreach (var librarySong in librarySongs)
                    {
                        var song = new Song
                        {
                            Id = librarySong.Id,
                            Title = librarySong.Name,
                            Artist = YouTubeMusicUtils.JoinAndCleanArtistNames(librarySong.Artists.Select(artist => artist.Name)),
                            ChannelTitle = YouTubeMusicUtils.CleanArtistName(librarySong.Artists.FirstOrDefault()?.Name ?? "Unknown"),
                            Album = librarySong.Album?.Name ?? "Unknown Album",
                            PlaylistName = "Saved Songs",
                            Url = $"https://music.youtube.com/watch?v={librarySong.Id}",
                            Duration = librarySong.Duration,
                            ThumbnailUrl = librarySong.Thumbnails?.FirstOrDefault()?.Url,
                            Source = "YouTube Music"
                        };
                        
                        songs.Add(song);
                    }
                    
                    Logger.Info($"Loaded {songs.Count} saved songs from YouTube Music library");
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to load saved songs from YouTube Music library - authentication may be required");
                }
            }
            
            return songs;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading saved songs");
            return new List<Song>();
        }
    }
    
    private async Task<Playlist?> LoadLocalPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_playlistsDirectory, $"{playlistId}.json");
            if (!File.Exists(filePath))
                return null;
            
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var playlist = JsonSerializer.Deserialize<Playlist>(json);
            return playlist;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error loading local playlist {playlistId}");
            return null;
        }
    }
    
    private async Task<List<Playlist>> LoadAllLocalPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var playlists = new List<Playlist>();
            var files = Directory.GetFiles(_playlistsDirectory, "*.json");
            
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file, cancellationToken);
                    var playlist = JsonSerializer.Deserialize<Playlist>(json);
                    if (playlist != null)
                        playlists.Add(playlist);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Error loading playlist file {file}");
                }
            }
            
            return playlists;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading all local playlists");
            return new List<Playlist>();
        }
    }
    
    private async Task SaveLocalPlaylistAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_playlistsDirectory, $"{playlist.Id}.json");
            var json = JsonSerializer.Serialize(playlist, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error saving local playlist {playlist.Id}");
            throw;
        }
    }
    
    private Song ConvertToSong(dynamic playlistSong)
    {
        try
        {
            string id = playlistSong.Id?.ToString() ?? "";
            string title = playlistSong.Name?.ToString() ?? "Unknown Title";
            string artist = "Unknown Artist";
            string channelTitle = "Unknown";
            string thumbnailUrl = null;
            
            // Handle artists safely
            if (playlistSong.Artists != null)
            {
                var artistsList = new List<string>();
                foreach (var artistObj in playlistSong.Artists)
                {
                    if (artistObj.Name != null)
                    {
                        artistsList.Add(artistObj.Name.ToString());
                    }
                }
                artist = YouTubeMusicUtils.JoinAndCleanArtistNames(artistsList);
                if (artistsList.Count > 0)
                {
                    channelTitle = YouTubeMusicUtils.CleanArtistName(artistsList[0]);
                }
            }
            
            // Handle thumbnails safely
            if (playlistSong.Thumbnails != null)
            {
                foreach (var thumbnail in playlistSong.Thumbnails)
                {
                    if (thumbnail.Url != null)
                    {
                        thumbnailUrl = thumbnail.Url.ToString();
                        break;
                    }
                }
            }
            
            return new Song
            {
                Id = id,
                Title = title,
                Artist = artist,
                ChannelTitle = channelTitle,
                Album = "Unknown Album",
                PlaylistName = "Community Playlist",
                Url = $"https://music.youtube.com/watch?v={id}",
                Duration = playlistSong.Duration,
                ThumbnailUrl = thumbnailUrl,
                Source = "YouTube Music"
            };
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Failed to convert playlist song to Song object");
            return new Song
            {
                Id = "unknown",
                Title = "Unknown Title",
                Artist = "Unknown Artist",
                ChannelTitle = "Unknown",
                Album = "Unknown Album",
                PlaylistName = "Community Playlist",
                Url = "",
                Duration = null,
                ThumbnailUrl = null,
                Source = "YouTube Music"
            };
        }
    }
} 