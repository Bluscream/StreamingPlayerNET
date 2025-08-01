using System.Collections.Concurrent;
using NLog;

namespace StreamingPlayerNET.Services;

public class LogService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentQueue<LogEntry> _logEntries = new();
    private readonly int _maxEntries = 100;

    
    public event EventHandler<LogEntry>? LogEntryAdded;
    public event EventHandler<string>? StatusUpdateRequested;
    
    public LogService()
    {
        // Subscribe to NLog events
        LogManager.ConfigurationChanged += OnConfigurationChanged;
        
        // Setup log target asynchronously to avoid blocking constructor
        Task.Run(async () => 
        {
            try
            {
                // Small delay to ensure the constructor completes first
                await Task.Delay(10);
                SetupLogTarget();
            }
            catch (Exception ex)
            {
                // Log setup failed, but don't block initialization
                Logger.Warn(ex, "Failed to setup log capture target");
            }
        });
    }
    
    private void SetupLogTarget()
    {
        // Create a custom target that captures log entries
        var logCaptureTarget = new LogCaptureTarget(this);
        
        // Get current configuration or create new one if null
        var config = LogManager.Configuration ?? new NLog.Config.LoggingConfiguration();
        
        // Remove any existing logCapture target to avoid duplicates
        if (config.FindTargetByName("logCapture") != null)
        {
            config.RemoveTarget("logCapture");
            // Also remove associated rules
            for (int i = config.LoggingRules.Count - 1; i >= 0; i--)
            {
                var existingRule = config.LoggingRules[i];
                if (existingRule.Targets.Any(t => t.Name == "logCapture"))
                {
                    config.LoggingRules.RemoveAt(i);
                }
            }
        }
        
        // Add the target to NLog configuration
        config.AddTarget("logCapture", logCaptureTarget);
        
        // Add rule to capture all log entries
        var newRule = new NLog.Config.LoggingRule("*", LogLevel.Trace, logCaptureTarget);
        config.LoggingRules.Add(newRule);
        
        LogManager.Configuration = config;
    }
    
    private void OnConfigurationChanged(object? sender, NLog.Config.LoggingConfigurationChangedEventArgs e)
    {
        // Re-setup the log capture target if configuration changes (asynchronously)
        Task.Run(async () => 
        {
            try
            {
                await Task.Delay(10); // Brief delay to ensure configuration is stable
                SetupLogTarget();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to re-setup log capture target after configuration change");
            }
        });
    }
    
    public void AddLogEntry(LogEntry entry)
    {
        _logEntries.Enqueue(entry);
        
        // Keep only the last maxEntries
        while (_logEntries.Count > _maxEntries)
        {
            _logEntries.TryDequeue(out _);
        }
        
        LogEntryAdded?.Invoke(this, entry);
        
        // Update status label if this is an error with an exception
        if (entry.Level == NLog.LogLevel.Error && !string.IsNullOrEmpty(entry.Exception))
        {
            var exceptionType = GetExceptionType(entry.Exception);
            StatusUpdateRequested?.Invoke(this, $"{exceptionType}");
        }
    }
    
    private string GetExceptionType(string exceptionString)
    {
        // Extract the exception type from the exception string
        // Format is typically "System.ExceptionType: Message"
        var colonIndex = exceptionString.IndexOf(':');
        if (colonIndex > 0)
        {
            return exceptionString.Substring(0, colonIndex).Trim();
        }
        
        // If no colon, try to get the first line
        var newlineIndex = exceptionString.IndexOf('\n');
        if (newlineIndex > 0)
        {
            return exceptionString.Substring(0, newlineIndex).Trim();
        }
        
        // Fallback to the full exception string if it's short enough
        return exceptionString.Length > 50 ? exceptionString.Substring(0, 50) + "..." : exceptionString;
    }
    
    public IEnumerable<LogEntry> GetLogEntries()
    {
        return _logEntries.ToArray();
    }
    
    public void ClearLogs()
    {
        while (_logEntries.TryDequeue(out _)) { }
    }
    
    public void Dispose()
    {
        LogManager.ConfigurationChanged -= OnConfigurationChanged;
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public NLog.LogLevel Level { get; set; }
    public string LoggerName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    
    public LogEntry(DateTime timestamp, NLog.LogLevel level, string loggerName, string message, string? exception = null)
    {
        Timestamp = timestamp;
        Level = level;
        LoggerName = loggerName;
        Message = message;
        Exception = exception;
    }
}

public class LogCaptureTarget : NLog.Targets.Target
{
    private readonly LogService _logService;
    
    public LogCaptureTarget(LogService logService)
    {
        _logService = logService;
    }
    
    protected override void Write(NLog.LogEventInfo logEvent)
    {
        var entry = new LogEntry(
            logEvent.TimeStamp,
            logEvent.Level,
            logEvent.LoggerName,
            logEvent.FormattedMessage,
            logEvent.Exception?.ToString()
        );
        
        _logService.AddLogEntry(entry);
    }
} 