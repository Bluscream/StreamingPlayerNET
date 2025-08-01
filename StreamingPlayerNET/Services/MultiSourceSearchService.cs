using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;

namespace StreamingPlayerNET.Services;

public class MultiSourceSearchService : ISearchService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SourceManager _sourceManager;
    
    public MultiSourceSearchService(SourceManager sourceManager)
    {
        _sourceManager = sourceManager ?? throw new ArgumentNullException(nameof(sourceManager));
    }
    
    public async Task<List<Song>> SearchAsync(string query, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Multi-source search for: '{query}' (max results per source: {maxResults})");
        
        var allResults = new List<Song>();
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        if (!enabledProviders.Any())
        {
            Logger.Warn("No enabled source providers found for search");
            return allResults;
        }
        
        // Calculate results per source to maintain total limit
        var resultsPerSource = Math.Max(1, maxResults / enabledProviders.Count);
        
        var searchTasks = enabledProviders.Select(async provider =>
        {
            try
            {
                Logger.Debug($"Searching in {provider.Name} for: '{query}'");
                var results = await provider.SearchService.SearchAsync(query, resultsPerSource, cancellationToken);
                
                // Add source information to each song
                foreach (var song in results)
                {
                    song.Source = provider.Name;
                }
                
                Logger.Info($"Found {results.Count} results from {provider.Name}");
                return results;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Search failed in {provider.Name} for query: '{query}'");
                return new List<Song>();
            }
        });
        
        // Wait for all searches to complete
        var searchResults = await Task.WhenAll(searchTasks);
        
        // Combine all results
        foreach (var results in searchResults)
        {
            allResults.AddRange(results);
        }
        
        // Sort results by relevance (you could implement more sophisticated ranking here)
        // For now, just take the first maxResults
        var finalResults = allResults.Take(maxResults).ToList();
        
        Logger.Info($"Multi-source search completed: {finalResults.Count} total results from {enabledProviders.Count} sources");
        return finalResults;
    }
    
    public async Task<List<Playlist>> SearchPlaylistsAsync(string query, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Multi-source playlist search for: '{query}' (max results per source: {maxResults})");
        
        var allResults = new List<Playlist>();
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        if (!enabledProviders.Any())
        {
            Logger.Warn("No enabled source providers found for playlist search");
            return allResults;
        }
        
        // Calculate results per source to maintain total limit
        var resultsPerSource = Math.Max(1, maxResults / enabledProviders.Count);
        
        var searchTasks = enabledProviders.Select(async provider =>
        {
            try
            {
                Logger.Debug($"Searching playlists in {provider.Name} for: '{query}'");
                var results = await provider.SearchService.SearchPlaylistsAsync(query, resultsPerSource, cancellationToken);
                
                // Add source information to each playlist
                foreach (var playlist in results)
                {
                    playlist.Source = provider.Name;
                }
                
                Logger.Info($"Found {results.Count} playlists from {provider.Name}");
                return results;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Playlist search failed in {provider.Name} for query: '{query}'");
                return new List<Playlist>();
            }
        });
        
        // Wait for all searches to complete
        var searchResults = await Task.WhenAll(searchTasks);
        
        // Combine all results
        foreach (var results in searchResults)
        {
            allResults.AddRange(results);
        }
        
        // Sort results by relevance (you could implement more sophisticated ranking here)
        // For now, just take the first maxResults
        var finalResults = allResults.Take(maxResults).ToList();
        
        Logger.Info($"Multi-source playlist search completed: {finalResults.Count} total results from {enabledProviders.Count} sources");
        return finalResults;
    }
    
    public async Task<List<Song>> SearchByArtistAsync(string artist, int maxResults = 50, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Multi-source artist search for: '{artist}' (max results per source: {maxResults})");
        
        var allResults = new List<Song>();
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        if (!enabledProviders.Any())
        {
            Logger.Warn("No enabled source providers found for artist search");
            return allResults;
        }
        
        // Calculate results per source to maintain total limit
        var resultsPerSource = Math.Max(1, maxResults / enabledProviders.Count);
        
        var searchTasks = enabledProviders.Select(async provider =>
        {
            try
            {
                Logger.Debug($"Searching artist in {provider.Name} for: '{artist}'");
                var results = await provider.SearchService.SearchByArtistAsync(artist, resultsPerSource, cancellationToken);
                
                // Add source information to each song
                foreach (var song in results)
                {
                    song.Source = provider.Name;
                }
                
                Logger.Info($"Found {results.Count} results from {provider.Name}");
                return results;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Artist search failed in {provider.Name} for artist: '{artist}'");
                return new List<Song>();
            }
        });
        
        // Wait for all searches to complete
        var searchResults = await Task.WhenAll(searchTasks);
        
        // Combine all results
        foreach (var results in searchResults)
        {
            allResults.AddRange(results);
        }
        
        // Sort results by relevance (you could implement more sophisticated ranking here)
        // For now, just take the first maxResults
        var finalResults = allResults.Take(maxResults).ToList();
        
        Logger.Info($"Multi-source artist search completed: {finalResults.Count} total results from {enabledProviders.Count} sources");
        return finalResults;
    }
    
    public async Task<List<Song>> SearchByPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Multi-source playlist search for playlist ID: '{playlistId}'");
        
        var allResults = new List<Song>();
        var enabledProviders = _sourceManager.GetEnabledSourceProvidersWithSettings();
        
        if (!enabledProviders.Any())
        {
            Logger.Warn("No enabled source providers found for playlist search");
            return allResults;
        }
        
        var searchTasks = enabledProviders.Select(async provider =>
        {
            try
            {
                Logger.Debug($"Searching playlist in {provider.Name} for ID: '{playlistId}'");
                var results = await provider.SearchService.SearchByPlaylistAsync(playlistId, cancellationToken);
                
                // Add source information to each song
                foreach (var song in results)
                {
                    song.Source = provider.Name;
                }
                
                Logger.Info($"Found {results.Count} results from {provider.Name}");
                return results;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Playlist search failed in {provider.Name} for playlist ID: '{playlistId}'");
                return new List<Song>();
            }
        });
        
        // Wait for all searches to complete
        var searchResults = await Task.WhenAll(searchTasks);
        
        // Combine all results
        foreach (var results in searchResults)
        {
            allResults.AddRange(results);
        }
        
        Logger.Info($"Multi-source playlist search completed: {allResults.Count} total results from {enabledProviders.Count} sources");
        return allResults;
    }
} 