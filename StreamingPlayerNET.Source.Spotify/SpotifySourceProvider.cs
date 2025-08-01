using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base;
using StreamingPlayerNET.Source.Base.Interfaces;
using StreamingPlayerNET.Source.Spotify.Services;
using NLog;

namespace StreamingPlayerNET.Source.Spotify;

public class SpotifySourceProvider : BaseSourceProvider
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly Configuration _config;
    private readonly SpotifySourceSettings _settings;
    private readonly SpotifyAuthService _authService;
    
    // Actual Spotify services
    private SpotifySearchService? _searchService;
    private SpotifyDownloadService? _downloadService;
    private SpotifyMetadataService? _metadataService;
    private SpotifyPlaylistService? _playlistService;
    
    public override string Name => "Spotify";
    public override string ShortName => "Spotify";
    public override string Description => "Spotify streaming source using Spotify Web API";
    public override string Version => "1.0.0";
    
    public override ISearchService SearchService => _searchService ?? throw new InvalidOperationException("Spotify service not initialized");
    public override IDownloadService DownloadService => _downloadService ?? throw new InvalidOperationException("Spotify service not initialized");
    public override IMetadataService MetadataService => _metadataService ?? throw new InvalidOperationException("Spotify service not initialized");
    public override IPlaylistService PlaylistService => _playlistService ?? throw new InvalidOperationException("Spotify service not initialized");
    public override ISourceSettings? Settings => _settings;
    
    public SpotifySourceProvider(Configuration config)
    {
        _config = config;
        _settings = new SpotifySourceSettings();
        _authService = new SpotifyAuthService(_settings);
        
        Logger.Info("Spotify Source Provider created");
    }
    
    protected override async Task OnInitializeAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Initializing Spotify Source Provider...");
        
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _settings.LoadAsync();
            
            // Authenticate with Spotify
            var authenticated = await _authService.AuthenticateAsync(cancellationToken);
            if (!authenticated)
            {
                Logger.Warn("Failed to authenticate with Spotify - services will not be available");
                return;
            }
            
            // Initialize services with authenticated client
            var spotifyClient = _authService.Client;
            if (spotifyClient != null)
            {
                _searchService = new SpotifySearchService(spotifyClient, _settings);
                _downloadService = new SpotifyDownloadService(_settings);
                _metadataService = new SpotifyMetadataService(spotifyClient, _settings);
                _playlistService = new SpotifyPlaylistService(spotifyClient, _settings);
                
                // Initialize services
                await _metadataService.InitializeAsync(cancellationToken);
                await _playlistService.InitializeAsync(cancellationToken);
                
                Logger.Info("Spotify Source Provider initialization completed successfully");
            }
            else
            {
                Logger.Error("Spotify client is null after authentication");
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Warn("Spotify Source Provider initialization was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize Spotify Source Provider");
            // Don't throw - just log the error and continue with placeholder services
        }
    }
    
    protected override Task OnDisposeAsync()
    {
        Logger.Info("Spotify Source Provider disposed");
        return Task.CompletedTask;
    }
} 