namespace StreamingPlayerNET.Common.Models;

public class Artist
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Url { get; set; }
    public long? SubscriberCount { get; set; }
    public long? SongCount { get; set; }
    public DateTime? JoinDate { get; set; }
    
    public List<Song> Songs { get; set; } = new();
    public List<Playlist> Playlists { get; set; } = new();
    
    public override string ToString()
    {
        return Name;
    }
}