using NLog;
using SpotifyAPI.Web;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;

namespace StreamingPlayerNET.Source.Spotify.Services;

public class SpotifyPlaylistService : IPlaylistService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SpotifyClient _spotifyClient;
    private readonly SpotifySourceSettings _settings;

    public SpotifyPlaylistService(SpotifyClient spotifyClient, SpotifySourceSettings settings)
    {
        _spotifyClient = spotifyClient;
        _settings = settings;
    }

    public bool IsAvailable => _spotifyClient != null;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Logger.Debug("Spotify Playlist Service initialized");
        return Task.CompletedTask;
    }

    public async Task<Playlist?> LoadPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Loading Spotify playlist: {playlistId}");
            
            var playlistRequest = new PlaylistGetRequest();
            var playlist = await _spotifyClient.Playlists.Get(playlistId, playlistRequest);

            var convertedPlaylist = new Playlist
            {
                Id = playlist.Id ?? string.Empty,
                Name = playlist.Name ?? "Unknown Playlist",
                Description = playlist.Description ?? string.Empty,
                ThumbnailUrl = playlist.Images?.FirstOrDefault()?.Url,
                SongCount = playlist.Tracks?.Total ?? 0,
                Source = "Spotify"
            };

            Logger.Debug($"Loaded playlist: {playlist.Name ?? "Unknown Playlist"} with {convertedPlaylist.SongCount} tracks");
            return convertedPlaylist;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error loading Spotify playlist: {playlistId}");
            return null;
        }
    }

    public async Task<List<Playlist>> LoadUserPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug("Loading user's Spotify playlists");
            
            var playlistsRequest = new PlaylistCurrentUsersRequest();
            var playlists = await _spotifyClient.Playlists.CurrentUsers(playlistsRequest);

            var convertedPlaylists = new List<Playlist>();
            if (playlists?.Items != null)
            {
                foreach (var playlist in playlists.Items)
                {
                    var convertedPlaylist = new Playlist
                    {
                        Id = playlist.Id ?? string.Empty,
                        Name = playlist.Name ?? "Unknown Playlist",
                        Description = playlist.Description ?? string.Empty,
                        ThumbnailUrl = playlist.Images?.FirstOrDefault()?.Url,
                        SongCount = playlist.Tracks?.Total ?? 0,
                        Source = "Spotify"
                    };
                    convertedPlaylists.Add(convertedPlaylist);
                }
            }

            Logger.Debug($"Loaded {convertedPlaylists.Count} user playlists");
            return convertedPlaylists;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading user's Spotify playlists");
            return new List<Playlist>();
        }
    }

    public async Task<Playlist> SavePlaylistAsync(Playlist playlist, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Saving Spotify playlist: {playlist.Name}");
            
            // Create a new playlist
            var createRequest = new PlaylistCreateRequest(playlist.Name)
            {
                Description = playlist.Description
            };

            var newPlaylist = await _spotifyClient.Playlists.Create(await GetCurrentUserIdAsync(), createRequest);

            var savedPlaylist = new Playlist
            {
                Id = newPlaylist.Id ?? string.Empty,
                Name = newPlaylist.Name ?? "Unknown Playlist",
                Description = newPlaylist.Description ?? string.Empty,
                ThumbnailUrl = newPlaylist.Images?.FirstOrDefault()?.Url,
                SongCount = 0,
                Source = "Spotify"
            };

            Logger.Debug($"Created new playlist: {savedPlaylist.Name} with ID: {savedPlaylist.Id}");
            return savedPlaylist;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error saving Spotify playlist: {playlist.Name}");
            return playlist;
        }
    }

    public Task<bool> DeletePlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Deleting Spotify playlist: {playlistId}");
            
            // Note: Spotify API doesn't provide a direct delete method for playlists
            // Users can only unfollow playlists they don't own
            // For owned playlists, they would need to be deleted through the Spotify app
            Logger.Warn("Playlist deletion not supported through Spotify API - user must delete through Spotify app");
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error deleting Spotify playlist: {playlistId}");
            return Task.FromResult(false);
        }
    }

    public async Task<bool> AddSongToPlaylistAsync(string playlistId, Song song, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Adding song {song.Title} to Spotify playlist: {playlistId}");
            
            var addRequest = new PlaylistAddItemsRequest(new List<string> { song.Id });
            await _spotifyClient.Playlists.AddItems(playlistId, addRequest);

            Logger.Debug($"Successfully added song to playlist");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error adding song to Spotify playlist: {playlistId}");
            return false;
        }
    }

    public async Task<bool> RemoveSongFromPlaylistAsync(string playlistId, string songId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Removing song {songId} from Spotify playlist: {playlistId}");
            
            var removeRequest = new PlaylistRemoveItemsRequest();
            if (removeRequest.Tracks != null)
            {
                removeRequest.Tracks.Add(new PlaylistRemoveItemsRequest.Item { Uri = $"spotify:track:{songId}" });
            }
            
            await _spotifyClient.Playlists.RemoveItems(playlistId, removeRequest);

            Logger.Debug($"Successfully removed song from playlist");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error removing song from Spotify playlist: {playlistId}");
            return false;
        }
    }

    public async Task<List<Playlist>> SearchPlaylistsAsync(string query, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Searching Spotify playlists for: {query}");
            
            var searchRequest = new SearchRequest(SearchRequest.Types.Playlist, query)
            {
                Limit = Math.Min(maxResults, _settings.MaxSearchResults),
                Offset = 0
            };

            var searchResponse = await _spotifyClient.Search.Item(searchRequest);
            var playlists = new List<Playlist>();

            if (searchResponse.Playlists?.Items != null)
            {
                foreach (var playlist in searchResponse.Playlists.Items)
                {
                    var convertedPlaylist = new Playlist
                    {
                        Id = playlist.Id ?? string.Empty,
                        Name = playlist.Name ?? "Unknown Playlist",
                        Description = playlist.Description ?? string.Empty,
                        ThumbnailUrl = playlist.Images?.FirstOrDefault()?.Url,
                        SongCount = playlist.Tracks?.Total ?? 0,
                        Source = "Spotify"
                    };
                    playlists.Add(convertedPlaylist);
                }
            }

            Logger.Debug($"Found {playlists.Count} playlists for query: {query}");
            return playlists;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error searching Spotify playlists for query: {query}");
            return new List<Playlist>();
        }
    }

    public async Task<List<Song>> GetPlaylistSongsAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Getting songs from Spotify playlist: {playlistId}");
            
            var playlistRequest = new PlaylistGetRequest();
            var playlist = await _spotifyClient.Playlists.Get(playlistId, playlistRequest);

            var songs = new List<Song>();
            if (playlist.Tracks?.Items != null)
            {
                foreach (var item in playlist.Tracks.Items)
                {
                    if (item.Track is FullTrack track)
                    {
                        var song = new Song
                        {
                            Id = track.Id,
                            Title = track.Name,
                            Artist = track.Artists?.FirstOrDefault()?.Name ?? "Unknown Artist",
                            Album = track.Album?.Name,
                            Duration = track.DurationMs > 0 ? TimeSpan.FromMilliseconds(track.DurationMs) : null,
                            ThumbnailUrl = track.Album?.Images?.FirstOrDefault()?.Url,
                            Url = track.ExternalUrls?.FirstOrDefault().Value,
                            PlaylistName = playlist.Name ?? "Unknown Playlist",
                            Source = "Spotify"
                        };
                        songs.Add(song);
                    }
                }
            }

            Logger.Debug($"Retrieved {songs.Count} songs from playlist: {playlist.Name ?? "Unknown Playlist"}");
            return songs;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error getting songs from Spotify playlist: {playlistId}");
            return new List<Song>();
        }
    }

    private async Task<string> GetCurrentUserIdAsync()
    {
        try
        {
            var profile = await _spotifyClient.UserProfile.Current();
            return profile.Id;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error getting current user ID");
            throw;
        }
    }
} 