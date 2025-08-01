using System.Text.Json;
using NLog;
using SpotifyAPI.Web;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;

namespace StreamingPlayerNET.Source.Spotify.Services;

public class SpotifySearchService : ISearchService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SpotifyClient _spotifyClient;
    private readonly SpotifySourceSettings _settings;

    public SpotifySearchService(SpotifyClient spotifyClient, SpotifySourceSettings settings)
    {
        _spotifyClient = spotifyClient;
        _settings = settings;
    }

    public async Task<List<Song>> SearchAsync(string query, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Searching Spotify for: {query}");
            
            var searchRequest = new SearchRequest(SearchRequest.Types.Track, query)
            {
                Limit = Math.Min(maxResults, _settings.MaxSearchResults),
                Offset = 0
            };

            var searchResponse = await _spotifyClient.Search.Item(searchRequest);
            var songs = new List<Song>();

            if (searchResponse.Tracks?.Items != null)
            {
                foreach (var track in searchResponse.Tracks.Items)
                {
                    var song = ConvertTrackToSong(track);
                    songs.Add(song);
                }
            }

            Logger.Debug($"Found {songs.Count} tracks for query: {query}");
            return songs;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error searching Spotify for query: {query}");
            return new List<Song>();
        }
    }

    public async Task<List<Song>> SearchByArtistAsync(string artist, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Searching Spotify for artist: {artist}");
            
            // First, search for the artist
            var artistSearchRequest = new SearchRequest(SearchRequest.Types.Artist, artist)
            {
                Limit = 1
            };

            var artistSearchResponse = await _spotifyClient.Search.Item(artistSearchRequest);
            var spotifyArtist = artistSearchResponse.Artists?.Items?.FirstOrDefault();

            if (spotifyArtist == null)
            {
                Logger.Warn($"Artist not found: {artist}");
                return new List<Song>();
            }

            // Get top tracks for the artist
            var topTracksRequest = new ArtistsTopTracksRequest("US"); // Using US market as default
            var topTracks = await _spotifyClient.Artists.GetTopTracks(spotifyArtist.Id, topTracksRequest);

            var songs = new List<Song>();
            foreach (var track in topTracks.Tracks.Take(maxResults))
            {
                var song = ConvertTrackToSong(track);
                songs.Add(song);
            }

            Logger.Debug($"Found {songs.Count} tracks for artist: {artist}");
            return songs;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error searching Spotify for artist: {artist}");
            return new List<Song>();
        }
    }

    public async Task<List<Song>> SearchByPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Searching Spotify playlist: {playlistId}");
            
            var playlistRequest = new PlaylistGetRequest();
            var playlist = await _spotifyClient.Playlists.Get(playlistId, playlistRequest);

            var songs = new List<Song>();
            if (playlist.Tracks?.Items != null)
            {
                foreach (var item in playlist.Tracks.Items)
                {
                    if (item.Track is FullTrack track)
                    {
                        var song = ConvertTrackToSong(track);
                        song.PlaylistName = playlist.Name ?? "Unknown Playlist";
                        songs.Add(song);
                    }
                }
            }

            Logger.Debug($"Found {songs.Count} tracks in playlist: {playlist.Name ?? "Unknown Playlist"}");
            return songs;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error searching Spotify playlist: {playlistId}");
            return new List<Song>();
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

    private static Song ConvertTrackToSong(FullTrack track)
    {
        return new Song
        {
            Id = track.Id,
            Title = track.Name,
            Artist = track.Artists?.FirstOrDefault()?.Name ?? "Unknown Artist",
            Album = track.Album?.Name,
            Duration = track.DurationMs > 0 ? TimeSpan.FromMilliseconds(track.DurationMs) : null,
            ThumbnailUrl = track.Album?.Images?.FirstOrDefault()?.Url,
            Url = track.ExternalUrls?.FirstOrDefault().Value,
            Source = "Spotify",
            State = PlaybackState.Stopped
        };
    }
} 