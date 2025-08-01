using System.ComponentModel;
using StreamingPlayerNET.Source.Base;

namespace StreamingPlayerNET.Source.YouTubeMusic;

public class YouTubeMusicSourceSettings : BaseSourceSettings
{
    public YouTubeMusicSourceSettings() : base("YouTubeMusic")
    {
    }
    
    [Category("Authentication")]
    [DisplayName("Cookies")]
    [Description("YouTube Music authentication cookies (optional but recommended)")]
    public string Cookies { get; set; } = "";
    
    [Category("Authentication")]
    [DisplayName("Visitor Data")]
    [Description("YouTube visitor data for session validation")]
    public string VisitorData { get; set; } = "";
    
    [Category("Authentication")]
    [DisplayName("PoToken")]
    [Description("Proof of Origin Token for anti-bot protection")]
    public string PoToken { get; set; } = "";
    
    [Category("Regional")]
    [DisplayName("Geographical Location")]
    [Description("ISO country code for regional content (e.g., US, GB, DE)")]
    public string GeographicalLocation { get; set; } = "US";
    
    [Category("Search")]
    [DisplayName("Max Search Results")]
    [Description("Maximum number of search results to return")]
    public int MaxSearchResults { get; set; } = 50;
    
    [Category("Search")]
    [DisplayName("Include Podcasts")]
    [Description("Include podcasts in search results")]
    public bool IncludePodcasts { get; set; } = false;
    
    [Category("Search")]
    [DisplayName("Include Videos")]
    [Description("Include music videos in search results")]
    public bool IncludeVideos { get; set; } = true;
    
    [Category("Streaming")]
    [DisplayName("Preferred Audio Quality")]
    [Description("Preferred audio quality for streaming")]
    public AudioQuality PreferredAudioQuality { get; set; } = AudioQuality.High;
    
    [Category("Streaming")]
    [DisplayName("Use HLS Streaming")]
    [Description("Use HLS (HTTP Live Streaming) when available")]
    public bool UseHlsStreaming { get; set; } = false;
    
    [Category("Advanced")]
    [DisplayName("Request Timeout (seconds)")]
    [Description("Timeout for API requests")]
    public int RequestTimeoutSeconds { get; set; } = 30;
    
    [Category("Advanced")]
    [DisplayName("Retry Count")]
    [Description("Number of retries for failed requests")]
    public int RetryCount { get; set; } = 3;
    
    [Category("Advanced")]
    [DisplayName("Enable Debug Logging")]
    [Description("Enable detailed debug logging for YouTube Music operations")]
    public bool EnableDebugLogging { get; set; } = false;
    
    [Category("Session Generation")]
    [DisplayName("Auto Generate Session")]
    [Description("Automatically generate session tokens when needed")]
    public bool AutoGenerateSession { get; set; } = true;
    
    [Category("Session Generation")]
    [DisplayName("Node.js Path")]
    [Description("Path to Node.js executable for session generation (leave empty for auto-detection)")]
    public string NodeJsPath { get; set; } = "";
}

public enum AudioQuality
{
    [Description("Low (64-128 kbps)")]
    Low,
    
    [Description("Medium (128-192 kbps)")]
    Medium,
    
    [Description("High (192-320 kbps)")]
    High,
    
    [Description("Highest Available")]
    Highest
}