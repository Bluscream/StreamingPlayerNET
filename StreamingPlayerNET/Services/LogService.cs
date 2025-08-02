using System.Collections.Concurrent;
using NLog;
using StreamingPlayerNET.Common.Models;

namespace StreamingPlayerNET.Services;

public class LogService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentQueue<LogEntry> _logEntries = new();
    private readonly int _maxEntries = 100;
    private bool _isSetupComplete = false;
    private ConfigurationService? _configService;

    
    public event EventHandler<LogEntry>? LogEntryAdded;
    public event EventHandler<string>? StatusUpdateRequested;
    
    public LogService(ConfigurationService? configService = null)
    {
        _configService = configService;
        
        // Subscribe to NLog events
        LogManager.ConfigurationChanged += OnConfigurationChanged;
        
        // Setup log target immediately to capture all log entries
        SetupLogTarget();
    }
    
    private void SetupLogTarget()
    {
        if (_isSetupComplete) return; // Prevent multiple setups
        
        try
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
            var newRule = new NLog.Config.LoggingRule("*", NLog.LogLevel.Trace, logCaptureTarget);
            config.LoggingRules.Add(newRule);
            
            // Apply the configuration
            LogManager.Configuration = config;
            
            _isSetupComplete = true;
            Logger.Debug("Log capture target setup completed");
        }
        catch (Exception ex)
        {
            // Log setup failed, but don't block initialization
            Logger.Warn(ex, "Failed to setup log capture target");
        }
    }
    
    private void OnConfigurationChanged(object? sender, NLog.Config.LoggingConfigurationChangedEventArgs e)
    {
        // Only re-setup if our target was removed
        if (LogManager.Configuration?.FindTargetByName("logCapture") == null)
        {
            _isSetupComplete = false;
            SetupLogTarget();
        }
    }
    
    private bool ShouldDisplayLogEntry(LogEntry entry)
    {
        // If no config service, show all entries
        if (_configService == null)
            return true;
            
        var config = ConfigurationService.Current;
        if (config == null)
            return true;
        
        // Check if debug mode is enabled
        if (config.EnableDebugMode)
        {
            // In debug mode, show all log levels
            return true;
        }
        
        // Check log level setting
        var minLogLevel = config.LogLevel;
        var entryLogLevel = ConvertToLogLevel(entry.Level);
        
        return entryLogLevel >= minLogLevel;
    }
    
    private StreamingPlayerNET.Common.Models.LogLevel ConvertToLogLevel(NLog.LogLevel nlogLevel)
    {
        return nlogLevel.Name.ToUpper() switch
        {
            "TRACE" => StreamingPlayerNET.Common.Models.LogLevel.Trace,
            "DEBUG" => StreamingPlayerNET.Common.Models.LogLevel.Debug,
            "INFO" => StreamingPlayerNET.Common.Models.LogLevel.Info,
            "WARN" => StreamingPlayerNET.Common.Models.LogLevel.Warning,
            "ERROR" => StreamingPlayerNET.Common.Models.LogLevel.Error,
            "FATAL" => StreamingPlayerNET.Common.Models.LogLevel.Fatal,
            _ => StreamingPlayerNET.Common.Models.LogLevel.Info
        };
    }
    
    public void AddLogEntry(LogEntry entry)
    {
        // Only add to the queue if it should be displayed based on configuration
        if (ShouldDisplayLogEntry(entry))
        {
            _logEntries.Enqueue(entry);
            
            // Keep only the last maxEntries
            while (_logEntries.Count > _maxEntries)
            {
                _logEntries.TryDequeue(out _);
            }
            
            LogEntryAdded?.Invoke(this, entry);
        }
        
        // Always update status label if this is an error with an exception
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