using StreamingPlayerNET.Services;
using NLog;

namespace StreamingPlayerNET.UI;

public partial class MainForm
{
    private void ApplyConfiguration()
    {
        var config = ConfigurationService.Current;
        
        // Apply window settings
        Size = new Size(config.WindowWidth, config.WindowHeight);
        
        // Apply dark mode theme
        ApplyDarkMode(config.DarkMode);
        
        // Apply tab visibility
        ToggleTabVisibility(playlistTabPage, config.ShowPlaylistsPanel);
        ToggleTabVisibility(searchTabPage, config.ShowSearchPanel);
        
        // Update menu check states
        showPlaylistsMenuItem.Checked = config.ShowPlaylistsPanel;
        showSearchMenuItem.Checked = config.ShowSearchPanel;
        
        // Apply volume
        volumeTrackBar.Value = config.DefaultVolume;
        volumeLabel.Text = $"{config.DefaultVolume}%";
        
        // Apply default repeat and shuffle settings
        _queue.RepeatMode = config.DefaultRepeatMode;
        _queue.ShuffleEnabled = config.DefaultShuffleEnabled;
        
        // Initialize download progress bar
        downloadProgressBar.Visible = false;
        downloadProgressBar.Value = 0;
        
        // Initialize seek bar colors
        seekBar.BackColor = SystemColors.Control;
        seekBar.ForeColor = SystemColors.Highlight;
        
        Logger.Debug("Configuration applied to UI");
    }

    private void ApplyDarkMode(bool isDarkMode)
    {
        try
        {
            ThemeService.ApplyTheme(this, isDarkMode);
            Logger.Debug($"Dark mode {(isDarkMode ? "enabled" : "disabled")}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to apply dark mode theme");
        }
    }

    private void ToggleDarkMode()
    {
        try
        {
            var config = ConfigurationService.Current;
            config.DarkMode = !config.DarkMode;
            
            // Save configuration
            ConfigurationService.SaveConfiguration();
            
            // Apply the new theme
            ApplyDarkMode(config.DarkMode);
            
            // Update menu item text
            UpdateDarkModeMenuItemText();
            
            Logger.Info($"Dark mode toggled to: {(config.DarkMode ? "enabled" : "disabled")}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to toggle dark mode");
            MessageBox.Show($"Failed to toggle dark mode: {ex.Message}", "Error", 
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
• {config.DarkModeHotkey.ToString()}: Toggle Dark Mode
• Alt+F4: Exit

Theme:
• Dark Mode: Switch between light and dark themes

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