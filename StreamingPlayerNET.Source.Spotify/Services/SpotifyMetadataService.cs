using NLog;
using SpotifyAPI.Web;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;

namespace StreamingPlayerNET.Source.Spotify.Services;

public class SpotifyMetadataService : IMetadataService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SpotifyClient _spotifyClient;
    private readonly SpotifySourceSettings _settings;

    public SpotifyMetadataService(SpotifyClient spotifyClient, SpotifySourceSettings settings)
    {
        _spotifyClient = spotifyClient;
        _settings = settings;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Logger.Debug("Spotify Metadata Service initialized");
        return Task.CompletedTask;
    }

    public async Task<Song> GetSongMetadataAsync(string songId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Getting metadata for Spotify track: {songId}");
            
            var track = await _spotifyClient.Tracks.Get(songId);
            
            var song = new Song
            {
                Id = track.Id,
                Title = track.Name,
                Artist = track.Artists?.FirstOrDefault()?.Name ?? "Unknown Artist",
                Album = track.Album?.Name,
                Duration = track.DurationMs > 0 ? TimeSpan.FromMilliseconds(track.DurationMs) : null,
                ThumbnailUrl = track.Album?.Images?.FirstOrDefault()?.Url,
                Url = track.ExternalUrls?.FirstOrDefault().Value,
                Description = $"Track from {track.Album?.Name} by {string.Join(", ", track.Artists?.Select(a => a.Name) ?? Array.Empty<string>())}",
                Source = "Spotify"
            };

            Logger.Debug($"Retrieved metadata for track: {song.Title} - {song.Artist}");
            return song;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error getting metadata for Spotify track: {songId}");
            return new Song { Id = songId, Source = "Spotify" };
        }
    }

    public Task<List<AudioStreamInfo>> GetAudioStreamsAsync(string songId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Getting audio streams for Spotify track: {songId}");
            
            // Spotify doesn't provide direct audio streams through their API
            // We'll create a placeholder stream info that indicates the track needs to be processed
            var streamInfo = new AudioStreamInfo
            {
                Url = $"spotify:track:{songId}",
                AudioBitrate = (int)_settings.PreferredQuality,
                Extension = "spotify",
                AudioCodec = "spotify",
                Container = "spotify"
            };

            Logger.Debug($"Created stream info for Spotify track: {songId}");
            return Task.FromResult(new List<AudioStreamInfo> { streamInfo });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error getting audio streams for Spotify track: {songId}");
            return Task.FromResult(new List<AudioStreamInfo>());
        }
    }

    public Task<AudioStreamInfo> GetBestAudioStreamAsync(string songId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Getting best audio stream for Spotify track: {songId}");
            
            // For Spotify, we create a simple stream info since we can't get actual streams
            var streamInfo = new AudioStreamInfo
            {
                Url = $"spotify:track:{songId}",
                AudioBitrate = (int)_settings.PreferredQuality,
                Extension = "spotify",
                AudioCodec = "spotify",
                Container = "spotify"
            };

            Logger.Debug($"Created stream info for Spotify track: {songId}");
            return Task.FromResult(streamInfo);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error getting best audio stream for Spotify track: {songId}");
            return Task.FromResult(new AudioStreamInfo());
        }
    }

    private static string GetQualityString(SpotifyAudioQuality quality)
    {
        return quality switch
        {
            SpotifyAudioQuality.Low => "Low (96 kbps)",
            SpotifyAudioQuality.Medium => "Medium (160 kbps)",
            SpotifyAudioQuality.High => "High (320 kbps)",
            _ => "Unknown"
        };
    }
} 