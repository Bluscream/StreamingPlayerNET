namespace StreamingPlayerNET.Common.Models;

public class DownloadProgressEventArgs : EventArgs
{
    public Song Song { get; }
    public long BytesReceived { get; }
    public long TotalBytes { get; }
    public double ProgressPercentage { get; }
    public string Status { get; }

    public DownloadProgressEventArgs(Song song, long bytesReceived, long totalBytes, string status = "Downloading...")
    {
        Song = song ?? throw new ArgumentNullException(nameof(song));
        BytesReceived = bytesReceived;
        TotalBytes = totalBytes;
        ProgressPercentage = totalBytes > 0 ? (double)bytesReceived / totalBytes * 100 : 0;
        Status = status;
    }

    public DownloadProgressEventArgs(Song song, string status)
    {
        Song = song ?? throw new ArgumentNullException(nameof(song));
        BytesReceived = 0;
        TotalBytes = 0;
        ProgressPercentage = 0;
        Status = status;
    }
    
    // Legacy constructor for backward compatibility
    public DownloadProgressEventArgs(string songTitle, long bytesReceived, long totalBytes, string status = "Downloading...")
    {
        Song = new Song { Title = songTitle };
        BytesReceived = bytesReceived;
        TotalBytes = totalBytes;
        ProgressPercentage = totalBytes > 0 ? (double)bytesReceived / totalBytes * 100 : 0;
        Status = status;
    }

    public DownloadProgressEventArgs(string songTitle, string status)
    {
        Song = new Song { Title = songTitle };
        BytesReceived = 0;
        TotalBytes = 0;
        ProgressPercentage = 0;
        Status = status;
    }
} 