using System.IO;

namespace StreamingPlayerNET.Common.Utils;

public static class AudioFormatUtils
{
    /// <summary>
    /// Supported audio file extensions
    /// </summary>
    public static readonly string[] SupportedExtensions = { ".mp3", ".m4a", ".wav", ".flac", ".aac", ".ogg", ".opus", ".webm" };
    
    /// <summary>
    /// Checks if a file extension is supported for playback
    /// </summary>
    /// <param name="extension">File extension (with or without dot)</param>
    /// <returns>True if the format is supported</returns>
    public static bool IsSupportedFormat(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return false;
            
        var normalizedExtension = extension.StartsWith(".") ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
        return SupportedExtensions.Contains(normalizedExtension);
    }
    
    /// <summary>
    /// Gets the file extension from a file path
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <returns>File extension in lowercase</returns>
    public static string GetFileExtension(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;
            
        return Path.GetExtension(filePath).ToLowerInvariant();
    }
    
    /// <summary>
    /// Checks if a file is a supported audio format based on its path
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <returns>True if the file format is supported</returns>
    public static bool IsSupportedAudioFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;
            
        var extension = GetFileExtension(filePath);
        return IsSupportedFormat(extension);
    }
    
    /// <summary>
    /// Gets a user-friendly description of the audio format
    /// </summary>
    /// <param name="extension">File extension</param>
    /// <returns>Format description</returns>
    public static string GetFormatDescription(string extension)
    {
        var normalizedExtension = extension.StartsWith(".") ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
        
        return normalizedExtension switch
        {
            ".mp3" => "MP3 Audio",
            ".m4a" => "AAC Audio (M4A)",
            ".wav" => "WAV Audio",
            ".flac" => "FLAC Audio",
            ".aac" => "AAC Audio",
            ".ogg" => "OGG Vorbis",
            ".opus" => "Opus Audio",
            ".webm" => "WebM Audio",
            _ => "Unknown Format"
        };
    }
    
    /// <summary>
    /// Gets the MIME type for an audio format
    /// </summary>
    /// <param name="extension">File extension</param>
    /// <returns>MIME type</returns>
    public static string GetMimeType(string extension)
    {
        var normalizedExtension = extension.StartsWith(".") ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
        
        return normalizedExtension switch
        {
            ".mp3" => "audio/mpeg",
            ".m4a" => "audio/mp4",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            ".aac" => "audio/aac",
            ".ogg" => "audio/ogg",
            ".opus" => "audio/opus",
            ".webm" => "audio/webm",
            _ => "audio/unknown"
        };
    }
    
    /// <summary>
    /// Gets the codec name from a file extension
    /// </summary>
    /// <param name="extension">File extension</param>
    /// <returns>Codec name</returns>
    public static string GetCodecFromExtension(string extension)
    {
        var normalizedExtension = extension.StartsWith(".") ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
        
        return normalizedExtension switch
        {
            ".m4a" => "mp4a",
            ".aac" => "aac",
            ".opus" => "opus",
            ".ogg" => "vorbis",
            ".mp3" => "mp3",
            ".flac" => "flac",
            ".webm" => "opus", // webm files typically contain opus audio
            ".wav" => "pcm",
            _ => "mp4a" // Default to mp4a
        };
    }
} 