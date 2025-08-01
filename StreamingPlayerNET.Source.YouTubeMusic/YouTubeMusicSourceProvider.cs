using YouTubeMusicAPI.Client;
using YouTubeSessionGenerator;
using YouTubeSessionGenerator.Js.Environments;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base;
using StreamingPlayerNET.Source.Base.Interfaces;
using StreamingPlayerNET.Source.YouTubeMusic.Services;
using NLog;
using System.Net;

namespace StreamingPlayerNET.Source.YouTubeMusic;

public class YouTubeMusicSourceProvider : BaseSourceProvider
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly Configuration _config;
    private readonly YouTubeMusicSourceSettings _settings;
    private readonly YouTubeMusicSearchService _searchService;
    private readonly YouTubeMusicDownloadService _downloadService;
    private readonly YouTubeMusicMetadataService _metadataService;
    private readonly YouTubeMusicPlaylistService _playlistService;
    private YouTubeMusicClient? _client;
    
    public override string Name => "YouTube Music";
    public override string ShortName => "YTM";
    public override string Description => "YouTube Music streaming source using YouTubeMusicAPI";
    public override string Version => "1.0.0";
    
    public override ISearchService SearchService => _searchService;
    public override IDownloadService DownloadService => _downloadService;
    public override IMetadataService MetadataService => _metadataService;
    public override IPlaylistService PlaylistService => _playlistService;
    public override ISourceSettings? Settings => _settings;
    
    public YouTubeMusicSourceProvider(Configuration config)
    {
        _config = config;
        _settings = new YouTubeMusicSourceSettings();
        
        // Initialize services with temporary client (will be recreated in OnInitializeAsync)
        _client = CreateYouTubeMusicClient();
        _searchService = new YouTubeMusicSearchService(_client, _settings);
        _downloadService = new YouTubeMusicDownloadService(_settings);
        _metadataService = new YouTubeMusicMetadataService(_client, _settings);
        _playlistService = new YouTubeMusicPlaylistService(_client, _settings);
        
        Logger.Info("YouTube Music Source Provider created");
    }
    
    protected override async Task OnInitializeAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Initializing YouTube Music Source Provider...");
        
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _settings.LoadAsync();
            
            // Recreate client with loaded settings
            _client = CreateYouTubeMusicClient();
            
            // Update services with new client
            var searchField = typeof(YouTubeMusicSearchService).GetField("_client", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            searchField?.SetValue(_searchService, _client);
            
            var metadataField = typeof(YouTubeMusicMetadataService).GetField("_client", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            metadataField?.SetValue(_metadataService, _client);
            
            var playlistField = typeof(YouTubeMusicPlaylistService).GetField("_client", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playlistField?.SetValue(_playlistService, _client);
            
            cancellationToken.ThrowIfCancellationRequested();
            await _metadataService.InitializeAsync(cancellationToken);
            await _playlistService.InitializeAsync(cancellationToken);
            
            // Generate session tokens if needed and enabled
            if (_settings.AutoGenerateSession && 
                (string.IsNullOrEmpty(_settings.VisitorData) || string.IsNullOrEmpty(_settings.PoToken)))
            {
                await GenerateSessionTokensAsync(cancellationToken);
            }
            
            Logger.Info("YouTube Music Source Provider initialization completed");
        }
        catch (OperationCanceledException)
        {
            Logger.Warn("YouTube Music Source Provider initialization was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize YouTube Music Source Provider - continuing with defaults");
            // Don't throw - just log the error and continue
        }
    }
    
    protected override Task OnDisposeAsync()
    {
        _downloadService?.Dispose();
        Logger.Info("YouTube Music Source Provider disposed");
        return Task.CompletedTask;
    }
    
    private YouTubeMusicClient CreateYouTubeMusicClient()
    {
        try
        {
            var logger = LogManager.GetLogger("YouTubeMusicAPI");
            var geographicalLocation = !string.IsNullOrEmpty(_settings.GeographicalLocation) 
                ? _settings.GeographicalLocation 
                : "US";
            
            var visitorData = !string.IsNullOrEmpty(_settings.VisitorData) 
                ? _settings.VisitorData 
                : null;
            
            var poToken = !string.IsNullOrEmpty(_settings.PoToken) 
                ? _settings.PoToken 
                : null;
            
            // Parse cookies if provided
            IEnumerable<Cookie>? cookies = null;
            if (!string.IsNullOrEmpty(_settings.Cookies))
            {
                cookies = ParseCookies(_settings.Cookies);
            }
            
            return new YouTubeMusicClient(geographicalLocation, visitorData, poToken, cookies);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to create YouTube Music client, using default configuration");
            var defaultLogger = LogManager.GetLogger("YouTubeMusicAPI");
            return new YouTubeMusicClient("US", null, null, null);
        }
    }
    
    private async Task GenerateSessionTokensAsync(CancellationToken cancellationToken)
    {
        try
        {
            Logger.Info("Generating session tokens...");
            
            // Create JavaScript environment configuration for PoToken generation
            var nodeEnvironment = !string.IsNullOrEmpty(_settings.NodeJsPath) 
                ? new YouTubeSessionGenerator.Js.Environments.NodeEnvironment(_settings.NodeJsPath)
                : new YouTubeSessionGenerator.Js.Environments.NodeEnvironment();
            
            var config = new YouTubeSessionConfig
            {
                JsEnvironment = nodeEnvironment
                // Logger = LogManager.GetLogger("YouTubeSessionGenerator") // ILogger not compatible with NLog.Logger
            };
            
            var generator = new YouTubeSessionCreator(config);
            
            // Generate visitor data first
            var visitorData = await generator.VisitorDataAsync(cancellationToken);
            _settings.VisitorData = visitorData;
            
            // Generate PoToken using the visitor data
            var poToken = await generator.ProofOfOriginTokenAsync(visitorData, null, cancellationToken);
            _settings.PoToken = poToken;
            
            await _settings.SaveAsync();
            
            Logger.Info("Session tokens generated and saved successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to generate session tokens - continuing without them");
        }
    }
    
    private static IEnumerable<Cookie> ParseCookies(string cookieString)
    {
        var cookies = new List<Cookie>();
        
        try
        {
            var cookiePairs = cookieString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var pair in cookiePairs)
            {
                var trimmedPair = pair.Trim();
                var equalIndex = trimmedPair.IndexOf('=');
                
                if (equalIndex > 0)
                {
                    var name = trimmedPair.Substring(0, equalIndex).Trim();
                    var value = trimmedPair.Substring(equalIndex + 1).Trim();
                    
                    cookies.Add(new Cookie
                    {
                        Name = name,
                        Value = value,
                        Domain = ".youtube.com"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.GetCurrentClassLogger().Error(ex, "Failed to parse cookies");
        }
        
        return cookies;
    }
}