using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Info;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using StreamingPlayerNET.Source.YouTubeMusic.Utils;
using NLog;

namespace StreamingPlayerNET.Source.YouTubeMusic.Services;

public class YouTubeMusicSearchService : ISearchService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly YouTubeMusicClient _client;
    private readonly YouTubeMusicSourceSettings _settings;
    
    public YouTubeMusicSearchService(YouTubeMusicClient client, YouTubeMusicSourceSettings settings)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Logger.Info("YouTube Music Search Service initialized");
    }
    
    public async Task<List<Song>> SearchAsync(string query, int maxResults = 0, CancellationToken cancellationToken = default)
    {
        if (maxResults <= 0)
        {
            maxResults = _settings.MaxSearchResults;
        }
        
        Logger.Info($"Starting YouTube Music search for: '{query}' (max results: {maxResults})");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var songs = new List<Song>();
            
            // Search for songs directly
            var searchResults = _client.SearchAsync(query, SearchCategory.Songs);
            var bufferedResults = await searchResults.FetchItemsAsync(0, maxResults);
            
            foreach (var result in bufferedResults)
            {
                if (result is SongSearchResult songResult)
                {
                    var song = ConvertToSong(songResult);
                    songs.Add(song);
                }
            }
            
            // If videos are enabled, also search for videos
            if (_settings.IncludeVideos && songs.Count < maxResults)
            {
                var remainingResults = maxResults - songs.Count;
                var videoSearchResults = _client.SearchAsync(query, SearchCategory.Videos);
                var bufferedVideoResults = await videoSearchResults.FetchItemsAsync(0, remainingResults);
                
                foreach (var result in bufferedVideoResults)
                {
                    if (result is VideoSearchResult videoResult)
                    {
                        var song = ConvertToSong(videoResult);
                        songs.Add(song);
                    }
                }
            }
            
            Logger.Info($"YouTube Music search completed: Found {songs.Count} songs for query '{query}'");
            return songs;
        }, "search songs", cancellationToken);
    }
    
    public async Task<List<Song>> SearchByArtistAsync(string artist, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Starting YouTube Music artist search for: '{artist}'");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var songs = new List<Song>();
            
            // First search for the artist
            var artistSearchResults = _client.SearchAsync(artist, SearchCategory.Artists);
            var bufferedArtistResults = await artistSearchResults.FetchItemsAsync(0, 1);
            
            if (bufferedArtistResults.Any() && bufferedArtistResults.First() is ArtistSearchResult artistResult)
            {
                // Get artist info and songs
                var artistInfo = await _client.GetArtistInfoAsync(artistResult.Id);
                
                var count = 0;
                foreach (var artistSong in artistInfo.Songs)
                {
                    if (count >= maxResults) break;
                    
                    var song = ConvertToSong(artistSong);
                    songs.Add(song);
                    count++;
                }
            }
            else
            {
                // Fallback to regular search with artist name
                return await SearchAsync(artist, maxResults, cancellationToken);
            }
            
            Logger.Info($"YouTube Music artist search completed: Found {songs.Count} songs for artist '{artist}'");
            return songs;
        }, "search by artist", cancellationToken);
    }
    
    public async Task<List<Song>> SearchByPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Starting YouTube Music playlist search for playlist ID: {playlistId}");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var songs = new List<Song>();
            
            // Try to parse as a community playlist browse ID
            var browseId = _client.GetCommunityPlaylistBrowseId(playlistId);
            var playlistInfo = await _client.GetCommunityPlaylistInfoAsync(browseId);
            
            var playlistSongs = _client.GetCommunityPlaylistSongsAsync(browseId);
            var bufferedSongs = await playlistSongs.FetchItemsAsync(0, int.MaxValue);
            
            foreach (var playlistSong in bufferedSongs)
            {
                var song = ConvertToSong(playlistSong);
                songs.Add(song);
            }
            
            Logger.Info($"YouTube Music playlist search completed: Found {songs.Count} songs in playlist '{playlistInfo.Name}'");
            return songs;
        }, "search by playlist", cancellationToken);
    }
    
    public async Task<List<Playlist>> SearchPlaylistsAsync(string query, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Starting YouTube Music playlist search for: '{query}'");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var playlists = new List<Playlist>();
            
            var searchResults = _client.SearchAsync(query, SearchCategory.CommunityPlaylists);
            var bufferedResults = await searchResults.FetchItemsAsync(0, maxResults);
            
            foreach (var result in bufferedResults)
            {
                if (result is CommunityPlaylistSearchResult playlistResult)
                {
                    var playlist = new Playlist
                    {
                        Id = playlistResult.Id,
                        Name = playlistResult.Name,
                        Description = null, // Description not available in CommunityPlaylistSearchResult
                        ThumbnailUrl = playlistResult.Thumbnails?.FirstOrDefault()?.Url,
                        Author = playlistResult.Creator?.Name ?? "Unknown",
                        SongCount = 0, // SongCount not available in CommunityPlaylistSearchResult
                        CreatedDate = null, // Not available in search results
                        IsPublic = true
                    };
                    
                    playlists.Add(playlist);
                }
            }
            
            Logger.Info($"YouTube Music playlist search completed: Found {playlists.Count} playlists for query '{query}'");
            return playlists;
        }, "search playlists", cancellationToken);
    }
    
    private Song ConvertToSong(SongSearchResult songResult)
    {
        return new Song
        {
            Id = songResult.Id,
            Title = songResult.Name,
            Artist = YouTubeMusicUtils.JoinAndCleanArtistNames(songResult.Artists.Select(a => a.Name)),
            ChannelTitle = YouTubeMusicUtils.CleanArtistName(songResult.Artists.FirstOrDefault()?.Name ?? "Unknown"),
            Album = "Unknown Album", // Album info not available in search results
            PlaylistName = "YouTube Music Search",
            Url = $"https://music.youtube.com/watch?v={songResult.Id}",
            Duration = songResult.Duration,
            ThumbnailUrl = songResult.Thumbnails?.FirstOrDefault()?.Url,
            Description = null,
            UploadDate = null,
            ViewCount = null,
            LikeCount = null,
            Source = "YouTube Music"
        };
    }
    
    private Song ConvertToSong(VideoSearchResult videoResult)
    {
        return new Song
        {
            Id = videoResult.Id,
            Title = videoResult.Name,
            Artist = YouTubeMusicUtils.JoinAndCleanArtistNames(videoResult.Artists.Select(a => a.Name)),
            ChannelTitle = YouTubeMusicUtils.CleanArtistName(videoResult.Artists.FirstOrDefault()?.Name ?? "Unknown"),
            Album = "Unknown Album", // Album info not available in video search results
            PlaylistName = "YouTube Music Videos",
            Url = $"https://music.youtube.com/watch?v={videoResult.Id}",
            Duration = videoResult.Duration,
            ThumbnailUrl = videoResult.Thumbnails?.FirstOrDefault()?.Url,
            Description = null,
            UploadDate = null,
            ViewCount = null, // ViewCount not available directly in VideoSearchResult, only ViewsInfo string
            LikeCount = null,
            Source = "YouTube Music"
        };
    }
    
    private Song ConvertToSong(ArtistSong artistSong)
    {
        return new Song
        {
            Id = artistSong.Id,
            Title = artistSong.Name,
            Artist = YouTubeMusicUtils.JoinAndCleanArtistNames(artistSong.Artists.Select(a => a.Name)),
            ChannelTitle = YouTubeMusicUtils.CleanArtistName(artistSong.Artists.FirstOrDefault()?.Name ?? "Unknown"),
            Album = "Unknown Album", // Album info not available in artist songs
            PlaylistName = "Artist Songs",
            Url = $"https://music.youtube.com/watch?v={artistSong.Id}",
            Duration = null, // Duration not available in ArtistSong, only Playsinfo
            ThumbnailUrl = artistSong.Thumbnails?.FirstOrDefault()?.Url,
            Description = null,
            UploadDate = null,
            ViewCount = null,
            LikeCount = null,
            Source = "YouTube Music"
        };
    }
    
    private Song ConvertToSong(CommunityPlaylistSong playlistSong)
    {
        return new Song
        {
            Id = playlistSong.Id,
            Title = playlistSong.Name,
            Artist = YouTubeMusicUtils.JoinAndCleanArtistNames(playlistSong.Artists.Select(a => a.Name)),
            ChannelTitle = YouTubeMusicUtils.CleanArtistName(playlistSong.Artists.FirstOrDefault()?.Name ?? "Unknown"),
            Album = "Unknown Album", // Album info not available in playlist songs
            PlaylistName = "Community Playlist",
            Url = $"https://music.youtube.com/watch?v={playlistSong.Id}",
            Duration = playlistSong.Duration,
            ThumbnailUrl = playlistSong.Thumbnails?.FirstOrDefault()?.Url,
            Description = null,
            UploadDate = null,
            ViewCount = null,
            LikeCount = null,
            Source = "YouTube Music"
        };
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName, CancellationToken cancellationToken = default)
    {
        var retryCount = _settings.RetryCount;
        var attempt = 0;
        
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < retryCount && !cancellationToken.IsCancellationRequested)
            {
                attempt++;
                Logger.Warn(ex, $"Attempt {attempt} failed for {operationName}, retrying... ({retryCount - attempt} attempts remaining)");
                
                // Exponential backoff: wait 1s, 2s, 4s, etc.
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}