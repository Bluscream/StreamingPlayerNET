using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;

namespace StreamingPlayerNET.Services;

public class SourceManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static SourceManager? _instance;
    private readonly List<ISourceProvider> _sourceProviders = new();
    
    public static SourceManager Instance => _instance ??= new SourceManager();
    
    private SourceManager() { }
    
    /// <summary>
    /// Register a source provider
    /// </summary>
    public void RegisterSourceProvider(ISourceProvider sourceProvider)
    {
        if (sourceProvider == null) throw new ArgumentNullException(nameof(sourceProvider));
        
        if (_sourceProviders.Any(sp => sp.Name == sourceProvider.Name))
        {
            Logger.Warn($"Source provider '{sourceProvider.Name}' is already registered");
            return;
        }
        
        _sourceProviders.Add(sourceProvider);
        Logger.Info($"Registered source provider: {sourceProvider.Name}");
    }
    
    /// <summary>
    /// Get all registered source providers
    /// </summary>
    public IReadOnlyList<ISourceProvider> GetSourceProviders()
    {
        return _sourceProviders.AsReadOnly();
    }
    
    /// <summary>
    /// Get all source providers that have settings
    /// </summary>
    public IReadOnlyList<ISourceProvider> GetSourceProvidersWithSettings()
    {
        return _sourceProviders.Where(sp => sp.Settings != null).ToList().AsReadOnly();
    }
    
    /// <summary>
    /// Get all source providers that are enabled and have settings
    /// </summary>
    public IReadOnlyList<ISourceProvider> GetEnabledSourceProvidersWithSettings()
    {
        return _sourceProviders.Where(sp => sp.Settings != null && sp.Settings.IsEnabled).ToList().AsReadOnly();
    }
    
    /// <summary>
    /// Get a specific source provider by name
    /// </summary>
    public ISourceProvider? GetSourceProvider(string name)
    {
        return _sourceProviders.FirstOrDefault(sp => sp.Name == name);
    }
    
    /// <summary>
    /// Initialize all source providers
    /// </summary>
    public async Task InitializeAllAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Initializing all source providers...");
        
        foreach (var provider in _sourceProviders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            try
            {
                await provider.InitializeAsync(cancellationToken);
                Logger.Info($"Initialized source provider: {provider.Name}");
            }
            catch (OperationCanceledException)
            {
                Logger.Warn($"Initialization of {provider.Name} was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to initialize source provider: {provider.Name}");
            }
        }
    }
    
    /// <summary>
    /// Dispose all source providers
    /// </summary>
    public async Task DisposeAllAsync()
    {
        Logger.Info("Disposing all source providers...");
        
        foreach (var provider in _sourceProviders)
        {
            try
            {
                await provider.DisposeAsync();
                Logger.Info($"Disposed source provider: {provider.Name}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to dispose source provider: {provider.Name}");
            }
        }
        
        _sourceProviders.Clear();
    }
} 