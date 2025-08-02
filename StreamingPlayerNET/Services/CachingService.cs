using StreamingPlayerNET.Common.Models;
using StreamingPlayerNET.Source.Base.Interfaces;
using NLog;
using System.Security.Cryptography;
using System.Text;

namespace StreamingPlayerNET.Services;

public class CachingService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _cacheBasePath;
    private readonly Dictionary<string, string> _fileCache = new();
    private readonly Dictionary<string, Task<string>> _ongoingDownloads = new();
    private readonly object _cacheLock = new();
    private IDownloadService? _downloadService;
    
    public CachingService()
    {
        _cacheBasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            "StreamingPlayerNET"
        );
        
        // Ensure cache directory exists
        Directory.CreateDirectory(_cacheBasePath);
        Logger.Info($"Caching service initialized with base path: {_cacheBasePath}");
        
        // Populate in-memory cache from disk
        PopulateCacheFromDisk();
    }
    
    public void SetDownloadService(IDownloadService downloadService)
    {
        _downloadService = downloadService;
        Logger.Debug("Download service set for CachingService");
    }
    
    /// <summary>
    /// Checks if a download is currently in progress for the given song
    /// </summary>
    public bool IsDownloadInProgress(Song song, AudioStreamInfo streamInfo)
    {
        if (song == null || streamInfo == null)
            return false;
            
        var cacheKey = GenerateCacheKey(song, streamInfo);
        
        lock (_cacheLock)
        {
            return _ongoingDownloads.ContainsKey(cacheKey);
        }
    }
    
    /// <summary>
    /// Gets the number of ongoing downloads
    /// </summary>
    public int GetOngoingDownloadCount()
    {
        lock (_cacheLock)
        {
            return _ongoingDownloads.Count;
        }
    }
    
    /// <summary>
    /// Gets the cached file path for a song, or null if not cached
    /// </summary>
    public string? GetCachedFilePath(Song song, AudioStreamInfo streamInfo)
    {
        if (song == null || streamInfo == null)
            return null;
            
        var cacheKey = GenerateCacheKey(song, streamInfo);
        
        lock (_cacheLock)
        {
            if (_fileCache.TryGetValue(cacheKey, out var cachedPath) && File.Exists(cachedPath))
            {
                Logger.Debug($"Cache hit for song: {song.Title}");
                return cachedPath;
            }
        }
        
        // Fallback: search for the file on disk
        var fallbackPath = FindCachedFileOnDisk(song, streamInfo);
        if (fallbackPath != null)
        {
            Logger.Debug($"Found cached file on disk for song: {song.Title}");
            // Add to in-memory cache for future lookups
            lock (_cacheLock)
            {
                _fileCache[cacheKey] = fallbackPath;
            }
            return fallbackPath;
        }
        
        Logger.Debug($"Cache miss for song: {song.Title}");
        return null;
    }
    
    /// <summary>
    /// Caches a downloaded file with proper directory structure
    /// </summary>
    public async Task<string> CacheFileAsync(Song song, AudioStreamInfo streamInfo, string tempFilePath, CancellationToken cancellationToken = default)
    {
        if (song == null || streamInfo == null || string.IsNullOrEmpty(tempFilePath))
            throw new ArgumentException("Invalid parameters for caching");
            
        if (!File.Exists(tempFilePath))
            throw new FileNotFoundException($"Temp file not found: {tempFilePath}");
            
        var cacheKey = GenerateCacheKey(song, streamInfo);
        var targetPath = GenerateTargetPath(song, streamInfo);
        
        try
        {
            // Ensure target directory exists
            var targetDir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            
            // Copy file to cache location
            if (File.Exists(targetPath))
            {
                Logger.Debug($"Overwriting existing cached file: {targetPath}");
                File.Delete(targetPath);
            }
            
            File.Copy(tempFilePath, targetPath, true);
            
            // Verify the file was copied successfully
            if (!File.Exists(targetPath))
            {
                throw new IOException($"Failed to copy file to cache location: {targetPath}");
            }
            
            // Add to cache dictionary
            lock (_cacheLock)
            {
                _fileCache[cacheKey] = targetPath;
            }
            
            Logger.Info($"Successfully cached file: {song.Title} -> {targetPath}");
            return targetPath;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to cache file for song: {song.Title}");
            throw;
        }
    }
    
    /// <summary>
    /// Downloads and caches a file from a URL
    /// </summary>
    public async Task<string> DownloadAndCacheAsync(Song song, AudioStreamInfo streamInfo, CancellationToken cancellationToken = default)
    {
        if (song == null || streamInfo == null)
            throw new ArgumentException("Invalid parameters for download and cache");
            
        var cacheKey = GenerateCacheKey(song, streamInfo);
        
        // Check if already cached
        lock (_cacheLock)
        {
            if (_fileCache.TryGetValue(cacheKey, out var cachedPath) && File.Exists(cachedPath))
            {
                Logger.Debug($"Using cached file for song: {song.Title}");
                return cachedPath;
            }
        }
        
        // Check if download is already in progress
        Task<string>? existingDownloadTask;
        lock (_cacheLock)
        {
            if (_ongoingDownloads.TryGetValue(cacheKey, out existingDownloadTask))
            {
                Logger.Debug($"Download already in progress for song: {song.Title}, waiting for completion");
            }
        }
        
        // Wait for existing download outside of lock
        if (existingDownloadTask != null)
        {
            return await existingDownloadTask;
        }
        
        var targetPath = GenerateTargetPath(song, streamInfo);
        
        // Create the download task
        var downloadTask = Task.Run(async () =>
        {
            try
            {
                // Ensure target directory exists
                var targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                
                string downloadedFilePath;
                
                if (_downloadService != null)
                {
                    // Use the download service (with progress events)
                    Logger.Debug($"CachingService using download service for: {song.Title}");
                    downloadedFilePath = await _downloadService.DownloadAudioAsync(streamInfo, song.Title, cancellationToken);
                    
                    // Move the downloaded file to the cache location
                    if (File.Exists(downloadedFilePath))
                    {
                        if (downloadedFilePath != targetPath)
                        {
                            File.Move(downloadedFilePath, targetPath, true);
                        }
                    }
                    else
                    {
                        throw new IOException($"Download service did not create file: {downloadedFilePath}");
                    }
                }
                else
                {
                    // Fallback to direct HTTP download (without progress events)
                    Logger.Debug($"CachingService falling back to direct HTTP download: {song.Title} from {streamInfo.Url}");
                    
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromMinutes(5); // 5 minute timeout for downloads
                    
                    using var response = await httpClient.GetAsync(streamInfo.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    
                    using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    
                    await contentStream.CopyToAsync(fileStream, cancellationToken);
                    await fileStream.FlushAsync(cancellationToken);
                }
                
                // Verify the file was downloaded successfully
                if (!File.Exists(targetPath))
                {
                    throw new IOException($"Failed to download file to cache location: {targetPath}");
                }
                
                var fileInfo = new FileInfo(targetPath);
                if (fileInfo.Length == 0)
                {
                    throw new IOException($"Downloaded file is empty: {targetPath}");
                }
                
                // Add to cache dictionary
                lock (_cacheLock)
                {
                    _fileCache[cacheKey] = targetPath;
                }
                
                Logger.Info($"Successfully downloaded and cached file: {song.Title} -> {targetPath} ({fileInfo.Length} bytes)");
                return targetPath;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to download and cache file for song: {song.Title}");
                
                // Clean up partial file if it exists
                try
                {
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                }
                catch (Exception cleanupEx)
                {
                    Logger.Warn(cleanupEx, $"Failed to clean up partial file: {targetPath}");
                }
                
                throw;
            }
            finally
            {
                // Remove from ongoing downloads
                lock (_cacheLock)
                {
                    _ongoingDownloads.Remove(cacheKey);
                }
            }
        }, cancellationToken);
        
        // Add to ongoing downloads
        lock (_cacheLock)
        {
            _ongoingDownloads[cacheKey] = downloadTask;
        }
        
        return await downloadTask;
    }
    
    /// <summary>
    /// Saves a stream to cache
    /// </summary>
    public async Task<string> SaveStreamToCacheAsync(Song song, AudioStreamInfo streamInfo, Stream sourceStream, CancellationToken cancellationToken = default)
    {
        if (song == null || streamInfo == null || sourceStream == null)
            throw new ArgumentException("Invalid parameters for stream caching");
            
        var targetPath = GenerateTargetPath(song, streamInfo);
        
        try
        {
            // Ensure target directory exists
            var targetDir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            
            Logger.Info($"Saving stream to cache: {song.Title} -> {targetPath}");
            
            using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await sourceStream.CopyToAsync(fileStream, cancellationToken);
            await fileStream.FlushAsync(cancellationToken);
            
            // Verify the file was saved successfully
            if (!File.Exists(targetPath))
            {
                throw new IOException($"Failed to save stream to cache location: {targetPath}");
            }
            
            var fileInfo = new FileInfo(targetPath);
            if (fileInfo.Length == 0)
            {
                throw new IOException($"Saved stream file is empty: {targetPath}");
            }
            
            // Add to cache dictionary
            var cacheKey = GenerateCacheKey(song, streamInfo);
            lock (_cacheLock)
            {
                _fileCache[cacheKey] = targetPath;
            }
            
            Logger.Info($"Successfully saved stream to cache: {song.Title} -> {targetPath} ({fileInfo.Length} bytes)");
            return targetPath;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to save stream to cache for song: {song.Title}");
            
            // Clean up partial file if it exists
            try
            {
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
            }
            catch (Exception cleanupEx)
            {
                Logger.Warn(cleanupEx, $"Failed to clean up partial file: {targetPath}");
            }
            
            throw;
        }
    }
    
    /// <summary>
    /// Clears the cache for a specific song
    /// </summary>
    public void ClearCacheForSong(Song song, AudioStreamInfo streamInfo)
    {
        if (song == null || streamInfo == null)
            return;
            
        var cacheKey = GenerateCacheKey(song, streamInfo);
        
        lock (_cacheLock)
        {
            if (_fileCache.TryGetValue(cacheKey, out var cachedPath))
            {
                try
                {
                    if (File.Exists(cachedPath))
                    {
                        File.Delete(cachedPath);
                        Logger.Debug($"Cleared cache for song: {song.Title}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Failed to delete cached file: {cachedPath}");
                }
                
                _fileCache.Remove(cacheKey);
            }
        }
    }
    
    /// <summary>
    /// Clears all cached files
    /// </summary>
    public void ClearAllCache()
    {
        lock (_cacheLock)
        {
            foreach (var cachedPath in _fileCache.Values)
            {
                try
                {
                    if (File.Exists(cachedPath))
                    {
                        File.Delete(cachedPath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Failed to delete cached file: {cachedPath}");
                }
            }
            
            _fileCache.Clear();
            Logger.Info("All cache cleared");
        }
    }
    
    /// <summary>
    /// Gets cache statistics
    /// </summary>
    public (int FileCount, long TotalSize) GetCacheStats()
    {
        lock (_cacheLock)
        {
            var fileCount = 0;
            var totalSize = 0L;
            
            foreach (var cachedPath in _fileCache.Values)
            {
                if (File.Exists(cachedPath))
                {
                    fileCount++;
                    try
                    {
                        var fileInfo = new FileInfo(cachedPath);
                        totalSize += fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"Failed to get file size: {cachedPath}");
                    }
                }
            }
            
            return (fileCount, totalSize);
        }
    }
    
    /// <summary>
    /// Searches for a cached file on disk based on song metadata
    /// </summary>
    private string? FindCachedFileOnDisk(Song song, AudioStreamInfo streamInfo)
    {
        try
        {
            var playlistName = "Unknown Playlist";
            var artistName = SanitizeFileName(song.Artist ?? "Unknown Artist");
            var albumName = SanitizeFileName(song.Album ?? "Unknown Album");
            var songTitle = SanitizeFileName(song.Title ?? "Unknown Song");
            
            // Determine expected file extension
            var extension = !string.IsNullOrEmpty(streamInfo.Extension) 
                ? streamInfo.Extension 
                : GetExtensionFromCodec(streamInfo.AudioCodec);
            
            // Build expected path
            var expectedFileName = $"{songTitle}.{extension}";
            var expectedPath = Path.Combine(_cacheBasePath, playlistName, artistName, albumName, expectedFileName);
            
            // Check if the expected file exists
            if (File.Exists(expectedPath))
            {
                return expectedPath;
            }
            
            // If not found, search more broadly in the artist/album directory
            var searchDir = Path.Combine(_cacheBasePath, playlistName, artistName, albumName);
            if (Directory.Exists(searchDir))
            {
                var files = Directory.GetFiles(searchDir, $"{songTitle}.*");
                if (files.Length > 0)
                {
                    return files[0]; // Return the first match
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, $"Failed to search for cached file on disk for song: {song.Title}");
            return null;
        }
    }
    
    /// <summary>
    /// Populates the in-memory cache from existing files on disk
    /// </summary>
    private void PopulateCacheFromDisk()
    {
        try
        {
            Logger.Info("Populating in-memory cache from disk...");
            var fileCount = 0;
            
            // Recursively scan all files in the cache directory
            var allFiles = Directory.GetFiles(_cacheBasePath, "*.*", SearchOption.AllDirectories);
            
            foreach (var filePath in allFiles)
            {
                try
                {
                    // Extract song information from file path
                    var relativePath = Path.GetRelativePath(_cacheBasePath, filePath);
                    var pathParts = relativePath.Split(Path.DirectorySeparatorChar);
                    
                    if (pathParts.Length >= 4) // playlist/artist/album/song.ext
                    {
                        var playlistName = pathParts[0];
                        var artistName = pathParts[1];
                        var albumName = pathParts[2];
                        var fileName = pathParts[3];
                        var songTitle = Path.GetFileNameWithoutExtension(fileName);
                        
                        // Create a temporary Song object for cache key generation
                        var song = new Song
                        {
                            Title = songTitle,
                            Artist = artistName,
                            Album = albumName,
                            PlaylistName = playlistName
                        };
                        
                        // Create a temporary AudioStreamInfo object for cache key generation
                        var extension = Path.GetExtension(fileName).TrimStart('.');
                        var audioCodec = GetCodecFromExtension(extension);
                        var streamInfo = new AudioStreamInfo
                        {
                            AudioCodec = audioCodec,
                            Extension = extension,
                            FormatId = "unknown", // We don't have this info from disk
                            AudioBitrate = 0 // We don't have this info from disk
                        };
                        
                        // Generate cache key and add to in-memory cache
                        var cacheKey = GenerateCacheKey(song, streamInfo);
                        lock (_cacheLock)
                        {
                            _fileCache[cacheKey] = filePath;
                        }
                        fileCount++;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Failed to process cached file: {filePath}");
                }
            }
            
            Logger.Info($"Populated in-memory cache with {fileCount} files from disk");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to populate in-memory cache from disk");
        }
    }
    
    private string GenerateCacheKey(Song song, AudioStreamInfo streamInfo)
    {
        // Create a unique cache key based on song metadata and codec
        // Use a more stable key that doesn't depend on stream-specific info
        var keyData = $"{song.Title}_{song.Artist}_{song.Album}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyData));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
    
    private string GenerateTargetPath(Song song, AudioStreamInfo streamInfo)
    {
        // Sanitize names for file system compatibility
        var playlistName = "Unknown Playlist"; // SanitizeFileName(song.PlaylistName ?? "Unknown Playlist");
        var artistName = SanitizeFileName(song.Artist ?? "Unknown Artist");
        var albumName = SanitizeFileName(song.Album ?? "Unknown Album");
        var songTitle = SanitizeFileName(song.Title ?? "Unknown Song");
        
        // Determine file extension
        var extension = !string.IsNullOrEmpty(streamInfo.Extension) 
            ? streamInfo.Extension 
            : GetExtensionFromCodec(streamInfo.AudioCodec);
        
        // Build path: MyMusic/StreamingPlayerNET/Artist/Album/Song.ext
        // Note: We don't use PlaylistName to avoid source segregation
        var fileName = $"{songTitle}.{extension}";
        var targetPath = Path.Combine(_cacheBasePath, playlistName, artistName, albumName, fileName);
        
        return targetPath;
    }
    
    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "Unknown";
            
        // Remove or replace invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;
        
        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }
        
        // Limit length to avoid path too long errors
        if (sanitized.Length > 100)
        {
            sanitized = sanitized.Substring(0, 100);
        }
        
        return sanitized.Trim();
    }
    
    private string GetExtensionFromCodec(string? codec)
    {
        if (string.IsNullOrEmpty(codec))
            return "m4a";
            
        return codec.ToLowerInvariant() switch
        {
            var c when c.Contains("mp4a") || c.Contains("aac") => "m4a",
            var c when c.Contains("opus") => "opus",
            var c when c.Contains("vorbis") => "ogg",
            var c when c.Contains("mp3") => "mp3",
            var c when c.Contains("flac") => "flac",
            _ => "m4a" // Default to m4a
        };
    }
    
    private string GetCodecFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            "m4a" => "mp4a",
            "opus" => "opus",
            "ogg" => "vorbis",
            "mp3" => "mp3",
            "flac" => "flac",
            "webm" => "opus", // webm files typically contain opus audio
            _ => "mp4a" // Default to mp4a
        };
    }
} 