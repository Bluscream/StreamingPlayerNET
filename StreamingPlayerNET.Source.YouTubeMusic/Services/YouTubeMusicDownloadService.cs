using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Common.Utils;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;

namespace StreamingPlayerNET.Source.YouTubeMusic.Services;

public class YouTubeMusicDownloadService : IDownloadService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly YouTubeMusicSourceSettings _settings;
    private readonly HttpClient _httpClient;
    
    public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;
    
    public YouTubeMusicDownloadService(YouTubeMusicSourceSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.RequestTimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        
        Logger.Info("YouTube Music Download Service initialized");
    }
    
    public async Task<string> DownloadAudioAsync(Song song, AudioStreamInfo streamInfo, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Downloading audio for song: {song.Title}");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var tempFileName = Path.GetTempFileName();
            var finalFileName = Path.ChangeExtension(tempFileName, streamInfo.Extension);
            
            // URL-decode the stream URL to fix "An invalid request URI was provided" error
            var decodedUrl = UrlUtils.DecodeUrlWithLogging(streamInfo.Url, Logger);
            
            using var response = await _httpClient.GetAsync(decodedUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            
            // Report download start
            DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs(song, "Starting download..."));
            
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(finalFileName, FileMode.Create, FileAccess.Write);
            
            var buffer = new byte[8192];
            var totalBytesRead = 0L;
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;
                
                // Report progress every 64KB or when complete
                if (totalBytesRead % 65536 == 0 || totalBytesRead == totalBytes)
                {
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs(song, totalBytesRead, totalBytes));
                }
            }
            
            // Delete the original temp file
            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
            
            // Report download completion
            DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs(song, "Download completed"));
            
            Logger.Info($"Successfully downloaded audio to: {finalFileName}");
            return finalFileName;
        }, "download audio", cancellationToken);
    }
    
    public async Task<Stream> GetAudioStreamAsync(AudioStreamInfo streamInfo, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting audio stream from: {streamInfo.Url}");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            // URL-decode the stream URL to fix "An invalid request URI was provided" error
            var decodedUrl = UrlUtils.DecodeUrlWithLogging(streamInfo.Url, Logger);
            
            var response = await _httpClient.GetAsync(decodedUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            
            Logger.Info($"Successfully obtained audio stream from: {decodedUrl}");
            return stream;
        }, "get audio stream", cancellationToken);
    }
    
    public async Task<long> GetContentLengthAsync(string url, CancellationToken cancellationToken = default)
    {
        Logger.Debug($"Getting content length for: {url}");
        
        return await ExecuteWithRetryAsync(async () =>
        {
            // URL-decode the URL to fix "An invalid request URI was provided" error
            var decodedUrl = UrlUtils.DecodeUrlWithLogging(url, Logger);
            
            using var request = new HttpRequestMessage(HttpMethod.Head, decodedUrl);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var contentLength = response.Content.Headers.ContentLength ?? 0;
            
            Logger.Debug($"Content length for {decodedUrl}: {contentLength} bytes");
            return contentLength;
        }, "get content length", cancellationToken);
    }
    
    public bool SupportsDirectStreaming(AudioStreamInfo streamInfo)
    {
        // YouTube Music streams are generally streamable directly
        // Check if the URL is valid and not requiring special handling
        var isSupported = !string.IsNullOrEmpty(streamInfo.Url) && 
                         Uri.IsWellFormedUriString(streamInfo.Url, UriKind.Absolute);
        
        Logger.Debug($"Direct streaming support for {streamInfo.FormatId}: {isSupported}");
        return isSupported;
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
    
    public void Dispose()
    {
        _httpClient?.Dispose();
        Logger.Info("YouTube Music Download Service disposed");
    }
}