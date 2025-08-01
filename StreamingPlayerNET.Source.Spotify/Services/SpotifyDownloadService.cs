using System.Diagnostics;
using NLog;
using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;

namespace StreamingPlayerNET.Source.Spotify.Services;

public class SpotifyDownloadService : IDownloadService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SpotifySourceSettings _settings;
    private readonly string _ytdlpPath;

    public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;

    public SpotifyDownloadService(SpotifySourceSettings settings, string ytdlpPath = "yt-dlp.exe")
    {
        _settings = settings;
        _ytdlpPath = ytdlpPath;
    }

    public async Task<string> DownloadAudioAsync(AudioStreamInfo streamInfo, string? songTitle = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Downloading Spotify audio: {songTitle ?? streamInfo.Url}");
            
            // Extract Spotify track ID from the URL
            var spotifyUrl = streamInfo.Url;
            if (!spotifyUrl.StartsWith("spotify:track:"))
            {
                Logger.Error($"Invalid Spotify URL format: {spotifyUrl}");
                return string.Empty;
            }

            var trackId = spotifyUrl.Replace("spotify:track:", "");
            var outputFileName = $"{songTitle ?? trackId}.%(ext)s";
            
            // Create output directory if it doesn't exist
            var outputDir = Path.Combine("Music", "Spotify");
            Directory.CreateDirectory(outputDir);

            var outputPath = Path.Combine(outputDir, outputFileName);

            // Build yt-dlp command arguments
            var arguments = new List<string>
            {
                $"--extract-audio",
                $"--audio-format", "mp3",
                $"--audio-quality", "0", // Best quality
                $"--output", outputPath,
                $"--no-playlist",
                $"--no-warnings",
                $"--quiet",
                $"--progress-template", "download:%(progress.downloaded_bytes)s/%(progress.total_bytes)s"
            };

            // Add quality settings based on preferences
            var bitrate = (int)_settings.PreferredQuality;
            arguments.AddRange(new[] { "--audio-bitrate", bitrate.ToString() });

            // Add the Spotify URL
            arguments.Add($"https://open.spotify.com/track/{trackId}");

            var startInfo = new ProcessStartInfo
            {
                FileName = _ytdlpPath,
                Arguments = string.Join(" ", arguments),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            var outputLines = new List<string>();
            var errorLines = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputLines.Add(e.Data);
                    ParseProgress(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorLines.Add(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                // Find the downloaded file
                var downloadedFile = Directory.GetFiles(outputDir, $"{songTitle ?? trackId}.*")
                    .FirstOrDefault(f => Path.GetExtension(f).ToLower() == ".mp3");

                if (!string.IsNullOrEmpty(downloadedFile))
                {
                    Logger.Debug($"Successfully downloaded: {downloadedFile}");
                    return downloadedFile;
                }
            }

            Logger.Error($"Download failed for {songTitle ?? trackId}. Exit code: {process.ExitCode}");
            if (errorLines.Any())
            {
                Logger.Error($"yt-dlp errors: {string.Join(Environment.NewLine, errorLines)}");
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error downloading Spotify audio: {songTitle ?? streamInfo.Url}");
            return string.Empty;
        }
    }

    public async Task<Stream> GetAudioStreamAsync(AudioStreamInfo streamInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Getting audio stream for: {streamInfo.Url}");
            
            // For Spotify, we need to download the file first, then return a stream
            var downloadedPath = await DownloadAudioAsync(streamInfo, cancellationToken: cancellationToken);
            
            if (!string.IsNullOrEmpty(downloadedPath) && File.Exists(downloadedPath))
            {
                return File.OpenRead(downloadedPath);
            }

            return Stream.Null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error getting audio stream for: {streamInfo.Url}");
            return Stream.Null;
        }
    }

    public Task<long> GetContentLengthAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Getting content length for: {url}");
            
            // For Spotify URLs, we can't get content length without downloading
            // Return 0 to indicate unknown size
            return Task.FromResult(0L);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error getting content length for: {url}");
            return Task.FromResult(0L);
        }
    }

    public bool SupportsDirectStreaming(AudioStreamInfo streamInfo)
    {
        // Spotify doesn't support direct streaming through their API
        return false;
    }

    private void ParseProgress(string output)
    {
        try
        {
            if (output.StartsWith("download:"))
            {
                var progressInfo = output.Replace("download:", "").Trim();
                var parts = progressInfo.Split('/');
                
                if (parts.Length == 2 && 
                    long.TryParse(parts[0], out var downloaded) && 
                    long.TryParse(parts[1], out var total))
                {
                    var percentage = total > 0 ? (int)((downloaded * 100) / total) : 0;
                    
                    var progressEventArgs = new DownloadProgressEventArgs(
                        "Spotify Track",
                        downloaded,
                        total,
                        $"Downloading... {percentage}%");

                    DownloadProgressChanged?.Invoke(this, progressEventArgs);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error parsing download progress");
        }
    }
} 