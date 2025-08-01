using StreamingPlayerNET.Common.Models;

namespace StreamingPlayerNET.Source.Base.Interfaces;

public interface ISearchService
{
    Task<List<Song>> SearchAsync(string query, int maxResults = 50, CancellationToken cancellationToken = default);
    Task<List<Song>> SearchByArtistAsync(string artist, int maxResults = 50, CancellationToken cancellationToken = default);
    Task<List<Song>> SearchByPlaylistAsync(string playlistId, CancellationToken cancellationToken = default);
    Task<List<Playlist>> SearchPlaylistsAsync(string query, int maxResults = 20, CancellationToken cancellationToken = default);
}