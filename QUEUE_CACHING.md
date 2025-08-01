# Queue Caching Feature

## Overview

The StreamingPlayerNET application now supports caching the playback queue between app restarts. This means that when you close the app and reopen it, your queue will be automatically restored with all the songs, current position, repeat mode, and shuffle state.

## How It Works

### Configuration Options

The queue caching feature can be configured through the application settings:

- **Enable Queue Caching**: Toggle to enable/disable queue caching (default: enabled)
- **Max Cached Queue Size**: Maximum number of songs to cache (default: 100)

### Cache Storage

The queue cache is stored in a JSON file located at:
```
%APPDATA%\StreamingPlayerNET\queue_cache.json
```

### What Gets Cached

The following queue state is preserved between app restarts:

- **Songs**: All songs in the queue (up to the configured maximum)
- **Current Index**: The currently playing song position
- **Repeat Mode**: Current repeat mode (None, One, All)
- **Shuffle State**: Whether shuffle is enabled
- **Last Saved**: Timestamp of when the cache was last updated

### Automatic Saving

The queue is automatically saved in the following situations:

1. **App Shutdown**: When the application is closed
2. **Queue Changes**: When songs are added, removed, or reordered
3. **Playback Changes**: When the current song index changes
4. **Settings Changes**: When repeat mode or shuffle state changes

### Automatic Loading

The queue is automatically loaded when the application starts, after all services are initialized. If a cached queue is found and queue caching is enabled, it will:

1. Clear the current empty queue
2. Restore all cached songs
3. Set the current index to the cached position
4. Restore repeat mode and shuffle state
5. Update the UI to reflect the restored queue

## Usage

### Enabling/Disabling

1. Open the application settings (Ctrl+,)
2. Navigate to the "Queue" section
3. Toggle "Enable Queue Caching" on/off
4. Adjust "Max Cached Queue Size" if needed
5. Click "Save" to apply changes

### Manual Cache Management

If you need to clear the queue cache manually, you can:

1. Close the application
2. Delete the `queue_cache.json` file from the AppData folder
3. Restart the application

The cache will be automatically recreated when you next close the app with songs in the queue.

## Technical Details

### Implementation

- **QueueCacheService**: Handles saving and loading queue state
- **QueueCacheData**: Data structure for serializing queue state
- **Event Integration**: Hooks into queue events for automatic saving
- **Configuration Integration**: Uses existing configuration system

### Events Monitored

The following queue events trigger automatic cache saving:

- `OnSongsChanged`: When songs are added, removed, or reordered
- `OnCurrentIndexChanged`: When the current song position changes
- `OnRepeatModeChanged`: When repeat mode is changed
- `OnShuffleChanged`: When shuffle state is toggled

### Error Handling

- If queue caching is disabled, save/load operations are skipped
- If cache file is corrupted or missing, the app starts with an empty queue
- All cache operations are logged for debugging purposes
- Cache failures don't prevent the app from starting normally

## Benefits

1. **Seamless Experience**: No need to rebuild your queue after each app restart
2. **Preserved State**: Maintains your exact playback position and settings
3. **Configurable**: Can be disabled if not needed
4. **Performance**: Minimal impact on app performance
5. **Reliable**: Graceful error handling ensures app stability

## Limitations

- Cache is limited to configured maximum song count
- Cache is stored locally (not synced across devices)
- Cache is cleared if the app crashes before saving
- Very large queues may impact startup time slightly 