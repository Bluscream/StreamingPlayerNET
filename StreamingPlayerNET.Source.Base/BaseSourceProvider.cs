using StreamingPlayerNET.Source.Base.Interfaces;

namespace StreamingPlayerNET.Source.Base;

/// <summary>
/// Base class for source providers that provides common functionality
/// </summary>
public abstract class BaseSourceProvider : ISourceProvider
{
    protected bool _disposed = false;
    protected bool _initialized = false;
    
    public abstract string Name { get; }
    public abstract string ShortName { get; }
    public abstract string Description { get; }
    public abstract string Version { get; }
    public virtual bool IsAvailable => _initialized && !_disposed;
    
    public abstract ISearchService SearchService { get; }
    public abstract IDownloadService DownloadService { get; }
    public abstract IMetadataService MetadataService { get; }
    public abstract IPlaylistService PlaylistService { get; }
    public virtual ISourceSettings? Settings => null;
    
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) return;
        
        await OnInitializeAsync(cancellationToken);
        _initialized = true;
    }
    
    public virtual async Task DisposeAsync()
    {
        if (_disposed) return;
        
        await OnDisposeAsync();
        _disposed = true;
        _initialized = false;
    }
    
    /// <summary>
    /// Override this to provide custom initialization logic
    /// </summary>
    protected virtual Task OnInitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    
    /// <summary>
    /// Override this to provide custom disposal logic
    /// </summary>
    protected virtual Task OnDisposeAsync() => Task.CompletedTask;
}