using System.Net;
using System.Text.Json;
using NLog;
using SpotifyAPI.Web;

namespace StreamingPlayerNET.Source.Spotify.Services;

public class SpotifyAuthService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SpotifySourceSettings _settings;
    private SpotifyClient? _spotifyClient;
    private string? _accessToken;
    private string? _refreshToken;
    private DateTime _tokenExpiry;

    public SpotifyAuthService(SpotifySourceSettings settings)
    {
        _settings = settings;
    }

    public SpotifyClient? Client => _spotifyClient;

    public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Info("Starting Spotify authentication...");

            if (string.IsNullOrEmpty(_settings.ClientId) || string.IsNullOrEmpty(_settings.ClientSecret))
            {
                Logger.Error("Spotify Client ID and Client Secret are required for authentication");
                return false;
            }

            // Try to load existing tokens first
            if (await LoadTokensAsync())
            {
                if (await RefreshTokenIfNeededAsync())
                {
                    Logger.Info("Successfully authenticated using existing tokens");
                    return true;
                }
            }

            // Perform new authentication
            return await PerformNewAuthenticationAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during Spotify authentication");
            return false;
        }
    }

    private async Task<bool> LoadTokensAsync()
    {
        try
        {
            var tokenFile = Path.Combine("AppData", "spotify_tokens.json");
            if (!File.Exists(tokenFile))
            {
                return false;
            }

            var tokenData = await File.ReadAllTextAsync(tokenFile);
            var tokens = JsonSerializer.Deserialize<SpotifyTokens>(tokenData);

            if (tokens != null && !string.IsNullOrEmpty(tokens.AccessToken))
            {
                _accessToken = tokens.AccessToken;
                _refreshToken = tokens.RefreshToken;
                _tokenExpiry = tokens.ExpiryTime;
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error loading Spotify tokens");
            return false;
        }
    }

    private async Task<bool> SaveTokensAsync()
    {
        try
        {
            var tokenFile = Path.Combine("AppData", "spotify_tokens.json");
            Directory.CreateDirectory(Path.GetDirectoryName(tokenFile)!);

            var tokens = new SpotifyTokens
            {
                AccessToken = _accessToken,
                RefreshToken = _refreshToken,
                ExpiryTime = _tokenExpiry
            };

            var tokenData = JsonSerializer.Serialize(tokens, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(tokenFile, tokenData);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error saving Spotify tokens");
            return false;
        }
    }

    private async Task<bool> RefreshTokenIfNeededAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _tokenExpiry)
            {
                return await CreateClientCredentialsFlow();
            }

            // Create client with existing token
            var config = SpotifyClientConfig.CreateDefault().WithToken(_accessToken);
            _spotifyClient = new SpotifyClient(config);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error refreshing Spotify token");
            return false;
        }
    }

    private async Task<bool> PerformNewAuthenticationAsync(CancellationToken cancellationToken)
    {
        return await CreateClientCredentialsFlow();
    }

    private async Task<bool> CreateClientCredentialsFlow()
    {
        try
        {
            Logger.Info("Using Spotify Client Credentials flow for authentication");
            
            // For now, use a simplified approach with client credentials
            // This provides access to public data but not user-specific data
            var config = SpotifyClientConfig.CreateDefault(_settings.ClientId, _settings.ClientSecret);
            _spotifyClient = new SpotifyClient(config);

            // Test the connection
            var testSearch = await _spotifyClient.Search.Item(new SearchRequest(SearchRequest.Types.Track, "test") { Limit = 1 });
            
            if (testSearch != null)
            {
                _accessToken = "client_credentials_token"; // Placeholder
                _tokenExpiry = DateTime.UtcNow.AddHours(1); // Client credentials tokens typically last 1 hour
                await SaveTokensAsync();
                
                Logger.Info("Successfully authenticated with Spotify using Client Credentials");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during Spotify Client Credentials authentication");
            return false;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        if (_spotifyClient == null)
        {
            return false;
        }

        try
        {
            // Test the connection by getting the current user profile
            var profile = await _spotifyClient.UserProfile.Current();
            return !string.IsNullOrEmpty(profile.Id);
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Spotify authentication test failed");
            return false;
        }
    }

    public void Logout()
    {
        _spotifyClient = null;
        _accessToken = null;
        _refreshToken = null;
        _tokenExpiry = DateTime.MinValue;

        try
        {
            var tokenFile = Path.Combine("AppData", "spotify_tokens.json");
            if (File.Exists(tokenFile))
            {
                File.Delete(tokenFile);
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error deleting Spotify tokens file");
        }

        Logger.Info("Logged out of Spotify");
    }

    private class SpotifyTokens
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
} 