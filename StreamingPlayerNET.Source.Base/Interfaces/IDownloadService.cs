using StreamingPlayerNET.Common.Models;

namespace StreamingPlayerNET.Source.Base.Interfaces;

public interface IDownloadService
{
    event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;
    
    Task<string> DownloadAudioAsync(Song song, CancellationToken cancellationToken = default);
    Task<Stream> GetAudioStreamAsync(AudioStreamInfo streamInfo, CancellationToken cancellationToken = default);
    Task<long> GetContentLengthAsync(string url, CancellationToken cancellationToken = default);
    bool SupportsDirectStreaming(AudioStreamInfo streamInfo);
}