using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using StreamingPlayerNET.Source.Base.Interfaces;

namespace StreamingPlayerNET.Source.Base;

/// <summary>
/// Base class for source-specific settings that provides common functionality
/// </summary>
public abstract class BaseSourceSettings : ISourceSettings
{
    private readonly string _sourceName;
    
    protected BaseSourceSettings(string sourceName)
    {
        _sourceName = sourceName ?? throw new ArgumentNullException(nameof(sourceName));
    }
    
    [Category("General")]
    [DisplayName("Enabled")]
    [Description("Whether this source is enabled")]
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Get the settings file path for this source
    /// </summary>
    protected virtual string SettingsFilePath
    {
        get
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsDir = Path.Combine(appDataPath, "StreamingPlayerNET", "Sources");
            Directory.CreateDirectory(settingsDir);
            return Path.Combine(settingsDir, $"{_sourceName}.json");
        }
    }
    
    public virtual PropertyInfo[] GetSettingsProperties()
    {
        return GetType()
            .GetProperties()
            .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            .ToArray();
    }
    
    public virtual async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(this, GetType(), new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save settings for {_sourceName}", ex);
        }
    }
    
    public virtual async Task LoadAsync()
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
            {
                // Don't try to reset to defaults during initialization - just use current values
                return;
            }
            
            var json = await File.ReadAllTextAsync(SettingsFilePath);
            var loadedSettings = JsonSerializer.Deserialize(json, GetType());
            
            if (loadedSettings != null)
            {
                // Copy properties from loaded settings to this instance
                foreach (var property in GetSettingsProperties())
                {
                    if (property.CanRead && property.CanWrite)
                    {
                        var value = property.GetValue(loadedSettings);
                        property.SetValue(this, value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Don't throw during initialization - just log and continue with defaults
            // This prevents the application from hanging due to file system issues
            Console.WriteLine($"Failed to load settings for {_sourceName}: {ex.Message}");
        }
    }
    
    public virtual async Task ResetToDefaultsAsync()
    {
        try
        {
            // Create a new instance of the same type to get default values
            var defaultInstance = Activator.CreateInstance(GetType());
            
            if (defaultInstance != null)
            {
                // Copy default values to this instance
                foreach (var property in GetSettingsProperties())
                {
                    if (property.CanRead && property.CanWrite)
                    {
                        var value = property.GetValue(defaultInstance);
                        property.SetValue(this, value);
                    }
                }
            }
            
            await SaveAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to reset settings for {_sourceName}", ex);
        }
    }
} 