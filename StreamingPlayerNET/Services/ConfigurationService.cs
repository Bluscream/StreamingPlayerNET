using System.Text.Json;
using StreamingPlayerNET.Common.Models;
using NLog;

namespace StreamingPlayerNET.Services;

public class ConfigurationService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "StreamingPlayerNET",
        "config.json");

    private static Configuration? _currentConfiguration;
    private static readonly object _lock = new object();

    public static Configuration Current
    {
        get
        {
            if (_currentConfiguration == null)
            {
                lock (_lock)
                {
                    _currentConfiguration ??= LoadConfiguration();
                }
            }
            return _currentConfiguration;
        }
    }

    public static void SaveConfiguration()
    {
        try
        {
            var configDir = Path.GetDirectoryName(ConfigFilePath);
            if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(Current, options);
            File.WriteAllText(ConfigFilePath, json);
            
            Logger.Info("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save configuration");
            throw;
        }
    }

    public static void ReloadConfiguration()
    {
        lock (_lock)
        {
            _currentConfiguration = LoadConfiguration();
        }
    }

    private static Configuration LoadConfiguration()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var config = JsonSerializer.Deserialize<Configuration>(json, options);
                if (config != null)
                {
                    Logger.Info("Configuration loaded successfully");
                    return config;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Failed to load configuration, using defaults");
        }

        Logger.Info("Using default configuration");
        return new Configuration();
    }

    public static void ResetToDefaults()
    {
        lock (_lock)
        {
            _currentConfiguration = new Configuration();
            SaveConfiguration();
        }
    }
} 