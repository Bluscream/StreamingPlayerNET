using System.Runtime.InteropServices;
using System.Windows.Forms;
using NLog;
using StreamingPlayerNET.Common.Models;

namespace StreamingPlayerNET.Services;

public class GlobalHotkeys : IDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly MusicPlayerService _musicPlayerService;
    private readonly ConfigurationService _configService;
    private readonly IntPtr _windowHandle;
    
    // Windows API constants for global hotkeys
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_ALT = 0x0001;
    private const int MOD_CONTROL = 0x0002;
    private const int MOD_SHIFT = 0x0004;
    private const int MOD_WIN = 0x0008;
    
    // Media key virtual key codes
    private const int VK_MEDIA_PLAY_PAUSE = 0xB3;
    private const int VK_MEDIA_STOP = 0xB2;
    private const int VK_MEDIA_NEXT_TRACK = 0xB0;
    private const int VK_MEDIA_PREV_TRACK = 0xB1;
    private const int VK_VOLUME_UP = 0xAF;
    private const int VK_VOLUME_DOWN = 0xAE;
    private const int VK_VOLUME_MUTE = 0xAD;
    
    // Hotkey IDs for registration
    private const int HOTKEY_MEDIA_PLAY_PAUSE = 1;
    private const int HOTKEY_MEDIA_STOP = 2;
    private const int HOTKEY_MEDIA_NEXT = 3;
    private const int HOTKEY_MEDIA_PREV = 4;
    private const int HOTKEY_VOLUME_UP = 5;
    private const int HOTKEY_VOLUME_DOWN = 6;
    private const int HOTKEY_VOLUME_MUTE = 7;
    
    private bool _isInitialized = false;
    private bool _disposed = false;
    private bool _mediaKeysEnabled = true;
    
    public event EventHandler<MediaCommand>? MediaCommandReceived;
    
    // Windows API imports
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
    
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    
    public GlobalHotkeys(MusicPlayerService musicPlayerService, ConfigurationService configService, IntPtr windowHandle)
    {
        _musicPlayerService = musicPlayerService;
        _configService = configService;
        _windowHandle = windowHandle;
        
        InitializeGlobalHotkeys();
    }
    
    private void InitializeGlobalHotkeys()
    {
        try
        {
            if (_isInitialized || _disposed) return;
            
            Logger.Info("Initializing Global Hotkeys for media keys fallback");
            
            // Register media key hotkeys
            RegisterMediaKeyHotkeys();
            
            _isInitialized = true;
            Logger.Info("Global Hotkeys initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize Global Hotkeys");
        }
    }
    
    private void RegisterMediaKeyHotkeys()
    {
        try
        {
            // Register media keys as global hotkeys
            var success = true;
            
            success &= RegisterHotKey(_windowHandle, HOTKEY_MEDIA_PLAY_PAUSE, 0, VK_MEDIA_PLAY_PAUSE);
            success &= RegisterHotKey(_windowHandle, HOTKEY_MEDIA_STOP, 0, VK_MEDIA_STOP);
            success &= RegisterHotKey(_windowHandle, HOTKEY_MEDIA_NEXT, 0, VK_MEDIA_NEXT_TRACK);
            success &= RegisterHotKey(_windowHandle, HOTKEY_MEDIA_PREV, 0, VK_MEDIA_PREV_TRACK);
            success &= RegisterHotKey(_windowHandle, HOTKEY_VOLUME_UP, 0, VK_VOLUME_UP);
            success &= RegisterHotKey(_windowHandle, HOTKEY_VOLUME_DOWN, 0, VK_VOLUME_DOWN);
            success &= RegisterHotKey(_windowHandle, HOTKEY_VOLUME_MUTE, 0, VK_VOLUME_MUTE);
            
            if (success)
            {
                Logger.Info("All media key hotkeys registered successfully");
                _mediaKeysEnabled = true;
            }
            else
            {
                Logger.Warn("Some media key hotkeys failed to register");
                _mediaKeysEnabled = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error registering media key hotkeys");
            _mediaKeysEnabled = false;
        }
    }
    
    public bool ProcessHotkeyMessage(Message m)
    {
        if (m.Msg == WM_HOTKEY && _mediaKeysEnabled)
        {
            try
            {
                var hotkeyId = m.WParam.ToInt32();
                var command = hotkeyId switch
                {
                    HOTKEY_MEDIA_PLAY_PAUSE => MediaCommand.Play,
                    HOTKEY_MEDIA_STOP => MediaCommand.Stop,
                    HOTKEY_MEDIA_NEXT => MediaCommand.Next,
                    HOTKEY_MEDIA_PREV => MediaCommand.Previous,
                    HOTKEY_VOLUME_UP => MediaCommand.VolumeUp,
                    HOTKEY_VOLUME_DOWN => MediaCommand.VolumeDown,
                    _ => (MediaCommand?)null
                };
                
                if (command.HasValue)
                {
                    Logger.Debug($"Global hotkey received: {command.Value}");
                    
                    // Handle the command asynchronously
                    Task.Run(() => HandleMediaCommand(command.Value));
                    
                    // Notify any listeners
                    MediaCommandReceived?.Invoke(this, command.Value);
                    
                    return true; // Message handled
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error handling global hotkey message");
            }
        }
        
        return false; // Message not handled
    }
    
    public void HandleMediaCommand(MediaCommand command)
    {
        try
        {
            switch (command)
            {
                case MediaCommand.Play:
                    // Toggle play/pause based on current state
                    if (_musicPlayerService.IsPlaying)
                    {
                        Logger.Info("Global hotkey: Pausing playback");
                        _musicPlayerService.Pause();
                    }
                    else if (_musicPlayerService.IsPaused)
                    {
                        Logger.Info("Global hotkey: Resuming paused playback");
                        _musicPlayerService.Resume();
                    }
                    else
                    {
                        Logger.Debug("Global hotkey: Play command received but no song is loaded");
                    }
                    break;
                    
                case MediaCommand.Stop:
                    _musicPlayerService.Stop();
                    break;
                    
                case MediaCommand.Next:
                    Task.Run(async () => await _musicPlayerService.PlayNextSongAsync());
                    break;
                    
                case MediaCommand.Previous:
                    Task.Run(async () => await _musicPlayerService.PlayPreviousSongAsync());
                    break;
                    
                case MediaCommand.VolumeUp:
                    // Volume controls are typically handled by the system
                    // But we can provide a fallback if needed
                    Logger.Debug("Volume up command received (handled by system)");
                    break;
                    
                case MediaCommand.VolumeDown:
                    // Volume controls are typically handled by the system
                    // But we can provide a fallback if needed
                    Logger.Debug("Volume down command received (handled by system)");
                    break;
                    
                default:
                    Logger.Warn($"Unhandled media command: {command}");
                    break;
            }
            
            Logger.Debug($"Handled global hotkey media command: {command}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to handle global hotkey media command: {command}");
        }
    }
    
    public bool IsMediaKeysEnabled => _mediaKeysEnabled;
    
    public void EnableMediaKeys()
    {
        if (!_mediaKeysEnabled && !_disposed)
        {
            RegisterMediaKeyHotkeys();
        }
    }
    
    public void DisableMediaKeys()
    {
        if (_mediaKeysEnabled && !_disposed)
        {
            try
            {
                UnregisterHotKey(_windowHandle, HOTKEY_MEDIA_PLAY_PAUSE);
                UnregisterHotKey(_windowHandle, HOTKEY_MEDIA_STOP);
                UnregisterHotKey(_windowHandle, HOTKEY_MEDIA_NEXT);
                UnregisterHotKey(_windowHandle, HOTKEY_MEDIA_PREV);
                UnregisterHotKey(_windowHandle, HOTKEY_VOLUME_UP);
                UnregisterHotKey(_windowHandle, HOTKEY_VOLUME_DOWN);
                UnregisterHotKey(_windowHandle, HOTKEY_VOLUME_MUTE);
                
                _mediaKeysEnabled = false;
                Logger.Info("Media keys disabled");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error disabling media keys");
            }
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        try
        {
            DisableMediaKeys();
            _disposed = true;
            Logger.Info("Global Hotkeys disposed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error disposing Global Hotkeys");
        }
    }
} 