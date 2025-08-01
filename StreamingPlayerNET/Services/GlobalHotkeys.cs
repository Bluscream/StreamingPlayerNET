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
    
    // Windows API constants for keyboard hook
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;
    
    // Media key virtual key codes
    private const int VK_MEDIA_PLAY_PAUSE = 0xB3;
    private const int VK_MEDIA_STOP = 0xB2;
    private const int VK_MEDIA_NEXT_TRACK = 0xB0;
    private const int VK_MEDIA_PREV_TRACK = 0xB1;
    private const int VK_VOLUME_UP = 0xAF;
    private const int VK_VOLUME_DOWN = 0xAE;
    private const int VK_VOLUME_MUTE = 0xAD;
    
    private bool _isInitialized = false;
    private bool _disposed = false;
    private bool _mediaKeysEnabled = true;
    private IntPtr _keyboardHookId = IntPtr.Zero;
    
    // Delegate for keyboard hook
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelKeyboardProc? _keyboardProc;
    
    public event EventHandler<MediaCommand>? MediaCommandReceived;
    
    // Windows API imports
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    
    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }
    
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
            
            // Set up keyboard hook
            _keyboardProc = KeyboardHookCallback;
            _keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle("user32.dll"), 0);
            
            if (_keyboardHookId == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                Logger.Error($"Failed to set keyboard hook. Error code: {error}");
                _mediaKeysEnabled = false;
                return;
            }
            
            _isInitialized = true;
            _mediaKeysEnabled = true;
            Logger.Info("Global Hotkeys initialized successfully with keyboard hook");
            Logger.Info("Monitoring for media keys: Play/Pause (0xB3), Stop (0xB2), Next (0xB0), Previous (0xB1), Volume Up (0xAF), Volume Down (0xAE)");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize Global Hotkeys");
            _mediaKeysEnabled = false;
        }
    }
    
    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _mediaKeysEnabled)
        {
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            var keyCode = hookStruct.vkCode;
            var message = wParam.ToInt32();
            
            // Only process key down events for media keys
            if (message == WM_KEYDOWN || message == WM_SYSKEYDOWN)
            {
                // Log all media key codes for debugging
                if (keyCode == VK_MEDIA_PLAY_PAUSE || keyCode == VK_MEDIA_STOP || 
                    keyCode == VK_MEDIA_NEXT_TRACK || keyCode == VK_MEDIA_PREV_TRACK ||
                    keyCode == VK_VOLUME_UP || keyCode == VK_VOLUME_DOWN || keyCode == VK_VOLUME_MUTE)
                {
                    Logger.Debug($"Media key detected: 0x{keyCode:X} (Message: {message})");
                }
                
                var command = keyCode switch
                {
                    VK_MEDIA_PLAY_PAUSE => MediaCommand.Play,
                    VK_MEDIA_STOP => MediaCommand.Stop,
                    VK_MEDIA_NEXT_TRACK => MediaCommand.Next,
                    VK_MEDIA_PREV_TRACK => MediaCommand.Previous,
                    VK_VOLUME_UP => MediaCommand.VolumeUp,
                    VK_VOLUME_DOWN => MediaCommand.VolumeDown,
                    _ => (MediaCommand?)null
                };
                
                if (command.HasValue)
                {
                    Logger.Info($"Global hotkey detected: {command.Value} (KeyCode: 0x{keyCode:X})");
                    
                    // Handle the command asynchronously
                    Task.Run(() => HandleMediaCommand(command.Value));
                    
                    // Notify any listeners
                    MediaCommandReceived?.Invoke(this, command.Value);
                    
                    // Return 1 to indicate we handled the key
                    return (IntPtr)1;
                }
            }
        }
        
        // Call the next hook
        return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
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
                    Logger.Info("Global hotkey: Stopping playback");
                    _musicPlayerService.Stop();
                    break;
                    
                case MediaCommand.Next:
                    Logger.Info("Global hotkey: Playing next song");
                    Task.Run(async () => await _musicPlayerService.PlayNextSongAsync());
                    break;
                    
                case MediaCommand.Previous:
                    Logger.Info("Global hotkey: Playing previous song");
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
            InitializeGlobalHotkeys();
        }
    }
    
    public void DisableMediaKeys()
    {
        if (_mediaKeysEnabled && !_disposed)
        {
            try
            {
                if (_keyboardHookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_keyboardHookId);
                    _keyboardHookId = IntPtr.Zero;
                }
                
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