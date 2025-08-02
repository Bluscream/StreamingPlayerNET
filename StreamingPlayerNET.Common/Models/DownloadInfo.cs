using System.ComponentModel;

namespace StreamingPlayerNET.Common.Models;

public class DownloadInfo : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _artist = string.Empty;
    private string _status = "Pending";
    private long _bytesDownloaded;
    private long _totalBytes;
    private DateTime _startTime;
    private TimeSpan _estimatedTimeRemaining;
    private double _progressPercentage;
    private string _errorMessage = string.Empty;

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    public string Artist
    {
        get => _artist;
        set
        {
            if (_artist != value)
            {
                _artist = value;
                OnPropertyChanged(nameof(Artist));
            }
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    public long BytesDownloaded
    {
        get => _bytesDownloaded;
        set
        {
            if (_bytesDownloaded != value)
            {
                _bytesDownloaded = value;
                OnPropertyChanged(nameof(BytesDownloaded));
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(FormattedProgress));
            }
        }
    }

    public long TotalBytes
    {
        get => _totalBytes;
        set
        {
            if (_totalBytes != value)
            {
                _totalBytes = value;
                OnPropertyChanged(nameof(TotalBytes));
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(FormattedProgress));
            }
        }
    }

    public DateTime StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime != value)
            {
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }
    }

    public TimeSpan EstimatedTimeRemaining
    {
        get => _estimatedTimeRemaining;
        set
        {
            if (_estimatedTimeRemaining != value)
            {
                _estimatedTimeRemaining = value;
                OnPropertyChanged(nameof(EstimatedTimeRemaining));
                OnPropertyChanged(nameof(FormattedTimeRemaining));
            }
        }
    }

    public double ProgressPercentage
    {
        get
        {
            if (TotalBytes > 0)
            {
                return Math.Min(100.0, (double)BytesDownloaded / TotalBytes * 100.0);
            }
            return 0.0;
        }
    }

    public string FormattedProgress
    {
        get
        {
            if (TotalBytes > 0)
            {
                var downloadedMB = BytesDownloaded / (1024.0 * 1024.0);
                var totalMB = TotalBytes / (1024.0 * 1024.0);
                return $"{downloadedMB:F1} MB / {totalMB:F1} MB ({ProgressPercentage:F1}%)";
            }
            return $"{BytesDownloaded / (1024.0 * 1024.0):F1} MB";
        }
    }

    public string ElapsedTime
    {
        get
        {
            var elapsed = DateTime.Now - StartTime;
            return elapsed.ToString(@"mm\:ss");
        }
    }

    public string FormattedTimeRemaining
    {
        get
        {
            if (EstimatedTimeRemaining.TotalSeconds > 0)
            {
                return EstimatedTimeRemaining.ToString(@"mm\:ss");
            }
            return "Calculating...";
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
    }

    public string CacheKey { get; set; } = string.Empty;
    public Song? Song { get; set; }
    public AudioStreamInfo? StreamInfo { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"{Title} - {Artist} ({Status})";
    }
} 