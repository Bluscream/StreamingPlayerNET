namespace StreamingPlayerNET.Common.Models;

public class DownloadProgressEventArgs : EventArgs
{
    public string SongTitle { get; }
    public long BytesReceived { get; }
    public long TotalBytes { get; }
    public double ProgressPercentage { get; }
    public string Status { get; }

    public DownloadProgressEventArgs(string songTitle, long bytesReceived, long totalBytes, string status = "Downloading...")
    {
        SongTitle = songTitle;
        BytesReceived = bytesReceived;
        TotalBytes = totalBytes;
        ProgressPercentage = totalBytes > 0 ? (double)bytesReceived / totalBytes * 100 : 0;
        Status = status;
    }

    public DownloadProgressEventArgs(string songTitle, string status)
    {
        SongTitle = songTitle;
        BytesReceived = 0;
        TotalBytes = 0;
        ProgressPercentage = 0;
        Status = status;
    }
} 