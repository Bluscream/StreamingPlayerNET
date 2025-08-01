using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;

namespace StreamingPlayerNET.Services;

public class MultiSourceMetadataService : IMetadataService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SourceManager _sourceManager;
    
    public MultiSourceMetadataService(SourceManager sourceManager)
    {
        _sourceManager = sourceManager ?? throw new ArgumentNullException(nameof(sourceManager));
    }
    
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Initializing multi-source metadata service");
        // No initialization needed for the multi-source service itself
    }
    
    public async Task<Song> GetSongMetadataAsync(string songId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting metadata for song ID: {songId}");
        
        // Try to determine the source from the song ID format or try all sources
        var sourceProvider = DetermineSourceProvider(songId);
        
        if (sourceProvider != null)
        {
            try
            {
                Logger.Debug($"Using specific source provider: {sourceProvider.Name}");
                return await sourceProvider.MetadataService.GetSongMetadataAsync(songId, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to get metadata from {sourceProvider.Name}, trying other sources");
            }
        }
        
        // If specific source failed or couldn't be determined, try all enabled sources
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                Logger.Debug($"Trying to get metadata from {provider.Name}");
                var song = await provider.MetadataService.GetSongMetadataAsync(songId, cancellationToken);
                Logger.Info($"Successfully got metadata from {provider.Name}");
                return song;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Failed to get metadata from {provider.Name}");
                continue;
            }
        }
        
        throw new InvalidOperationException($"Failed to get metadata for song ID {songId} from any source provider");
    }
    
    public async Task<AudioStreamInfo> GetBestAudioStreamAsync(string songId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting best audio stream for song ID: {songId}");
        
        // Try to determine the source from the song ID format or try all sources
        var sourceProvider = DetermineSourceProvider(songId);
        
        if (sourceProvider != null)
        {
            try
            {
                Logger.Debug($"Using specific source provider: {sourceProvider.Name}");
                return await sourceProvider.MetadataService.GetBestAudioStreamAsync(songId, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to get audio stream from {sourceProvider.Name}, trying other sources");
            }
        }
        
        // If specific source failed or couldn't be determined, try all enabled sources
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                Logger.Debug($"Trying to get audio stream from {provider.Name}");
                var streamInfo = await provider.MetadataService.GetBestAudioStreamAsync(songId, cancellationToken);
                Logger.Info($"Successfully got audio stream from {provider.Name}");
                return streamInfo;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Failed to get audio stream from {provider.Name}");
                continue;
            }
        }
        
        throw new InvalidOperationException($"Failed to get audio stream for song ID {songId} from any source provider");
    }
    
    public async Task<List<AudioStreamInfo>> GetAudioStreamsAsync(string songId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Getting audio streams for song ID: {songId}");
        
        // Try to determine the source from the song ID format or try all sources
        var sourceProvider = DetermineSourceProvider(songId);
        
        if (sourceProvider != null)
        {
            try
            {
                Logger.Debug($"Using specific source provider: {sourceProvider.Name}");
                return await sourceProvider.MetadataService.GetAudioStreamsAsync(songId, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to get audio streams from {sourceProvider.Name}, trying other sources");
            }
        }
        
        // If specific source failed or couldn't be determined, try all enabled sources
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                Logger.Debug($"Trying to get audio streams from {provider.Name}");
                var streams = await provider.MetadataService.GetAudioStreamsAsync(songId, cancellationToken);
                Logger.Info($"Successfully got audio streams from {provider.Name}");
                return streams;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Failed to get audio streams from {provider.Name}");
                continue;
            }
        }
        
        throw new InvalidOperationException($"Failed to get audio streams for song ID {songId} from any source provider");
    }
    
    public async Task<List<AudioStreamInfo>> GetAvailableAudioStreamsAsync(string songId, CancellationToken cancellationToken = default)
    {
        // This is an alias for GetAudioStreamsAsync for backward compatibility
        return await GetAudioStreamsAsync(songId, cancellationToken);
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