using StreamingPlayerNET.Services;
using StreamingPlayerNET.Common.Models;
using NLog;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private void ApplyConfiguration()
    {
        try
        {
            var config = ConfigurationService.Current;
            
            // Apply window size and position
            Width = config.WindowWidth;
            Height = config.WindowHeight;
            
            // Apply splitter distance
            if (config.ShowPlaylistsPanel)
            {
                playlistSplitContainer.Panel1Collapsed = false;
                playlistSplitContainer.SplitterDistance = config.SplitterDistance;
            }
            else
            {
                playlistSplitContainer.Panel1Collapsed = true;
            }
            
            // Apply search panel visibility
            if (config.ShowSearchPanel)
            {
                searchTabPage.Visible = true;
                showSearchMenuItem.Checked = true;
            }
            else
            {
                searchTabPage.Visible = false;
                showSearchMenuItem.Checked = false;
            }
            
            // Apply theme
            ApplyTheme(config.Theme);
            
            Logger.Debug("Configuration applied successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to apply configuration");
        }
    }

    private void ApplyTheme(AppTheme theme)
    {
        try
        {
            ThemeService.ApplyTheme(this, theme);
            Logger.Debug($"Theme {theme} applied");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to apply theme");
        }
    }

    private void SetTheme(AppTheme theme)
    {
        try
        {
            var config = ConfigurationService.Current;
            
            // Set the new theme
            config.Theme = theme;
            
            // Save configuration
            ConfigurationService.SaveConfiguration();
            
            // Apply the new theme
            ApplyTheme(config.Theme);
            
            // Update menu state
            UpdateThemeMenuState();
            
            Logger.Info($"Theme set to: {config.Theme}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to set theme");
            MessageBox.Show($"Failed to set theme: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowSettings()
    {
        try
        {
            using var settingsForm = new SettingsForm();
            if (settingsForm.ShowDialog(this) == DialogResult.OK)
            {
                // Reload configuration and apply changes
                ConfigurationService.ReloadConfiguration();
                ApplyConfiguration();
                
                // Update services with new configuration
                UpdateServicesConfiguration();
                
                Logger.Info("Settings updated and applied");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to show settings form");
            MessageBox.Show($"Failed to open settings: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateServicesConfiguration()
    {
        var config = ConfigurationService.Current;
        
        // Update music player service configuration
        if (_musicPlayerService != null)
        {
            _musicPlayerService.SetVolume(config.DefaultVolume / 100f);
        }
        
        Logger.Debug("Services configuration updated");
    }

    private void ShowHelp()
    {
        var config = ConfigurationService.Current;
        var helpText = $@"SimplePlayerNET - Help

Keyboard Shortcuts:
• {config.PlayPauseHotkey.ToString()}: Play/Pause
• {config.StopHotkey.ToString()}: Stop
• {config.PreviousTrackHotkey.ToString()}: Previous Track
• {config.NextTrackHotkey.ToString()}: Next Track
• {config.RepeatModeHotkey.ToString()}: Toggle Repeat Mode
• {config.ShuffleHotkey.ToString()}: Toggle Shuffle
• {config.VolumeUpHotkey.ToString()}: Volume Up
• {config.VolumeDownHotkey.ToString()}: Volume Down
• {config.TogglePlaylistsHotkey.ToString()}: Toggle Playlists Panel
• {config.ToggleSearchHotkey.ToString()}: Toggle Search Panel
• {config.SettingsHotkey.ToString()}: Settings
• {config.HelpHotkey.ToString()}: Help
• {config.ThemeCycleHotkey.ToString()}: Cycle Theme
• Alt+F4: Exit

Theme:
• Light Theme: Standard Windows system colors
• Dark Theme: Dark gray background with white text
• Black Theme: Pure black background with white text

Note: Disabled hotkeys are shown as ""(Disabled)"" and will not work.

Features:
• Search for music across multiple sources (YouTube, Spotify, etc.)
• Play songs and videos from any enabled source
• Control playback with buttons or keyboard
• Repeat modes: None, One, All
• Shuffle playback
• Adjust volume with slider or keyboard
• View playlists (when authenticated)
• Real-time progress tracking
• Customizable hotkeys in Settings
• Enable/disable individual hotkeys

For more information, visit the project repository.";

        MessageBox.Show(helpText, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowAbout()
    {
        var aboutText = @"SimplePlayerNET

A desktop application for playing music from streaming services.

Features:
• Search and play music from multiple sources
• Playback controls
• Repeat modes (None, One, All)
• Shuffle playback
• Volume control
• Progress tracking
• Playlist support (when authenticated)

Version: 1.0.0
Built with .NET 9.0

This application uses:
• YouTubeExplode for video metadata
• NAudio for audio playback
• NLog for logging

Note: This is a demo application for educational purposes.";

        MessageBox.Show(aboutText, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}