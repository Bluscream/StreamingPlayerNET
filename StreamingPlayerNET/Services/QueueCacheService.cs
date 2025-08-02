using System.Text.Json;
using StreamingPlayerNET.Common.Models;
using NLog;

namespace StreamingPlayerNET.Services;

public class QueueCacheService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string QueueCacheFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "StreamingPlayerNET",
        "queue_cache.json");
    
    // Throttling mechanism to prevent too frequent saves
    private static Timer? _saveTimer;
    private static Queue? _pendingQueue;
    private static readonly object _saveLock = new object();
    private static readonly TimeSpan SaveThrottleInterval = TimeSpan.FromMilliseconds(500); // 500ms throttle

    public static void SaveQueue(Queue queue)
    {
        lock (_saveLock)
        {
            // Store the queue for later saving
            _pendingQueue = queue;
            
            // Reset the timer
            _saveTimer?.Dispose();
            _saveTimer = new Timer(_ => PerformSave(), null, SaveThrottleInterval, Timeout.InfiniteTimeSpan);
        }
    }
    
    private static void PerformSave()
    {
        lock (_saveLock)
        {
            if (_pendingQueue == null)
            {
                return;
            }
            
            var queueToSave = _pendingQueue;
            _pendingQueue = null;
            
            try
            {
                var config = ConfigurationService.Current;
                if (!config.EnableQueueCaching)
                {
                    Logger.Debug("Queue caching is disabled, skipping save");
                    return;
                }

                var configDir = Path.GetDirectoryName(QueueCacheFilePath);
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // Create queue cache data
                var queueCache = new QueueCacheData
                {
                    Songs = queueToSave.Songs.Take(config.MaxCachedQueueSize)
                        .Select(s => s is QueueSong ? (QueueSong)s : QueueSong.FromSong(s))
                        .ToList(),
                    CurrentIndex = queueToSave.CurrentIndex,
                    RepeatMode = queueToSave.RepeatMode,
                    ShuffleEnabled = queueToSave.ShuffleEnabled,
                    LastSaved = DateTime.UtcNow
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(queueCache, options);
                File.WriteAllText(QueueCacheFilePath, json);
                
                Logger.Debug("Queue cache saved successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save queue cache");
            }
        }
    }
    
    // Force immediate save (for shutdown scenarios)
    public static void SaveQueueImmediate(Queue queue)
    {
        lock (_saveLock)
        {
            // Cancel any pending save
            _saveTimer?.Dispose();
            _saveTimer = null;
            _pendingQueue = null;
            
            try
            {
                var config = ConfigurationService.Current;
                if (!config.EnableQueueCaching)
                {
                    Logger.Debug("Queue caching is disabled, skipping immediate save");
                    return;
                }

                var configDir = Path.GetDirectoryName(QueueCacheFilePath);
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // Create queue cache data
                var queueCache = new QueueCacheData
                {
                    Songs = queue.Songs.Take(config.MaxCachedQueueSize)
                        .Select(s => s is QueueSong ? (QueueSong)s : QueueSong.FromSong(s))
                        .ToList(),
                    CurrentIndex = queue.CurrentIndex,
                    RepeatMode = queue.RepeatMode,
                    ShuffleEnabled = queue.ShuffleEnabled,
                    LastSaved = DateTime.UtcNow
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(queueCache, options);
                File.WriteAllText(QueueCacheFilePath, json);
                
                Logger.Debug("Queue cache saved immediately");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save queue cache immediately");
            }
        }
    }

    public static QueueCacheData? LoadQueue()
    {
        try
        {
            var config = ConfigurationService.Current;
            if (!config.EnableQueueCaching)
            {
                Logger.Debug("Queue caching is disabled, skipping load");
                return null;
            }

            if (!File.Exists(QueueCacheFilePath))
            {
                Logger.Debug("No queue cache file found");
                return null;
            }

            var json = File.ReadAllText(QueueCacheFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var queueCache = JsonSerializer.Deserialize<QueueCacheData>(json, options);
            if (queueCache != null)
            {
                Logger.Info($"Queue cache loaded successfully with {queueCache.Songs.Count} songs");
                return queueCache;
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Failed to load queue cache, using empty queue");
        }

        return null;
    }

    public static void ClearQueueCache()
    {
        try
        {
            if (File.Exists(QueueCacheFilePath))
            {
                File.Delete(QueueCacheFilePath);
                Logger.Info("Queue cache cleared successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to clear queue cache");
        }
    }
}

public class QueueCacheData
{
    public List<QueueSong> Songs { get; set; } = new();
    public int CurrentIndex { get; set; } = -1;
    public RepeatMode RepeatMode { get; set; } = RepeatMode.None;
    public bool ShuffleEnabled { get; set; } = false;
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
} 