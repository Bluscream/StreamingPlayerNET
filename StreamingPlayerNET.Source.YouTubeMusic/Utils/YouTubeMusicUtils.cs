using System;
using System.Linq;
using System.Collections.Generic; // Added missing import for IEnumerable

namespace StreamingPlayerNET.Source.YouTubeMusic.Utils;

public static class YouTubeMusicUtils
{
    /// <summary>
    /// Removes " - Topic" suffix from artist names, which is commonly added by YouTube Music
    /// for official artist channels.
    /// </summary>
    /// <param name="artistName">The artist name to clean</param>
    /// <returns>The cleaned artist name without " - Topic" suffix</returns>
    public static string CleanArtistName(string artistName)
    {
        if (string.IsNullOrWhiteSpace(artistName))
            return artistName;
            
        return artistName.EndsWith(" - Topic", StringComparison.OrdinalIgnoreCase) 
            ? artistName.Substring(0, artistName.Length - 8) 
            : artistName;
    }
    
    /// <summary>
    /// Joins multiple artist names and cleans each one by removing " - Topic" suffix.
    /// </summary>
    /// <param name="artistNames">Collection of artist names to join and clean</param>
    /// <returns>Comma-separated string of cleaned artist names</returns>
    public static string JoinAndCleanArtistNames(IEnumerable<string> artistNames)
    {
        if (artistNames == null || !artistNames.Any())
            return string.Empty;
            
        var cleanedNames = artistNames.Select(CleanArtistName);
        return string.Join(", ", cleanedNames);
    }
} 