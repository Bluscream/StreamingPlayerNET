namespace StreamingPlayerNET.Common.Models;

public class Playlist
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Author { get; set; }
    public int SongCount { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsPublic { get; set; } = true;
    public string? Source { get; set; }
    
    public List<Song> Songs { get; set; } = new();
    
    public void AddSong(Song song)
    {
        if (!Songs.Any(s => s.Id == song.Id))
        {
            Songs.Add(song);
            SongCount = Songs.Count;
        }
    }
    
    public void RemoveSong(string songId)
    {
        var song = Songs.FirstOrDefault(s => s.Id == songId);
        if (song != null)
        {
            Songs.Remove(song);
            SongCount = Songs.Count;
        }
    }
    
    public void Clear()
    {
        Songs.Clear();
        SongCount = 0;
    }
    
    public override string ToString()
    {
        return $"{Name} ({SongCount} songs)";
    }
}