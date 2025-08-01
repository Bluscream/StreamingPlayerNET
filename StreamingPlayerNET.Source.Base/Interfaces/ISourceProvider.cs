namespace StreamingPlayerNET.Source.Base.Interfaces;

/// <summary>
/// Interface that defines what a source provider must implement to be loaded as a plugin
/// </summary>
public interface ISourceProvider
{
    /// <summary>
    /// The name of this source provider (e.g., "YouTube Music", "Spotify", etc.)
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// A short name/identifier for this source provider (e.g., "YT Music", "Spotify", "YouTube")
    /// </summary>
    string ShortName { get; }
    
    /// <summary>
    /// A brief description of this source provider
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// The version of this source provider
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Whether this source provider is currently available/enabled
    /// </summary>
    bool IsAvailable { get; }
    
    /// <summary>
    /// The search service for this source
    /// </summary>
    ISearchService SearchService { get; }
    
    /// <summary>
    /// The download service for this source
    /// </summary>
    IDownloadService DownloadService { get; }
    
    /// <summary>
    /// The metadata service for this source
    /// </summary>
    IMetadataService MetadataService { get; }
    
    /// <summary>
    /// The playlist service for this source
    /// </summary>
    IPlaylistService PlaylistService { get; }
    
    /// <summary>
    /// The settings for this source provider (can be null if no settings are needed)
    /// </summary>
    ISourceSettings? Settings { get; }
    
    /// <summary>
    /// Initialize the source provider
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cleanup resources when the source provider is no longer needed
    /// </summary>
    Task DisposeAsync();
}