using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using StreamingPlayerNET.Source.YoutubeDL;
using NLog;
using System.Text.Json;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Search;
using YoutubeExplode.Playlists;
using System.Net.Http;
using System.Text;
using System.Linq;

namespace StreamingPlayerNET.Source.YoutubeDL.Services;

public class YouTubePlaylistService : IPlaylistService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly string _playlistsDirectory;
    private readonly YoutubeClient _youtubeClient;
    private readonly HttpClient _httpClient;
    private YouTubeSourceSettings? _settings;
    private bool _initialized = false;
    
    public bool IsAvailable => _initialized;
    
    public YouTubePlaylistService()
    {
        _playlistsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData", "Playlists", "YouTube");
        _youtubeClient = new YoutubeClient();
        _httpClient = new HttpClient();
        
        // Ensure the playlists directory exists
        Directory.CreateDirectory(_playlistsDirectory);
    }
    
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) return;
        
        try
        {
            Logger.Info("Initializing YouTube Playlist Service...");
            
            // Load settings
            _settings = new YouTubeSourceSettings();
            await _settings.LoadAsync();
            
            // Configure HTTP client timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings?.RequestTimeoutSeconds ?? 30);
            
            _initialized = true;
            Logger.Info("YouTube Playlist Service initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize YouTube Playlist Service");
            throw;
        }
    }
    
    public async Task<StreamingPlayerNET.Common.Models.Playlist?> LoadPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Loading YouTube playlist: {playlistId}");
            
            // First try to load from local storage
            var localPlaylist = await LoadLocalPlaylistAsync(playlistId, cancellationToken);
            if (localPlaylist != null)
            {
                Logger.Info($"Loaded playlist '{localPlaylist.Name}' from local storage");
                return localPlaylist;
            }
            
            // If not found locally and API is enabled, try to fetch from YouTube
            if (_settings?.EnableYouTubeApi == true && !string.IsNullOrEmpty(_settings.YouTubeApiKey))
            {
                var youtubePlaylist = await LoadFromYouTubeApiAsync(playlistId, cancellationToken);
                if (youtubePlaylist != null)
                {
                    // Cache the playlist locally
                    await SaveLocalPlaylistAsync(youtubePlaylist, cancellationToken);
                    return youtubePlaylist;
                }
            }
            
            // Try using YoutubeExplode to fetch playlist
            var explodePlaylist = await LoadFromYoutubeExplodeAsync(playlistId, cancellationToken);
            if (explodePlaylist != null)
            {
                // Cache the playlist locally
                await SaveLocalPlaylistAsync(explodePlaylist, cancellationToken);
                return explodePlaylist;
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
    
    public async Task<List<StreamingPlayerNET.Common.Models.Playlist>> LoadUserPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info("Loading user's YouTube playlists");
            
            var playlists = new List<StreamingPlayerNET.Common.Models.Playlist>();
            
            // Load local playlists
            var localPlaylists = await LoadAllLocalPlaylistsAsync(cancellationToken);
            playlists.AddRange(localPlaylists);
            
            // If API is enabled, try to fetch from YouTube API
            if (_settings?.EnableYouTubeApi == true && !string.IsNullOrEmpty(_settings.YouTubeApiKey))
            {
                try
                {
                    var apiPlaylists = await LoadFromYouTubeApiAsync(cancellationToken);
                    playlists.AddRange(apiPlaylists);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to load playlists from YouTube API");
                }
            }
            
            Logger.Info($"Loaded {playlists.Count} user playlists");
            return playlists;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading user playlists");
            return new List<StreamingPlayerNET.Common.Models.Playlist>();
        }
    }
    
    public async Task<StreamingPlayerNET.Common.Models.Playlist> SavePlaylistAsync(StreamingPlayerNET.Common.Models.Playlist playlist, CancellationToken cancellationToken = default)
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
            playlist.Source = "YouTube";
            
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
    
    public async Task<List<StreamingPlayerNET.Common.Models.Playlist>> SearchPlaylistsAsync(string query, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Searching YouTube playlists for: '{query}'");
            
            var playlists = new List<StreamingPlayerNET.Common.Models.Playlist>();
            
            // Search local playlists
            var localPlaylists = await LoadAllLocalPlaylistsAsync(cancellationToken);
            var matchingLocalPlaylists = localPlaylists
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (p.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .Take(maxResults);
            playlists.AddRange(matchingLocalPlaylists);
            
            // Search using YoutubeExplode
            try
            {
                var searchResults = await SearchWithYoutubeExplodeAsync(query, maxResults, cancellationToken);
                playlists.AddRange(searchResults);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to search playlists with YoutubeExplode");
            }
            
            // Remove duplicates based on ID
            var uniquePlaylists = playlists.GroupBy(p => p.Id).Select(g => g.First()).Take(maxResults).ToList();
            
            Logger.Info($"Found {uniquePlaylists.Count} playlists for query '{query}'");
            return uniquePlaylists;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error searching playlists for query '{query}'");
            return new List<StreamingPlayerNET.Common.Models.Playlist>();
        }
    }
    
    public async Task<List<Song>> GetPlaylistSongsAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info($"Getting songs for playlist: {playlistId}");
            
            var playlist = await LoadPlaylistAsync(playlistId, cancellationToken);
            if (playlist == null)
            {
                Logger.Warn($"Playlist {playlistId} not found");
                return new List<Song>();
            }
            
            // If playlist has no songs and it's a YouTube playlist ID, try to fetch songs
            if (playlist.Songs.Count == 0 && IsYouTubePlaylistId(playlistId))
            {
                try
                {
                    var songs = await GetPlaylistSongsFromYouTubeAsync(playlistId, cancellationToken);
                    playlist.Songs.AddRange(songs);
                    await SavePlaylistAsync(playlist, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Failed to fetch songs for playlist {playlistId} from YouTube");
                }
            }
            
            Logger.Info($"Retrieved {playlist.Songs.Count} songs from playlist '{playlist.Name}'");
            return playlist.Songs;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error getting songs for playlist {playlistId}");
            return new List<Song>();
        }
    }
    
    private async Task<StreamingPlayerNET.Common.Models.Playlist?> LoadFromYouTubeApiAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings?.YouTubeApiKey))
            return null;
            
        try
        {
            var url = $"https://www.googleapis.com/youtube/v3/playlists?part=snippet&id={playlistId}&key={_settings.YouTubeApiKey}";
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var jsonDoc = JsonDocument.Parse(response);
            
            var items = jsonDoc.RootElement.GetProperty("items");
            if (items.GetArrayLength() == 0)
                return null;
                
            var playlistInfo = items[0].GetProperty("snippet");
            var playlist = new StreamingPlayerNET.Common.Models.Playlist
            {
                Id = playlistId,
                Name = playlistInfo.GetProperty("title").GetString() ?? "Unknown Playlist",
                Description = playlistInfo.GetProperty("description").GetString(),
                Source = "YouTube",
                CreatedDate = DateTime.Parse(playlistInfo.GetProperty("publishedAt").GetString() ?? DateTime.UtcNow.ToString()),
                LastModified = DateTime.UtcNow
            };
            
            // Fetch playlist items
            var songs = await GetPlaylistSongsFromYouTubeApiAsync(playlistId, cancellationToken);
            playlist.Songs.AddRange(songs);
            
            return playlist;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to load playlist {playlistId} from YouTube API");
            return null;
        }
    }
    
    private async Task<List<StreamingPlayerNET.Common.Models.Playlist>> LoadFromYouTubeApiAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings?.YouTubeApiKey))
            return new List<StreamingPlayerNET.Common.Models.Playlist>();
            
        try
        {
            // This would require OAuth2 authentication for user's playlists
            // For now, we'll return an empty list as this requires user authentication
            Logger.Info("YouTube API user playlist loading requires OAuth2 authentication");
            return new List<StreamingPlayerNET.Common.Models.Playlist>();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load playlists from YouTube API");
            return new List<StreamingPlayerNET.Common.Models.Playlist>();
        }
    }
    
    private async Task<StreamingPlayerNET.Common.Models.Playlist?> LoadFromYoutubeExplodeAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            var youtubePlaylist = await _youtubeClient.Playlists.GetAsync(playlistId, cancellationToken);
            
            var result = new StreamingPlayerNET.Common.Models.Playlist
            {
                Id = playlistId,
                Name = youtubePlaylist.Title,
                Description = youtubePlaylist.Description,
                Source = "YouTube",
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
            
            // Fetch playlist videos manually
            var videos = new List<YoutubeExplode.Playlists.PlaylistVideo>();
            var maxItems = _settings?.MaxPlaylistItems ?? 100;
            var count = 0;
            
            await foreach (var video in _youtubeClient.Playlists.GetVideosAsync(playlistId, cancellationToken))
            {
                if (count >= maxItems) break;
                videos.Add(video);
                count++;
            }
                
            foreach (var video in videos)
            {
                var song = new Song
                {
                    Id = video.Id.Value,
                    Title = video.Title,
                    Artist = video.Author.ChannelTitle,
                    Album = "YouTube Playlist",
                    Duration = video.Duration ?? TimeSpan.Zero,
                    Source = "YouTube",
                    Url = $"https://www.youtube.com/watch?v={video.Id}",
                    ThumbnailUrl = video.Thumbnails.FirstOrDefault()?.Url
                };
                
                result.AddSong(song);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to load playlist {playlistId} with YoutubeExplode");
            return null;
        }
    }
    
    private async Task<List<StreamingPlayerNET.Common.Models.Playlist>> SearchWithYoutubeExplodeAsync(string query, int maxResults, CancellationToken cancellationToken = default)
    {
        try
        {
            var playlists = new List<StreamingPlayerNET.Common.Models.Playlist>();
            var searchResults = _youtubeClient.Search.GetPlaylistsAsync(query, cancellationToken);
            
            var count = 0;
            await foreach (var youtubePlaylist in searchResults.WithCancellation(cancellationToken))
            {
                if (count >= maxResults) break;
                
                var result = new StreamingPlayerNET.Common.Models.Playlist
                {
                    Id = youtubePlaylist.Id.Value,
                    Name = youtubePlaylist.Title,
                    Description = null, // Description not available in PlaylistSearchResult
                    Source = "YouTube",
                    CreatedDate = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };
                
                playlists.Add(result);
                count++;
            }
            
            return playlists;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to search playlists with YoutubeExplode for query '{query}'");
            return new List<StreamingPlayerNET.Common.Models.Playlist>();
        }
    }
    
    private async Task<List<Song>> GetPlaylistSongsFromYouTubeAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            var songs = new List<Song>();
            var maxItems = _settings?.MaxPlaylistItems ?? 100;
            var count = 0;
            
            await foreach (var video in _youtubeClient.Playlists.GetVideosAsync(playlistId, cancellationToken))
            {
                if (count >= maxItems) break;
                
                var song = new Song
                {
                    Id = video.Id.Value,
                    Title = video.Title,
                    Artist = video.Author.ChannelTitle,
                    Album = "YouTube Playlist",
                    Duration = video.Duration ?? TimeSpan.Zero,
                    Source = "YouTube",
                    Url = $"https://www.youtube.com/watch?v={video.Id}",
                    ThumbnailUrl = video.Thumbnails.FirstOrDefault()?.Url
                };
                
                songs.Add(song);
                count++;
            }
            
            return songs;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to get songs for playlist {playlistId} from YouTube");
            return new List<Song>();
        }
    }
    
    private async Task<List<Song>> GetPlaylistSongsFromYouTubeApiAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings?.YouTubeApiKey))
            return new List<Song>();
            
        try
        {
            var songs = new List<Song>();
            var nextPageToken = "";
            var maxResults = _settings?.MaxApiResults ?? 50;
            
            do
            {
                var url = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={playlistId}&maxResults={maxResults}&key={_settings.YouTubeApiKey}";
                if (!string.IsNullOrEmpty(nextPageToken))
                    url += $"&pageToken={nextPageToken}";
                    
                var response = await _httpClient.GetStringAsync(url, cancellationToken);
                var jsonDoc = JsonDocument.Parse(response);
                
                var items = jsonDoc.RootElement.GetProperty("items");
                foreach (var item in items.EnumerateArray())
                {
                    var snippet = item.GetProperty("snippet");
                    var videoId = snippet.GetProperty("resourceId").GetProperty("videoId").GetString();
                    var title = snippet.GetProperty("title").GetString();
                    var channelTitle = snippet.GetProperty("videoOwnerChannelTitle").GetString();
                    
                    var song = new Song
                    {
                        Id = videoId ?? "",
                        Title = title ?? "Unknown Title",
                        Artist = channelTitle ?? "Unknown Artist",
                        Album = "YouTube Playlist",
                        Source = "YouTube",
                        Url = $"https://www.youtube.com/watch?v={videoId}"
                    };
                    
                    songs.Add(song);
                }
                
                // Get next page token
                if (jsonDoc.RootElement.TryGetProperty("nextPageToken", out var nextToken))
                    nextPageToken = nextToken.GetString() ?? "";
                else
                    nextPageToken = "";
                    
            } while (!string.IsNullOrEmpty(nextPageToken) && songs.Count < maxResults);
            
            return songs;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to get songs for playlist {playlistId} from YouTube API");
            return new List<Song>();
        }
    }
    
    private bool IsYouTubePlaylistId(string playlistId)
    {
        // YouTube playlist IDs typically start with PL, FL, or are 34 characters long
        return playlistId.StartsWith("PL") || playlistId.StartsWith("FL") || playlistId.Length == 34;
    }
    
    public async Task<bool> ValidateYouTubeApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(apiKey))
            return false;
            
        try
        {
            var url = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q=test&maxResults=1&key={apiKey}";
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var jsonDoc = JsonDocument.Parse(response);
            
            // Check if there's an error
            if (jsonDoc.RootElement.TryGetProperty("error", out var error))
            {
                Logger.Warn($"YouTube API key validation failed: {error}");
                return false;
            }
            
            Logger.Info("YouTube API key validation successful");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "YouTube API key validation failed");
            return false;
        }
    }
    
    public string GetYouTubeApiKeyInstructions()
    {
        return @"To get a YouTube API key:

1. Go to the Google Cloud Console: https://console.cloud.google.com/
2. Create a new project or select an existing one
3. Enable the YouTube Data API v3
4. Go to Credentials and create an API key
5. Restrict the API key to YouTube Data API v3 only
6. Copy the API key and paste it in the settings

Note: The API key is required for accessing YouTube playlists and channel data.";
    }
    
    private async Task<StreamingPlayerNET.Common.Models.Playlist?> LoadLocalPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_playlistsDirectory, $"{playlistId}.json");
            if (!File.Exists(filePath))
                return null;
            
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var playlist = JsonSerializer.Deserialize<StreamingPlayerNET.Common.Models.Playlist>(json);
            return playlist;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error loading local playlist {playlistId}");
            return null;
        }
    }
    
    private async Task<List<StreamingPlayerNET.Common.Models.Playlist>> LoadAllLocalPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var playlists = new List<StreamingPlayerNET.Common.Models.Playlist>();
            var files = Directory.GetFiles(_playlistsDirectory, "*.json");
            
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file, cancellationToken);
                    var playlist = JsonSerializer.Deserialize<StreamingPlayerNET.Common.Models.Playlist>(json);
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
            return new List<StreamingPlayerNET.Common.Models.Playlist>();
        }
    }
    
    private async Task SaveLocalPlaylistAsync(StreamingPlayerNET.Common.Models.Playlist playlist, CancellationToken cancellationToken = default)
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
} 