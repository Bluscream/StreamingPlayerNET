using System.ComponentModel;
using System.Reflection;

namespace StreamingPlayerNET.Source.Base.Interfaces;

/// <summary>
/// Interface for source-specific settings that can be displayed in the settings window
/// </summary>
public interface ISourceSettings
{
    /// <summary>
    /// Whether these settings are currently enabled/available
    /// </summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// Get all properties that should be displayed in the settings UI
    /// </summary>
    /// <returns>Array of property info objects with their display attributes</returns>
    PropertyInfo[] GetSettingsProperties();
    
    /// <summary>
    /// Save the current settings
    /// </summary>
    Task SaveAsync();
    
    /// <summary>
    /// Load settings from storage
    /// </summary>
    Task LoadAsync();
    
    /// <summary>
    /// Reset settings to defaults
    /// </summary>
    Task ResetToDefaultsAsync();
} 