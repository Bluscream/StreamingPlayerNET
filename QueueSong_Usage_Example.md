# QueueSong Usage Examples

The `QueueSong` class extends the base `Song` class with additional functionality for tracking playback position and queue-specific information.

## Basic Usage

### Creating a QueueSong from a regular Song

```csharp
// Create a regular song
var song = new Song
{
    Id = "video123",
    Title = "Example Song",
    Artist = "Example Artist",
    Duration = TimeSpan.FromMinutes(3, 30)
};

// Convert to QueueSong
var queueSong = QueueSong.FromSong(song);
```

### Adding songs to the queue

```csharp
var queue = new Queue();

// Add a regular song (automatically converted to QueueSong)
queue.AddSong(song);

// Add a QueueSong directly
queue.AddSong(queueSong);
```

## Position Tracking

### Saving and restoring playback position

```csharp
// When playback is interrupted (e.g., app closing, song switching)
if (queue.CurrentQueueSong != null)
{
    queue.SaveCurrentSongPosition();
}

// When resuming playback
if (queue.CurrentQueueSong != null)
{
    queue.RestoreCurrentSongPosition();
}
```

### Manual position management

```csharp
var queueSong = new QueueSong
{
    Title = "My Song",
    Artist = "My Artist"
};

// Start playing
queueSong.RecordPlaybackStart();

// Later, when paused
queueSong.SaveCurrentPosition();

// When resuming
queueSong.RestorePosition();
```

## Using with MusicPlayerService

```csharp
var musicPlayer = new MusicPlayerService(/* dependencies */);

// Play a QueueSong with automatic position restoration
await musicPlayer.PlayQueueSongAsync(queueSong);

// The service automatically handles:
// - Recording playback start
// - Saving position on pause/stop
// - Restoring position when resuming
```

## Queue Management

```csharp
var queue = new Queue();

// Add songs (automatically converted to QueueSong)
queue.AddSong(song1);
queue.AddSong(song2);

// Access queue-specific properties
var currentQueueSong = queue.CurrentQueueSong;
if (currentQueueSong != null)
{
    Console.WriteLine($"Time in queue: {currentQueueSong.GetTimeInQueue()}");
    Console.WriteLine($"Total play time: {currentQueueSong.GetTotalPlayTime()}");
    Console.WriteLine($"Saved position: {currentQueueSong.SavedPosition}");
}
```

## Persistence

The `QueueCacheService` automatically handles saving and loading `QueueSong` instances with their position data:

```csharp
// Save queue (includes QueueSong position data)
QueueCacheService.SaveQueue(queue);

// Load queue (restores QueueSong position data)
var queueCache = QueueCacheService.LoadQueue();
if (queueCache != null)
{
    // Songs are automatically converted to QueueSong instances
    foreach (var song in queueCache.Songs)
    {
        queue.AddSong(song);
    }
}
```

## Key Features

- **Position Persistence**: Automatically saves playback position when stopped mid-track
- **Playback Tracking**: Records total play time and queue time
- **Seamless Integration**: Works with existing `Song` objects through automatic conversion
- **Queue-Specific Properties**: Additional metadata for queue management
- **Automatic Serialization**: Position data is saved with queue cache

## Properties

- `CurrentPosition`: The current playback position (QueueSong-specific)
- `State`: The current playback state (QueueSong-specific)
- `SavedPosition`: The position where playback was last stopped
- `WasPlaying`: Whether the song was playing when paused/stopped
- `AddedToQueueAt`: Timestamp when added to queue
- `StartedPlayingAt`: Timestamp when playback started
- `PausedAt`: Timestamp when paused/stopped
- `TotalPlayTime`: Cumulative time the song has been played

## Methods

- `SaveCurrentPosition()`: Saves current playback position
- `RestorePosition()`: Restores saved position for resumption
- `RecordPlaybackStart()`: Records start of playback
- `RecordPlaybackPause()`: Records pause/stop of playback
- `GetTimeInQueue()`: Gets total time in queue
- `GetTotalPlayTime()`: Gets total active play time
- `FromSong(Song)`: Static method to convert Song to QueueSong 