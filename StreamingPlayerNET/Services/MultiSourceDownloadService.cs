using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;

namespace StreamingPlayerNET.Services;

public class MultiSourceDownloadService : IDownloadService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SourceManager _sourceManager;
    
    public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;
    
    public MultiSourceDownloadService(SourceManager sourceManager)
    {
        _sourceManager = sourceManager ?? throw new ArgumentNullException(nameof(sourceManager));
    }
    
    public async Task<string> DownloadAudioAsync(Song song, AudioStreamInfo streamInfo, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Downloading audio for song: {song.Title}");
        
        var enabledProviders = _sourceManager.GetEnabledProviders();
        if (!enabledProviders.Any())
        {
            throw new InvalidOperationException("No source providers are enabled");
        }
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                Logger.Debug($"Trying to download from {provider.Name}");
                
                // Subscribe to progress events from the provider's download service
                provider.DownloadService.DownloadProgressChanged += OnDownloadProgressChanged;
                
                try
                {
                    var filePath = await provider.DownloadService.DownloadAudioAsync(song, streamInfo, cancellationToken);
                    Logger.Info($"Successfully downloaded from {provider.Name}");
                    return filePath;
                }
                finally
                {
                    // Unsubscribe from progress events
                    provider.DownloadService.DownloadProgressChanged -= OnDownloadProgressChanged;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Failed to download from {provider.Name}");
                continue;
            }
        }
        
        throw new InvalidOperationException($"Failed to download audio from any source provider");
    }
    
    private void OnDownloadProgressChanged(object? sender, DownloadProgressEventArgs e)
    {
        // Forward the progress event to our subscribers
        DownloadProgressChanged?.Invoke(this, e);
    }
    

    
    public async Task<Stream> GetAudioStreamAsync(AudioStreamInfo streamInfo, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting audio stream");
        
        var enabledProviders = _sourceManager.GetEnabledProviders();
        if (!enabledProviders.Any())
        {
            throw new InvalidOperationException("No source providers are enabled");
        }
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                Logger.Debug($"Trying to get stream from {provider.Name}");
                var stream = await provider.DownloadService.GetAudioStreamAsync(streamInfo, cancellationToken);
                Logger.Info($"Successfully got stream from {provider.Name}");
                return stream;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Failed to get stream from {provider.Name}");
                continue;
            }
        }
        
        throw new InvalidOperationException($"Failed to get audio stream from any source provider");
    }
    
    public async Task<long> GetContentLengthAsync(string url, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting content length for URL");
        
        var enabledProviders = _sourceManager.GetEnabledProviders();
        if (!enabledProviders.Any())
        {
            throw new InvalidOperationException("No source providers are enabled");
        }
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                Logger.Debug($"Trying to get content length from {provider.Name}");
                var contentLength = await provider.DownloadService.GetContentLengthAsync(url, cancellationToken);
                Logger.Info($"Successfully got content length from {provider.Name}: {contentLength}");
                return contentLength;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Failed to get content length from {provider.Name}");
                continue;
            }
        }
        
        throw new InvalidOperationException($"Failed to get content length from any source provider");
    }
    
    public bool SupportsDirectStreaming(AudioStreamInfo streamInfo)
    {
        var enabledProviders = _sourceManager.GetEnabledProviders();
        return enabledProviders.Any(provider => provider.DownloadService.SupportsDirectStreaming(streamInfo));
    }
}