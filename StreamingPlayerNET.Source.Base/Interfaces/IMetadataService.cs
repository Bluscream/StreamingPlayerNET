using StreamingPlayerNET.Common.Models;

namespace StreamingPlayerNET.Source.Base.Interfaces;

public interface IMetadataService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<Song> GetSongMetadataAsync(string songId, CancellationToken cancellationToken = default);
    Task<List<AudioStreamInfo>> GetAudioStreamsAsync(string songId, CancellationToken cancellationToken = default);
    Task<AudioStreamInfo> GetBestAudioStreamAsync(string songId, CancellationToken cancellationToken = default);
}