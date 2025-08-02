using System.Diagnostics;
using NLog;
using Humanizer;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Services;
using StreamingPlayerNET.Source.Base.Interfaces;
using StreamingPlayerNET.Source.YoutubeDL;
using StreamingPlayerNET.Source.Spotify;
using StreamingPlayerNET.Source.YouTubeMusic;
using static StreamingPlayerNET.Common.Models.PlaybackState;

namespace StreamingPlayerNET.UI;

public partial class MainForm : Form
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private MusicPlayerService? _musicPlayerService;
    private ISearchService? _searchService;
    private IMetadataService? _metadataService;
    private IDownloadService? _downloadService;
    private IPlaybackService? _playbackService;
    private ConfigurationService? _configService;

    private ToastNotificationService? _toastNotificationService;
    private WindowsMediaService? _windowsMediaService;
    private GlobalHotkeys? _globalHotkeys;
    private LogService? _logService;
    
    private List<Song> _searchResults = new();
    private List<Playlist> _playlists = new();
    private Queue _queue = new();
    private List<DownloadInfo> _downloads = new();
    private System.Windows.Forms.Timer? _progressTimer;
    private System.Windows.Forms.Timer? _downloadsUpdateTimer;
    
    // Data binding properties for the different views
    // Note: Using manual ListView population instead of data binding for better control
    
    public MainForm(LogService? logService = null)
    {
        Logger.Info("=== Simple Music Player .NET Starting ===");
        Logger.Debug("Initializing MainForm components");
        
        // Use the provided LogService or create a new one if none provided
        _logService = logService ?? new LogService();
        
        InitializeComponent();
        Logger.Debug("Form components initialized successfully");
        
        // Set the form icon
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("StreamingPlayerNET.logo.ico");
            if (stream != null)
            {
                Icon = new Icon(stream);
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Failed to load application icon");
        }
        
        // Apply configuration to UI
        ApplyConfiguration();
        
        // Setup UI event handlers
        SetupEventHandlers();
        
        // Setup data binding
        SetupDataBinding();
        
        // Setup progress timer
        SetupProgressTimer();
        
        // Load playlists will be called after services are initialized
        
        Logger.Info("MainForm initialization completed successfully");
        
        // Initialize services asynchronously after form is shown
        this.Load += async (s, e) => await InitializeServicesAsync();
    }

    protected override void WndProc(ref Message m)
    {
        // Let the Windows Media Service handle media commands
        if (_windowsMediaService?.ProcessMediaCommand(m) == true)
        {
            return; // Message was handled
        }

        base.WndProc(ref m);
    }


}