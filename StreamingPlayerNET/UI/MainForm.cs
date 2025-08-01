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
    
    private List<Song> _searchResults = new();
    private List<Playlist> _playlists = new();
    private Queue _queue = new();
    private System.Windows.Forms.Timer? _progressTimer;
    
    // Data binding properties for the different views
    // Note: Using manual ListView population instead of data binding for better control
    
    public MainForm()
    {
        Logger.Info("=== Simple Music Player .NET Starting ===");
        Logger.Debug("Initializing MainForm components");
        
        InitializeComponent();
        Logger.Debug("Form components initialized successfully");
        
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