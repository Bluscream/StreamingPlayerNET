using System.ComponentModel;
using System.Text.Json.Serialization;

namespace StreamingPlayerNET.Common.Models;

public class Configuration
{
    [Category("Download")]
    [DisplayName("Large File Threshold (MB)")]
    [Description("Files larger than this size will be downloaded in chunks")]
    public int LargeFileThresholdMB { get; set; } = 10;

    [Category("Download")]
    [DisplayName("Partial Download Size (MB)")]
    [Description("Size of each download chunk for large files")]
    public int PartialDownloadSizeMB { get; set; } = 10;

    [Category("Download")]
    [DisplayName("Download Timeout (seconds)")]
    [Description("Timeout for download operations")]
    public int DownloadTimeoutSeconds { get; set; } = 30;

    [Category("Download")]
    [DisplayName("Max Concurrent Downloads")]
    [Description("Maximum number of concurrent download operations")]
    public int MaxConcurrentDownloads { get; set; } = 3;

    [Category("Audio")]
    [DisplayName("Default Volume")]
    [Description("Default volume level (0-100)")]
    public int DefaultVolume { get; set; } = 50;

    [Category("Audio")]
    [DisplayName("Audio Buffer Size (KB)")]
    [Description("Size of audio buffer for playback")]
    public int AudioBufferSizeKB { get; set; } = 1024;

    [Category("Audio")]
    [DisplayName("Preferred Audio Quality")]
    [Description("Preferred audio quality for playback")]
    public AudioQuality PreferredAudioQuality { get; set; } = AudioQuality.High;

    [Category("Playback")]
    [DisplayName("Default Repeat Mode")]
    [Description("Default repeat mode for playback")]
    public RepeatMode DefaultRepeatMode { get; set; } = RepeatMode.None;

    [Category("Playback")]
    [DisplayName("Default Shuffle State")]
    [Description("Default shuffle state for playback")]
    public bool DefaultShuffleEnabled { get; set; } = false;

    [Category("Search")]
    [DisplayName("Max Search Results")]
    [Description("Maximum number of search results to return")]
    public int MaxSearchResults { get; set; } = 50;

    [Category("Search")]
    [DisplayName("Search Timeout (seconds)")]
    [Description("Timeout for search operations")]
    public int SearchTimeoutSeconds { get; set; } = 15;

    [Category("UI")]
    [DisplayName("Window Width")]
    [Description("Default window width")]
    public int WindowWidth { get; set; } = 1000;

    [Category("UI")]
    [DisplayName("Window Height")]
    [Description("Default window height")]
    public int WindowHeight { get; set; } = 592;

    [Category("UI")]
    [DisplayName("Splitter Distance")]
    [Description("Default splitter distance for playlist panel")]
    public int SplitterDistance { get; set; } = 250;

    [Category("UI")]
    [DisplayName("Show Playlists Panel")]
    [Description("Whether to show the playlists panel by default")]
    public bool ShowPlaylistsPanel { get; set; } = true;

    [Category("UI")]
    [DisplayName("Show Search Panel")]
    [Description("Whether to show the search panel by default")]
    public bool ShowSearchPanel { get; set; } = true;

    [Category("UI")]
    [DisplayName("Dark Mode")]
    [Description("Enable dark mode theme for the application")]
    public bool DarkMode { get; set; } = false;

    [Category("UI")]
    [DisplayName("Status String Format")]
    [Description("Format string for playing status. Supports: {song}, {artist}, {album}, {playlist}, {song_elapsed}, {song_total}, {playlist_index}, {playlist_total}, {status}")]
    public string StatusStringFormat { get; set; } = "{song} by {artist}";

    [Category("UI")]
    [DisplayName("Show Remaining Time")]
    [Description("Whether to show remaining time (true) or total time (false) in the time display")]
    public bool ShowRemainingTime { get; set; } = true;

    [Category("Hotkeys")]
    [DisplayName("Play/Pause Hotkey")]
    [Description("Keyboard shortcut for play/pause functionality")]
    public KeyBind PlayPauseHotkey { get; set; } = new KeyBind("Space");

    [Category("Hotkeys")]
    [DisplayName("Stop Hotkey")]
    [Description("Keyboard shortcut for stop functionality")]
    public KeyBind StopHotkey { get; set; } = new KeyBind("Escape");

    [Category("Hotkeys")]
    [DisplayName("Next Track Hotkey")]
    [Description("Keyboard shortcut for next track")]
    public KeyBind NextTrackHotkey { get; set; } = new KeyBind("Ctrl+Right");

    [Category("Hotkeys")]
    [DisplayName("Previous Track Hotkey")]
    [Description("Keyboard shortcut for previous track")]
    public KeyBind PreviousTrackHotkey { get; set; } = new KeyBind("Ctrl+Left");

    [Category("Hotkeys")]
    [DisplayName("Repeat Mode Hotkey")]
    [Description("Keyboard shortcut for toggling repeat mode")]
    public KeyBind RepeatModeHotkey { get; set; } = new KeyBind("Ctrl+R");

