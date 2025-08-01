using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base;
using StreamingPlayerNET.Source.Base.Interfaces;
using StreamingPlayerNET.Source.YoutubeDL.Services;
using NLog;

namespace StreamingPlayerNET.Source.YoutubeDL;

public class YouTubeSourceProvider : BaseSourceProvider
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly Configuration _config;
    private readonly YouTubeSearchService _searchService;
    private readonly YouTubeDownloadService _downloadService;
    private readonly YouTubeMetadataService _metadataService;
    private readonly YouTubePlaylistService _playlistService;
    private readonly YouTubeSourceSettings _settings;
    
    public override string Name => "YouTubeDL";
    public override string ShortName => "YTDL";
    public override string Description => "YouTube streaming source using yt-dlp and YoutubeExplode";
    public override string Version => "1.0.0";
    
    public override ISearchService SearchService => _searchService;
    public override IDownloadService DownloadService => _downloadService;
    public override IMetadataService MetadataService => _metadataService;
    public override IPlaylistService PlaylistService => _playlistService;
    public override ISourceSettings? Settings => _settings;
    
    public YouTubeSourceProvider(Configuration config)
    {
        _config = config;
        _settings = new YouTubeSourceSettings();
        _searchService = new YouTubeSearchService(_config);
        _downloadService = new YouTubeDownloadService(_config);
        _metadataService = new YouTubeMetadataService();
        _playlistService = new YouTubePlaylistService();
        
        Logger.Info("YouTube Source Provider created");
    }
    
    protected override async Task OnInitializeAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Initializing YouTube Source Provider...");
        
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _settings.LoadAsync();
            
            cancellationToken.ThrowIfCancellationRequested();
            await _metadataService.InitializeAsync(cancellationToken);
            await _playlistService.InitializeAsync(cancellationToken);
            
            Logger.Info("YouTube Source Provider initialization completed");
        }
        catch (OperationCanceledException)
        {
            Logger.Warn("YouTube Source Provider initialization was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize YouTube Source Provider - continuing with defaults");
            // Don't throw - just log the error and continue
        }
    }
    
    protected override Task OnDisposeAsync()
    {
        Logger.Info("YouTube Source Provider disposed");
        return Task.CompletedTask;
    }
}