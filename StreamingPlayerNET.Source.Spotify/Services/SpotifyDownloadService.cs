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

    public async Task<string> DownloadAudioAsync(Song song, CancellationToken cancellationToken = default)
    {
        Logger.Info($"Downloading Spotify audio for song: {song.Title}");
        
        if (song.SelectedStream == null)
        {
            throw new InvalidOperationException($"No selected stream for song: {song.Title}");
        }
        
        try
        {
            var tempFile = Path.GetTempFileName();
            var outputFile = Path.ChangeExtension(tempFile, song.SelectedStream.Extension);
            
            // Report download start
            DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs(song, "Starting download..."));
            
            var startInfo = new ProcessStartInfo
            {
                FileName = _ytdlpPath,
                Arguments = $"--extract-audio --audio-format {song.SelectedStream.Extension} --audio-quality 0 --output \"{outputFile}\" \"{song.SelectedStream.Url}\"",
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
                    ParseProgress(e.Data, song);
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
            
            if (process.ExitCode == 0 && File.Exists(outputFile))
            {
                // Report download completion
                DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs(song, "Download completed"));
                
                Logger.Info($"Successfully downloaded Spotify audio to: {outputFile}");
                return outputFile;
            }
            
            Logger.Error($"Download failed for {song.Title}. Exit code: {process.ExitCode}");
            if (errorLines.Any())
            {
                Logger.Error($"yt-dlp errors: {string.Join(Environment.NewLine, errorLines)}");
            }

            return string.Empty;
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 2)
        {
            Logger.Error(ex, $"yt-dlp.exe not found. Full error details: {ex}");
            var errorMessage = "yt-dlp.exe missing. Please ensure yt-dlp.exe is available in the application directory.";
            Logger.Error(errorMessage);
            throw new InvalidOperationException(errorMessage, ex);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error downloading Spotify audio: {song.Title}");
            return string.Empty;
        }
    }
    
    public async Task<Stream> GetAudioStreamAsync(AudioStreamInfo streamInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.Debug($"Getting audio stream for: {streamInfo.Url}");
            
            // For Spotify, we need to download the file first, then return a stream
            // Create a temporary song object for the download
            var tempSong = new Song { Title = "Spotify Track", SelectedStream = streamInfo };
            var downloadedPath = await DownloadAudioAsync(tempSong, cancellationToken: cancellationToken);
            
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

    private void ParseProgress(string output, Song song)
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
                        song,
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