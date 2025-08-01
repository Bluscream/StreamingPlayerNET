/*using StreamingPlayerNET.Services;
using NLog;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private void SetupLogTab()
    {
        // Subscribe to log service events
        if (_logService != null)
        {
            _logService.LogEntryAdded += OnLogEntryAdded;
            _logService.StatusUpdateRequested += OnLogServiceStatusUpdateRequested;
            
            // Load existing log entries
            LoadExistingLogEntries();
        }
    }

    private void OnLogEntryAdded(object? sender, LogEntry entry)
    {
        SafeInvoke(() => AddLogEntryToListView(entry));
    }

    private void OnLogServiceStatusUpdateRequested(object? sender, string statusMessage)
    {
        SafeInvoke(() => statusLabel.Text = statusMessage);
    }

    private void AddLogEntryToListView(LogEntry entry)
    {
        var item = new ListViewItem(entry.Timestamp.ToString("HH:mm:ss.fff"));
        item.SubItems.Add(entry.Level.ToString());
        item.SubItems.Add(entry.LoggerName);
        item.SubItems.Add(entry.Message);
        
        // Color code based on log level
        if (entry.Level == NLog.LogLevel.Error)
        {
            item.BackColor = Color.LightCoral;
        }
        else if (entry.Level == NLog.LogLevel.Warn)
        {
            item.BackColor = Color.LightYellow;
        }
        else if (entry.Level == NLog.LogLevel.Info)
        {
            item.BackColor = Color.LightBlue;
        }
        else if (entry.Level == NLog.LogLevel.Debug)
        {
            item.BackColor = Color.LightGray;
        }
        else if (entry.Level == NLog.LogLevel.Trace)
        {
            item.BackColor = Color.White;
        }
        

    }

    private void LoadExistingLogEntries()
    {
        if (_logService == null) return;
        
        var entries = _logService.GetLogEntries();
        foreach (var entry in entries.Reverse()) // Show newest first
        {
            AddLogEntryToListView(entry);
        }
    }


}*/