    [Category("Hotkeys")]
    [DisplayName("Shuffle Hotkey")]
    [Description("Keyboard shortcut for toggling shuffle")]
    public KeyBind ShuffleHotkey { get; set; } = new KeyBind("Ctrl+S");

    [Category("Hotkeys")]
    [DisplayName("Volume Up Hotkey")]
    [Description("Keyboard shortcut for volume up")]
    public KeyBind VolumeUpHotkey { get; set; } = new KeyBind("Ctrl+Up");

    [Category("Hotkeys")]
    [DisplayName("Volume Down Hotkey")]
    [Description("Keyboard shortcut for volume down")]
    public KeyBind VolumeDownHotkey { get; set; } = new KeyBind("Ctrl+Down");

    [Category("Hotkeys")]
    [DisplayName("Toggle Playlists Panel Hotkey")]
    [Description("Keyboard shortcut for toggling playlists panel visibility")]
    public KeyBind TogglePlaylistsHotkey { get; set; } = new KeyBind("Ctrl+P");

    [Category("Hotkeys")]
    [DisplayName("Toggle Search Panel Hotkey")]
    [Description("Keyboard shortcut for toggling search panel visibility")]
    public KeyBind ToggleSearchHotkey { get; set; } = new KeyBind("Ctrl+S");

    [Category("Hotkeys")]
    [DisplayName("Settings Hotkey")]
    [Description("Keyboard shortcut for opening settings")]
    public KeyBind SettingsHotkey { get; set; } = new KeyBind("Ctrl+,");

    [Category("Hotkeys")]
    [DisplayName("Help Hotkey")]
    [Description("Keyboard shortcut for help functionality")]
    public KeyBind HelpHotkey { get; set; } = new KeyBind("F1");

    [Category("Hotkeys")]
    [DisplayName("Dark Mode Hotkey")]
    [Description("Keyboard shortcut for toggling dark mode")]
    public KeyBind DarkModeHotkey { get; set; } = new KeyBind("Ctrl+T");

    [Category("Logging")]
    [DisplayName("Log Level")]
    [Description("Minimum log level to record")]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    [Category("Logging")]
    [DisplayName("Max Log Files")]
    [Description("Maximum number of log files to keep")]
    public int MaxLogFiles { get; set; } = 10;

    [Category("Logging")]
    [DisplayName("Log File Size (MB)")]
    [Description("Maximum size of each log file")]
    public int MaxLogFileSizeMB { get; set; } = 10;

    [Category("Advanced")]
    [DisplayName("Enable Debug Mode")]
    [Description("Enable additional debug information")]
    public bool EnableDebugMode { get; set; } = false;

    [Category("Advanced")]
    [DisplayName("Enable Global Hotkeys Fallback")]
    [Description("Enable global hotkeys as fallback for media keys when Windows Media Session is not available")]
    public bool EnableGlobalHotkeysFallback { get; set; } = true;

    [Category("Advanced")]
    [DisplayName("Cache Directory")]
    [Description("Directory for caching downloaded files")]
    public string CacheDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StreamingPlayerNET", "Cache");

    [Category("Advanced")]
    [DisplayName("Temp Directory")]
    [Description("Directory for temporary files")]
    public string TempDirectory { get; set; } = Path.GetTempPath();

    [Category("Advanced")]
    [DisplayName("User Agent")]
    [Description("User agent string for HTTP requests")]
    public string UserAgent { get; set; } = "StreamingPlayerNET/1.0.0";

    [Category("Queue")]
    [DisplayName("Enable Queue Caching")]
    [Description("Whether to save and restore the queue between app restarts")]
    public bool EnableQueueCaching { get; set; } = true;

    [Category("Queue")]
    [DisplayName("Max Cached Queue Size")]
    [Description("Maximum number of songs to cache in the queue")]
    public int MaxCachedQueueSize { get; set; } = 100;

    [JsonIgnore]
    public long LargeFileThresholdBytes => LargeFileThresholdMB * 1024L * 1024L;

    [JsonIgnore]
    public long PartialDownloadSizeBytes => PartialDownloadSizeMB * 1024L * 1024L;

    [JsonIgnore]
    public int AudioBufferSizeBytes => AudioBufferSizeKB * 1024;

    [JsonIgnore]
    public int MaxLogFileSizeBytes => MaxLogFileSizeMB * 1024 * 1024;
}

public enum AudioQuality
{
    [Description("Low (128 kbps)")]
    Low = 128,
    
    [Description("Medium (192 kbps)")]
    Medium = 192,
    
    [Description("High (256 kbps)")]
    High = 256,
    
    [Description("Very High (320 kbps)")]
    VeryHigh = 320
}

public enum LogLevel
{
    [Description("Trace")]
    Trace = 0,
    
    [Description("Debug")]
    Debug = 1,
    
    [Description("Info")]
    Info = 2,
    
    [Description("Warning")]
    Warning = 3,
    
    [Description("Error")]
    Error = 4,
    
    [Description("Fatal")]
    Fatal = 5
}