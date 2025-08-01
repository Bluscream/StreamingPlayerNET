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
    
    public async Task<string> DownloadAudioAsync(AudioStreamInfo streamInfo, string? songTitle = null, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Downloading audio for stream: {streamInfo.Url}");
        
        // Try all enabled sources since we don't have a song ID to determine the source
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                Logger.Debug($"Trying to download from {provider.Name}");
                
                // Subscribe to progress events from the provider's download service
                provider.DownloadService.DownloadProgressChanged += OnDownloadProgressChanged;
                
                try
                {
                    var filePath = await provider.DownloadService.DownloadAudioAsync(streamInfo, songTitle, cancellationToken);
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
        Logger.Info($"Getting audio stream for: {streamInfo.Url}");
        
        // Try all enabled sources since we don't have a song ID to determine the source
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                Logger.Debug($"Trying to get audio stream from {provider.Name}");
                var stream = await provider.DownloadService.GetAudioStreamAsync(streamInfo, cancellationToken);
                Logger.Info($"Successfully got audio stream from {provider.Name}");
                return stream;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Failed to get audio stream from {provider.Name}");
                continue;
            }
        }
        
        throw new InvalidOperationException($"Failed to get audio stream from any source provider");
    }
    
    public async Task<long> GetContentLengthAsync(string url, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting content length for: {url}");
        
        // Try all enabled sources
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                Logger.Debug($"Trying to get content length from {provider.Name}");
                var length = await provider.DownloadService.GetContentLengthAsync(url, cancellationToken);
                Logger.Info($"Successfully got content length from {provider.Name}: {length}");
                return length;
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
        Logger.Debug($"Checking direct streaming support for: {streamInfo.Url}");
        
        // Check if any enabled source supports direct streaming
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                var supports = provider.DownloadService.SupportsDirectStreaming(streamInfo);
                if (supports)
                {
                    Logger.Debug($"{provider.Name} supports direct streaming");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Failed to check direct streaming support from {provider.Name}");
                continue;
            }
        }
        
        Logger.Debug("No source providers support direct streaming");
        return false;
    }
    

    
    private ISourceProvider? DetermineSourceProvider(string songId)
    {
        // This is a simple heuristic - in a real implementation, you might have more sophisticated logic
        // or store source information with the song ID
        
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        // YouTube IDs are typically 11 characters and contain alphanumeric characters, hyphens, and underscores
        if (songId.Length == 11 && songId.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
        {
            var youtubeProvider = enabledProviders.FirstOrDefault(p => p.Name.Contains("YouTube", StringComparison.OrdinalIgnoreCase));
            if (youtubeProvider != null)
            {
                return youtubeProvider;
            }
        }
        
        // Spotify IDs are typically 22 characters and contain alphanumeric characters
        if (songId.Length == 22 && songId.All(char.IsLetterOrDigit))
        {
            var spotifyProvider = enabledProviders.FirstOrDefault(p => p.Name.Contains("Spotify", StringComparison.OrdinalIgnoreCase));
            if (spotifyProvider != null)
            {
                return spotifyProvider;
            }
        }
        
        // If we can't determine the source, return null to try all sources
        return null;
    }
}