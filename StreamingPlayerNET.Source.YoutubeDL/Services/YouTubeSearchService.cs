using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;

namespace StreamingPlayerNET.Source.YoutubeDL.Services;

public class YouTubeSearchService : ISearchService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly YoutubeClient _youtubeClient;
    private readonly Configuration _config;
    
    public YouTubeSearchService(Configuration config)
    {
        _config = config;
        _youtubeClient = new YoutubeClient();
        Logger.Info("YouTube Search Service initialized");
    }
    
    public async Task<List<Song>> SearchAsync(string query, int maxResults = 0, CancellationToken cancellationToken = default)
    {
        if (maxResults <= 0)
        {
            maxResults = _config.MaxSearchResults;
        }
        
        Logger.Info($"Starting YouTube search for: '{query}' (max results: {maxResults})");
        
        var songs = new List<Song>();
        var enhancedQuery = query + " music"; // Add "music" to get better results
        
        try
        {
            var searchResults = _youtubeClient.Search.GetVideosAsync(enhancedQuery);
            int count = 0;
            
            await foreach (var video in searchResults.WithCancellation(cancellationToken))
            {
                if (count >= maxResults) break;
                
                var song = new Song
                {
                    Id = video.Id.Value,
                    Title = video.Title,
                    Artist = video.Author.ChannelTitle,
                    ChannelTitle = video.Author.ChannelTitle,
                    Url = $"https://www.youtube.com/watch?v={video.Id.Value}",
                    Duration = video.Duration,
                    ThumbnailUrl = video.Thumbnails.FirstOrDefault()?.Url,
                    UploadDate = null, // video.UploadDate?.DateTime not available
                    ViewCount = null, // video.ViewCount not available
                    LikeCount = null // video.LikeCount not available
                };
                
                songs.Add(song);
                count++;
                
                if (count % 10 == 0)
                {
                    Logger.Debug($"Search progress: {count} results found");
                }
            }
            
            Logger.Info($"YouTube search completed: Found {songs.Count} songs for query '{query}'");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"YouTube search failed for query: '{query}'");
            throw;
        }
        
        return songs;
    }
    
    public async Task<List<Song>> SearchByArtistAsync(string artist, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Starting YouTube artist search for: '{artist}'");
        return await SearchAsync($"artist:{artist}", maxResults, cancellationToken);
    }
    
    public async Task<List<Song>> SearchByPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Starting YouTube playlist search for playlist ID: {playlistId}");
        
        var songs = new List<Song>();
        
        try
        {
            var playlist = await _youtubeClient.Playlists.GetAsync(playlistId, cancellationToken);
            var videos = _youtubeClient.Playlists.GetVideosAsync(playlistId);
            
            await foreach (var video in videos.WithCancellation(cancellationToken))
            {
                var song = new Song
                {
                    Id = video.Id.Value,
                    Title = video.Title,
                    Artist = video.Author.ChannelTitle,
                    ChannelTitle = video.Author.ChannelTitle,
                    Url = $"https://www.youtube.com/watch?v={video.Id.Value}",
                    Duration = video.Duration,
                    ThumbnailUrl = video.Thumbnails.FirstOrDefault()?.Url,
                    UploadDate = null, // video.UploadDate?.DateTime not available
                    ViewCount = null, // video.ViewCount not available
                    LikeCount = null // video.LikeCount not available
                };
                
                songs.Add(song);
            }
            
            Logger.Info($"YouTube playlist search completed: Found {songs.Count} songs in playlist '{playlist.Title}'");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"YouTube playlist search failed for playlist ID: {playlistId}");
            throw;
        }
        
        return songs;
    }
    
    public async Task<List<Playlist>> SearchPlaylistsAsync(string query, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Starting YouTube playlist search for: '{query}'");
        
        var playlists = new List<Playlist>();
        
        try
        {
            var searchResults = _youtubeClient.Search.GetPlaylistsAsync(query);
            int count = 0;
            
            await foreach (var playlist in searchResults.WithCancellation(cancellationToken))
            {
                if (count >= maxResults) break;
                
                var playlistModel = new Playlist
                {
                    Id = playlist.Id.Value,
                    Name = playlist.Title,
                    Description = null, // playlist.Description not available
                    ThumbnailUrl = playlist.Thumbnails.FirstOrDefault()?.Url,
                    Author = playlist.Author?.ChannelTitle ?? "Unknown",
                    SongCount = 0, // playlist.SongCount not available
                    CreatedDate = null, // playlist.CreatedDate?.DateTime not available
                    IsPublic = true
                };
                
                playlists.Add(playlistModel);
                count++;
            }
            
            Logger.Info($"YouTube playlist search completed: Found {playlists.Count} playlists for query '{query}'");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"YouTube playlist search failed for query: '{query}'");
            throw;
        }
        
        return playlists;
    }
}