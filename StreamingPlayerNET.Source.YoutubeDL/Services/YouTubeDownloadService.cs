using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Common.Utils;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;
using Humanizer;
using System.Diagnostics;

namespace StreamingPlayerNET.Source.YoutubeDL.Services;

public class YouTubeDownloadService : IDownloadService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Configuration _config;
    
    public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;
    
    public YouTubeDownloadService(Configuration config)
    {
        _config = config;
    }
    
    public async Task<string> DownloadAudioAsync(Song song, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Info($"Downloading audio for song: {song.Title}");
        
        if (song.SelectedStream == null)
        {
            throw new InvalidOperationException($"No selected stream for song: {song.Title}");
        }
        
        var tempFile = Path.GetTempFileName();
        
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_config.DownloadTimeoutSeconds);
            
            // URL-decode the stream URL to fix "An invalid request URI was provided" error
            var decodedUrl = UrlUtils.DecodeUrlWithLogging(song.SelectedStream.Url, Logger);
            
            // Check if we should use partial download for large files
            var contentLength = await GetContentLengthAsync(decodedUrl, cancellationToken);
            var usePartialDownload = contentLength > _config.LargeFileThresholdBytes;
            
            if (usePartialDownload)
            {
                Logger.Info($"Large file detected ({contentLength.Bytes()}), using partial download");
                httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(0, _config.PartialDownloadSizeBytes);
            }
            
            var response = await httpClient.GetAsync(decodedUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            
            // Report download start
            DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs(song, "Starting download..."));
            
            await using (var fileStream = File.Create(tempFile))
            using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            {
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
                
                await fileStream.FlushAsync(cancellationToken);
            }
            
            var fileSize = new FileInfo(tempFile).Length;
            stopwatch.Stop();
            Logger.Info($"Download completed: {fileSize.Bytes()} saved to {tempFile} in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            
            // Report download completion
            DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs(song, "Download completed"));
            
            return tempFile;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, $"Download failed for song: {song.Title} after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            
            // Clean up temp file on failure
            try { File.Delete(tempFile); } catch { }
            
            throw;
        }
    }
    
    public async Task<Stream> GetAudioStreamAsync(AudioStreamInfo streamInfo, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.Info($"Getting audio stream: {streamInfo}");
        
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_config.DownloadTimeoutSeconds);
            
            // URL-decode the stream URL to fix "An invalid request URI was provided" error
            var decodedUrl = UrlUtils.DecodeUrlWithLogging(streamInfo.Url, Logger);
            
            var response = await httpClient.GetAsync(decodedUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            stopwatch.Stop();
            Logger.Info($"Audio stream retrieved in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            
            return stream;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, $"Failed to get audio stream: {streamInfo} after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            throw;
        }
    }
    
    public async Task<long> GetContentLengthAsync(string url, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_config.DownloadTimeoutSeconds);
            
            // URL-decode the URL to fix "An invalid request URI was provided" error
            var decodedUrl = UrlUtils.DecodeUrlWithLogging(url, Logger);
            
            var request = new HttpRequestMessage(HttpMethod.Head, decodedUrl);
            var response = await httpClient.SendAsync(request, cancellationToken);
            
            var contentLength = response.Content.Headers.ContentLength;
            stopwatch.Stop();
            Logger.Debug($"Content length for {decodedUrl}: {contentLength?.Bytes().ToString() ?? "Unknown"} retrieved in {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            
            return contentLength ?? 0;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Warn(ex, $"Failed to get content length for URL: {url} after {stopwatch.Elapsed.TotalMilliseconds.Milliseconds()}");
            return 0;
        }
    }
    
    public bool SupportsDirectStreaming(AudioStreamInfo streamInfo)
    {
        // Check if the stream format is supported for direct streaming
        var supportedExtensions = new[] { "m4a", "mp3", "aac", "opus", "webm" };
        return supportedExtensions.Contains(streamInfo.Extension.ToLower());
    }
}