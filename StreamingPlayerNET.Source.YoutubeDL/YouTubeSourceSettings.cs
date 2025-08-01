using System.ComponentModel;
using StreamingPlayerNET.Source.Base;

namespace StreamingPlayerNET.Source.YoutubeDL;

public class YouTubeSourceSettings : BaseSourceSettings
{
    public YouTubeSourceSettings() : base("YouTube")
    {
    }
    
    [Category("API")]
    [DisplayName("YouTube API Key")]
    [Description("YouTube Data API v3 key for accessing playlists and channel data")]
    public string YouTubeApiKey { get; set; } = "";
    
    [Category("API")]
    [DisplayName("Enable YouTube API")]
    [Description("Enable YouTube API integration for playlist fetching")]
    public bool EnableYouTubeApi { get; set; } = true;
    
    [Category("API")]
    [DisplayName("Max API Results")]
    [Description("Maximum number of results to fetch from YouTube API")]
    public int MaxApiResults { get; set; } = 50;
    
    [Category("Download")]
    [DisplayName("Preferred Audio Format")]
    [Description("Preferred audio format for downloads")]
    public AudioFormat PreferredAudioFormat { get; set; } = AudioFormat.Mp3;
    
    [Category("Download")]
    [DisplayName("Extract Audio Only")]
    [Description("Whether to extract audio only (faster, smaller files)")]
    public bool ExtractAudioOnly { get; set; } = true;
    
    [Category("Download")]
    [DisplayName("Use yt-dlp")]
    [Description("Use yt-dlp instead of youtube-dl for better performance")]
    public bool UseYoutubeDLp { get; set; } = true;
    
    [Category("Download")]
    [DisplayName("yt-dlp Path")]
    [Description("Path to yt-dlp executable (leave empty for auto-detection)")]
    public string YoutubeDLpPath { get; set; } = "";
    
    [Category("Search")]
    [DisplayName("Search Type")]
    [Description("Type of search to perform")]
    public SearchType SearchType { get; set; } = SearchType.Video;
    
    [Category("Search")]
    [DisplayName("Include Playlists")]
    [Description("Include playlists in search results")]
    public bool IncludePlaylists { get; set; } = true;
    
    [Category("Search")]
    [DisplayName("Max Playlist Items")]
    [Description("Maximum number of items to fetch from playlists")]
    public int MaxPlaylistItems { get; set; } = 100;
    
    [Category("Advanced")]
    [DisplayName("User Agent")]
    [Description("Custom user agent for requests")]
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
    
    [Category("Advanced")]
    [DisplayName("Request Timeout (seconds)")]
    [Description("Timeout for HTTP requests")]
    public int RequestTimeoutSeconds { get; set; } = 30;
    
    [Category("Advanced")]
    [DisplayName("Retry Count")]
    [Description("Number of retries for failed requests")]
    public int RetryCount { get; set; } = 3;
    
    [Category("Advanced")]
    [DisplayName("Enable Debug Logging")]
    [Description("Enable detailed debug logging for YouTube operations")]
    public bool EnableDebugLogging { get; set; } = false;
}

public enum AudioFormat
{
    [Description("MP3")]
    Mp3,
    
    [Description("AAC")]
    Aac,
    
    [Description("OGG")]
    Ogg,
    
    [Description("WAV")]
    Wav
}

public enum SearchType
{
    [Description("Video")]
    Video,
    
    [Description("Music")]
    Music,
    
    [Description("Both")]
    Both
} 