using StreamingPlayerNET.Common.Models;

namespace StreamingPlayerNET.Source.Base.Interfaces;

/// <summary>
/// Interface for playlist services that handle playlist operations
/// </summary>
public interface IPlaylistService
{
    /// <summary>
    /// Load a playlist by its ID
    /// </summary>
    /// <param name="playlistId">The ID of the playlist to load</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The loaded playlist or null if not found</returns>
    Task<Playlist?> LoadPlaylistAsync(string playlistId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Load all playlists for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's playlists</returns>
    Task<List<Playlist>> LoadUserPlaylistsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save a playlist (create new or update existing)
    /// </summary>
    /// <param name="playlist">The playlist to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The saved playlist with updated ID if it was a new playlist</returns>
    Task<Playlist> SavePlaylistAsync(Playlist playlist, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a playlist by its ID
    /// </summary>
    /// <param name="playlistId">The ID of the playlist to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the playlist was deleted successfully</returns>
    Task<bool> DeletePlaylistAsync(string playlistId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add a song to a playlist
    /// </summary>
    /// <param name="playlistId">The ID of the playlist</param>
    /// <param name="song">The song to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the song was added successfully</returns>
    Task<bool> AddSongToPlaylistAsync(string playlistId, Song song, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove a song from a playlist
    /// </summary>
    /// <param name="playlistId">The ID of the playlist</param>
    /// <param name="songId">The ID of the song to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the song was removed successfully</returns>
    Task<bool> RemoveSongFromPlaylistAsync(string playlistId, string songId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search for playlists by query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching playlists</returns>
    Task<List<Playlist>> SearchPlaylistsAsync(string query, int maxResults = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a playlist's songs by playlist ID
    /// </summary>
    /// <param name="playlistId">The ID of the playlist</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of songs in the playlist</returns>
    Task<List<Song>> GetPlaylistSongsAsync(string playlistId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Initialize the playlist service
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Whether the playlist service is available
    /// </summary>
    bool IsAvailable { get; }
} 