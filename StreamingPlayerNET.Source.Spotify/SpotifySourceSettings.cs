using System.ComponentModel;
using StreamingPlayerNET.Source.Base;

namespace StreamingPlayerNET.Source.Spotify;

public class SpotifySourceSettings : BaseSourceSettings
{
    public SpotifySourceSettings() : base("Spotify")
    {
    }
    
    [Category("Authentication")]
    [DisplayName("Client ID")]
    [Description("Spotify API Client ID")]
    public string ClientId { get; set; } = "";
    
    [Category("Authentication")]
    [DisplayName("Client Secret")]
    [Description("Spotify API Client Secret")]
    public string ClientSecret { get; set; } = "";
    
    [Category("Authentication")]
    [DisplayName("Redirect URI")]
    [Description("OAuth redirect URI for authentication")]
    public string RedirectUri { get; set; } = "http://localhost:8888/callback";
    
    [Category("Search")]
    [DisplayName("Search Type")]
    [Description("Type of search to perform")]
    public SpotifySearchType SearchType { get; set; } = SpotifySearchType.Track;
    
    [Category("Search")]
    [DisplayName("Include Albums")]
    [Description("Include albums in search results")]
    public bool IncludeAlbums { get; set; } = true;
    
    [Category("Search")]
    [DisplayName("Include Playlists")]
    [Description("Include playlists in search results")]
    public bool IncludePlaylists { get; set; } = true;
    
    [Category("Search")]
    [DisplayName("Max Results")]
    [Description("Maximum number of search results")]
    public int MaxSearchResults { get; set; } = 50;
    
    [Category("Playback")]
    [DisplayName("Preferred Quality")]
    [Description("Preferred audio quality")]
    public SpotifyAudioQuality PreferredQuality { get; set; } = SpotifyAudioQuality.High;
    
    [Category("Playback")]
    [DisplayName("Enable Crossfade")]
    [Description("Enable crossfade between tracks")]
    public bool EnableCrossfade { get; set; } = false;
    
    [Category("Advanced")]
    [DisplayName("Request Timeout (seconds)")]
    [Description("Timeout for API requests")]
    public int RequestTimeoutSeconds { get; set; } = 30;
    
    [Category("Advanced")]
    [DisplayName("Enable Debug Logging")]
    [Description("Enable detailed debug logging")]
    public bool EnableDebugLogging { get; set; } = false;
}

public enum SpotifySearchType
{
    [Description("Track")]
    Track,
    
    [Description("Album")]
    Album,
    
    [Description("Playlist")]
    Playlist,
    
    [Description("Artist")]
    Artist,
    
    [Description("All")]
    All
}

public enum SpotifyAudioQuality
{
    [Description("Low (96 kbps)")]
    Low = 96,
    
    [Description("Medium (160 kbps)")]
    Medium = 160,
    
    [Description("High (320 kbps)")]
    High = 320
} 