namespace StreamingPlayerNET.Common.Models;

public class DownloadCompletedEventArgs : EventArgs
{
    public string CacheKey { get; }
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public string? FilePath { get; }

    public DownloadCompletedEventArgs(string cacheKey, bool success, string? errorMessage = null, string? filePath = null)
    {
        CacheKey = cacheKey;
        Success = success;
        ErrorMessage = errorMessage;
        FilePath = filePath;
    }
} 