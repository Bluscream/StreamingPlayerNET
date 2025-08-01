# Global Hotkeys for Media Keys

## Overview

The GlobalHotkeys class provides a fallback mechanism for media key support when the Windows Media Session is not available or working properly. It uses a low-level keyboard hook to detect media key presses globally across the system.

## Features

- **Media Key Support**: Detects and handles standard media keys:
  - Play/Pause (0xB3)
  - Stop (0xB2)
  - Next Track (0xB0)
  - Previous Track (0xB1)
  - Volume Up (0xAF)
  - Volume Down (0xAE)
  - Volume Mute (0xAD)

- **Fallback Mechanism**: Works as a backup when Windows Media Session fails
- **Configurable**: Can be enabled/disabled through configuration
- **Global Detection**: Works even when the application is not in focus

## How It Works

1. **Keyboard Hook**: Uses `SetWindowsHookEx` with `WH_KEYBOARD_LL` to install a low-level keyboard hook
2. **Key Detection**: Monitors for specific virtual key codes that correspond to media keys
3. **Command Mapping**: Maps detected keys to `MediaCommand` enum values
4. **Event Handling**: Triggers the same event handlers as the Windows Media Service

## Configuration

The feature can be controlled through the configuration setting:

```csharp
[Category("Advanced")]
[DisplayName("Enable Global Hotkeys Fallback")]
[Description("Enable global hotkeys as fallback for media keys when Windows Media Session is not available")]
public bool EnableGlobalHotkeysFallback { get; set; } = true;
```

## Integration

The GlobalHotkeys service is automatically initialized during application startup if enabled:

1. **Initialization**: Created in `InitializeServicesAsync()` in `MainForm/Core.cs`
2. **Event Wiring**: Connected to the same `OnMediaCommandReceived` handler as Windows Media Service
3. **Cleanup**: Properly disposed during application shutdown

## Logging

The service provides detailed logging for troubleshooting:

- Initialization status and error codes
- Media key detection events
- Command handling results
- Disposal confirmation

## Troubleshooting

### Media Keys Not Working

1. **Check Configuration**: Ensure `EnableGlobalHotkeysFallback` is set to `true`
2. **Check Logs**: Look for initialization errors or hook failures
3. **Permissions**: Ensure the application has sufficient permissions to install keyboard hooks
4. **Conflicts**: Check if other applications are intercepting media keys

### Common Issues

- **Hook Installation Failed**: Usually indicates permission issues or system restrictions
- **Keys Not Detected**: May indicate different virtual key codes on your keyboard
- **Performance Impact**: Low-level hooks can affect system performance if not properly managed

## Technical Details

### Virtual Key Codes

The service monitors these specific virtual key codes:
- `0xB3` - VK_MEDIA_PLAY_PAUSE
- `0xB2` - VK_MEDIA_STOP
- `0xB0` - VK_MEDIA_NEXT_TRACK
- `0xB1` - VK_MEDIA_PREV_TRACK
- `0xAF` - VK_VOLUME_UP
- `0xAE` - VK_VOLUME_DOWN
- `0xAD` - VK_VOLUME_MUTE

### Windows API Usage

- `SetWindowsHookEx`: Installs the keyboard hook
- `UnhookWindowsHookEx`: Removes the keyboard hook
- `CallNextHookEx`: Passes unhandled keys to the next hook in the chain
- `GetModuleHandle`: Gets the module handle for the hook procedure

### Thread Safety

The keyboard hook callback runs on a system-wide hook thread, so all operations are performed asynchronously to avoid blocking the UI thread.

## Future Enhancements

Potential improvements for the GlobalHotkeys feature:

1. **Custom Key Mapping**: Allow users to map custom key combinations to media commands
2. **Modifier Key Support**: Support for Ctrl, Alt, Shift combinations
3. **Profile Support**: Different key mappings for different scenarios
4. **Visual Feedback**: Toast notifications when media keys are detected
5. **Statistics**: Track usage statistics for media key commands 