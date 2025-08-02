namespace StreamingPlayerNET.Common.Models;

public class DownloadCompletedEventArgs : EventArgs
{
    public Song Song { get; }
    public string CacheKey { get; }
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public string? FilePath { get; }

    public DownloadCompletedEventArgs(Song song, string cacheKey, bool success, string? errorMessage = null, string? filePath = null)
    {
        Song = song ?? throw new ArgumentNullException(nameof(song));
        CacheKey = cacheKey;
        Success = success;
        ErrorMessage = errorMessage;
        FilePath = filePath;
    }
    
    // Legacy constructor for backward compatibility
    public DownloadCompletedEventArgs(string cacheKey, bool success, string? errorMessage = null, string? filePath = null)
    {
        Song = new Song(); // Empty song for legacy compatibility
        CacheKey = cacheKey;
        Success = success;
        ErrorMessage = errorMessage;
        FilePath = filePath;
    }
} 