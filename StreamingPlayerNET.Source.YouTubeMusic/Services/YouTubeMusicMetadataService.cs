using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Streaming;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using StreamingPlayerNET.Source.YouTubeMusic.Utils;
using NLog;

namespace StreamingPlayerNET.Source.YouTubeMusic.Services;

public class YouTubeMusicMetadataService : IMetadataService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly YouTubeMusicClient _client;
    private readonly YouTubeMusicSourceSettings _settings;
    
    public YouTubeMusicMetadataService(YouTubeMusicClient client, YouTubeMusicSourceSettings settings)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Logger.Info("YouTube Music Metadata Service initialized");
    }
    
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("YouTube Music Metadata Service initialization completed");
        return Task.CompletedTask;
    }
    
    public async Task<Song> GetSongMetadataAsync(string songId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting metadata for song ID: {songId}");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var songInfo = await _client.GetSongVideoInfoAsync(songId);
            
            var song = new Song
            {
                Id = songId,
                Title = songInfo.Name,
                Artist = YouTubeMusicUtils.JoinAndCleanArtistNames(songInfo.Artists.Select(a => a.Name)),
                ChannelTitle = YouTubeMusicUtils.CleanArtistName(songInfo.Artists.FirstOrDefault()?.Name ?? "Unknown"),
                Album = "Unknown Album", // Album info not available in song video info
                PlaylistName = "YouTube Music",
                Url = $"https://music.youtube.com/watch?v={songId}",
                Duration = songInfo.Duration,
                ThumbnailUrl = songInfo.Thumbnails?.FirstOrDefault()?.Url,
                Description = songInfo.Description,
                UploadDate = songInfo.UploadedAt,
                ViewCount = songInfo.ViewsCount,
                LikeCount = null, // LikeCount not available in SongVideoInfo
                Source = "YouTube Music"
            };
            
            Logger.Info($"Successfully retrieved metadata for song: {song.Title} - {song.Artist}");
            return song;
        }, "get song metadata", cancellationToken);
    }
    
    public async Task<List<StreamingPlayerNET.Common.Models.AudioStreamInfo>> GetAudioStreamsAsync(string songId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting audio streams for song ID: {songId}");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var streamingData = await _client.GetStreamingDataAsync(songId);
            var audioStreams = new List<StreamingPlayerNET.Common.Models.AudioStreamInfo>();
            
            foreach (var streamInfo in streamingData.StreamInfo)
            {
                if (streamInfo is YouTubeMusicAPI.Models.Streaming.AudioStreamInfo ytmAudioStream)
                {
                    var audioStream = new StreamingPlayerNET.Common.Models.AudioStreamInfo
                    {
                        Url = ytmAudioStream.Url,
                        FormatId = ytmAudioStream.Itag.ToString(),
                        AudioCodec = ytmAudioStream.Container.Codecs ?? "unknown",
                        VideoCodec = null, // Audio only
                        Extension = GetExtensionFromContainer(ytmAudioStream.Container.Format),
                        AudioBitrate = ytmAudioStream.Bitrate,
                        FileSize = ytmAudioStream.ContentLenght,
                        Container = ytmAudioStream.Container.Format ?? "unknown",
                        Duration = ytmAudioStream.Duration
                    };
                    
                    audioStreams.Add(audioStream);
                }
            }
            
            // Sort by bitrate descending to prioritize higher quality
            audioStreams = audioStreams
                .OrderByDescending(s => s.AudioBitrate ?? 0)
                .ToList();
            
            Logger.Info($"Found {audioStreams.Count} audio streams for song ID: {songId}");
            return audioStreams;
        }, "get audio streams", cancellationToken);
    }
    
    public async Task<StreamingPlayerNET.Common.Models.AudioStreamInfo> GetBestAudioStreamAsync(string songId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting best audio stream for song ID: {songId}");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var audioStreams = await GetAudioStreamsAsync(songId, cancellationToken);
            
            if (!audioStreams.Any())
            {
                throw new InvalidOperationException($"No audio streams found for song ID: {songId}");
            }
            
            // Filter based on quality preference
            StreamingPlayerNET.Common.Models.AudioStreamInfo bestStream = _settings.PreferredAudioQuality switch
            {
                AudioQuality.Low => audioStreams.Where(s => s.AudioBitrate <= 128000).OrderByDescending(s => s.AudioBitrate).FirstOrDefault() ?? audioStreams.First(),
                AudioQuality.Medium => audioStreams.Where(s => s.AudioBitrate > 128000 && s.AudioBitrate <= 192000).OrderByDescending(s => s.AudioBitrate).FirstOrDefault() ?? audioStreams.First(),
                AudioQuality.High => audioStreams.Where(s => s.AudioBitrate > 192000 && s.AudioBitrate <= 320000).OrderByDescending(s => s.AudioBitrate).FirstOrDefault() ?? audioStreams.First(),
                AudioQuality.Highest => audioStreams.OrderByDescending(s => s.AudioBitrate).First(),
                _ => audioStreams.OrderByDescending(s => s.AudioBitrate).First()
            };
            
            Logger.Info($"Selected best audio stream: {bestStream.AudioCodec} @ {bestStream.AudioBitrate} kbps");
            return bestStream;
        }, "get best audio stream", cancellationToken);
    }
    
    private static string GetExtensionFromContainer(string? container)
    {
        return container?.ToLowerInvariant() switch
        {
            "mp4" => "m4a",
            "webm" => "webm",
            "3gpp" => "3gp",
            _ => "m4a" // Default to m4a
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