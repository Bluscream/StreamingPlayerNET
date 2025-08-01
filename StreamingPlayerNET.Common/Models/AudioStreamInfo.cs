using Humanizer;

namespace StreamingPlayerNET.Common.Models;

public class AudioStreamInfo
{
    public string Url { get; set; } = string.Empty;
    public string FormatId { get; set; } = string.Empty;
    public string AudioCodec { get; set; } = string.Empty;
    public string? VideoCodec { get; set; }
    public string Extension { get; set; } = string.Empty;
    public long? AudioBitrate { get; set; }
    public long? FileSize { get; set; }
    public string Container { get; set; } = string.Empty;
    
    // Calculated properties
    public long? EstimatedFileSize => FileSize ?? (AudioBitrate.HasValue && Duration.HasValue 
        ? (long)(Duration.Value.TotalSeconds * AudioBitrate.Value * 1000 / 8) 
        : null);
    
    public TimeSpan? Duration { get; set; }
    
    public bool IsAudioOnly => string.IsNullOrEmpty(VideoCodec);
    
    public override string ToString()
    {
        var size = EstimatedFileSize?.Bytes().ToString() ?? "Unknown";
        var duration = Duration?.Humanize() ?? "Unknown";
        return $"{AudioCodec} @ {AudioBitrate} kbps ({size}) - {duration}";
    }
}