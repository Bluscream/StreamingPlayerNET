using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;
using System.Diagnostics;
using Humanizer;

namespace StreamingPlayerNET.Source.YoutubeDL.Services;

public class YouTubeMetadataService : IMetadataService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly YoutubeClient _youtubeClient;
    private readonly YoutubeDLSharp.YoutubeDL _youtubeDL;
    private bool _isInitialized = false;
    
    public YouTubeMetadataService()
    {
        _youtubeClient = new YoutubeClient();
        _youtubeDL = new YoutubeDLSharp.YoutubeDL();
        Logger.Info("YouTube Metadata Service initialized");
    }
    
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized) return Task.CompletedTask;
        
        try
        {
            Logger.Info("Initializing YouTube Metadata Service...");
            
            cancellationToken.ThrowIfCancellationRequested();
            
            // Check if yt-dlp exists in the working directory
            var ytDlpPath = Path.Combine(AppContext.BaseDirectory, "yt-dlp.exe");
            if (!File.Exists(ytDlpPath))
            {
                Logger.Info("yt-dlp.exe not found. YouTube features will be limited.");
                // Don't try to download automatically - let user download manually
                // This prevents hanging during startup
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            
            _isInitialized = true;
            Logger.Info("YouTube Metadata Service initialization completed");
            return Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {
            Logger.Warn("YouTube Metadata Service initialization was cancelled");
            throw;
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 2)
        {
            var errorMessage = "yt-dlp.exe not found during initialization. Full error details: " + ex;
            Logger.Error(errorMessage);
            // Don't throw - just log the error and continue
            _isInitialized = true; // Mark as initialized anyway to prevent retry loops
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize YouTube Metadata Service");
            // Don't throw - just log the error and continue
            _isInitialized = true; // Mark as initialized anyway to prevent retry loops
            return Task.CompletedTask;
        }
    }
    
    public async Task<Song> GetSongMetadataAsync(string songId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Info($"Getting metadata for song ID: {songId}");
        
        try
        {
            var video = await _youtubeClient.Videos.GetAsync(songId, cancellationToken);
            
            var song = new Song
            {
                Id = video.Id.Value,
                Title = video.Title,
                Artist = video.Author.ChannelTitle,
                ChannelTitle = video.Author.ChannelTitle,
                Duration = video.Duration,
                ThumbnailUrl = video.Thumbnails.FirstOrDefault()?.Url,
                Description = video.Description,
                UploadDate = null, // video.UploadDate?.DateTime not available
                ViewCount = null, // video.ViewCount not available
                LikeCount = null, // video.LikeCount not available
                Source = "YouTubeDL"
            };
            
            stopwatch.Stop();
            Logger.Info($"Retrieved metadata for song: {song.Title} by {song.Artist} in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            return song;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, $"Failed to get metadata for song ID: {songId} after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            throw;
        }
    }
    
    public async Task<List<AudioStreamInfo>> GetAudioStreamsAsync(string songId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Info($"Getting audio streams for song ID: {songId}");
        
        var streams = new List<AudioStreamInfo>();
        
        try
        {
            // Try YoutubeDLSharp first for more detailed format information
            if (_isInitialized)
            {
                var url = $"https://youtube.com/watch?v={songId}";
                var result = await _youtubeDL.RunVideoDataFetch(url);
                
                if (result.Success && result.Data?.Formats?.Any() == true)
                {
                    Logger.Debug($"YoutubeDLSharp found {result.Data.Formats.Count()} formats");
                    
                    foreach (var format in result.Data.Formats)
                    {
                        if (!string.IsNullOrEmpty(format.AudioCodec) && 
                            format.AudioCodec != "none" && 
                            !string.IsNullOrEmpty(format.Url))
                        {
                            var stream = new AudioStreamInfo
                            {
                                Url = format.Url,
                                FormatId = format.FormatId ?? "",
                                AudioCodec = format.AudioCodec,
                                VideoCodec = format.VideoCodec,
                                Extension = format.Extension ?? "",
                                AudioBitrate = format.AudioBitrate.HasValue ? (long)format.AudioBitrate.Value : null,
                                Duration = result.Data.Duration.HasValue ? TimeSpan.FromSeconds(result.Data.Duration.Value) : null
                            };
                            
                            streams.Add(stream);
                        }
                    }
                }
            }
            
            // Fallback to YoutubeExplode if no streams found
            if (!streams.Any())
            {
                Logger.Debug("Falling back to YoutubeExplode for stream extraction");
                var manifest = await _youtubeClient.Videos.Streams.GetManifestAsync(songId, cancellationToken);
                var audioStreams = manifest.GetAudioOnlyStreams();
                
                foreach (var stream in audioStreams)
                {
                    var streamInfo = new AudioStreamInfo
                    {
                        Url = stream.Url,
                        FormatId = "unknown", // stream.Itag not available
                        AudioCodec = stream.AudioCodec,
                        Extension = stream.Container.ToString().ToLower(),
                        AudioBitrate = (long)(stream.Bitrate.BitsPerSecond / 1000.0), // Convert to kbps
                        FileSize = stream.Size.Bytes,
                        Container = stream.Container.ToString(),
                        Duration = null // stream.Duration not available
                    };
                    
                    streams.Add(streamInfo);
                }
            }
            
            stopwatch.Stop();
            Logger.Info($"Found {streams.Count} audio streams for song ID: {songId} in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            return streams;
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 2)
        {
            stopwatch.Stop();
            Logger.Error(ex, $"yt-dlp.exe not found. Full error details: {ex}");
            var errorMessage = "yt-dlp.exe missing. Please ensure yt-dlp.exe is available in the application directory.";
            Logger.Error(errorMessage);
            throw new InvalidOperationException(errorMessage, ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, $"Failed to get audio streams for song ID: {songId} after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            throw;
        }
    }
    
    public async Task<AudioStreamInfo> GetBestAudioStreamAsync(string songId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Info($"Getting best audio stream for song ID: {songId}");
        
        var streams = await GetAudioStreamsAsync(songId, cancellationToken);
        
        if (!streams.Any())
        {
            stopwatch.Stop();
            Logger.Error($"No audio streams found for song ID: {songId} after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            throw new InvalidOperationException($"No audio streams found for song ID: {songId}");
        }
        
        // Prefer audio-only streams with highest bitrate
        var bestStream = streams
            .Where(s => s.IsAudioOnly)
            .OrderByDescending(s => s.AudioBitrate ?? 0)
            .FirstOrDefault();
        
        // Fallback to mixed streams if no audio-only found
        if (bestStream == null)
        {
            bestStream = streams
                .OrderByDescending(s => s.AudioBitrate ?? 0)
                .First();
        }
        
        stopwatch.Stop();
        Logger.Info($"Selected best audio stream: {bestStream} in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
        return bestStream;
    }
}