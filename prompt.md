# StreamingPlayerNET - Complete Project Recreation Prompt

Create a fully functional .NET 9.0 WinForms music streaming application with the following specifications:

## Project Overview

**StreamingPlayerNET** is a multi-source music streaming player that allows users to search and play music from multiple streaming services through a unified interface. The application features a modern architecture with plugin-based source providers, comprehensive theming, global hotkeys, and a professional UI.

## Solution Structure

Create a .NET 9.0 solution with the following projects:

### 1. StreamingPlayerNET (Main Application)
- **Target Framework**: `net9.0-windows10.0.17763.0`
- **Type**: WinForms Application
- **Dependencies**: 
  - NAudio (2.2.1)
  - NLog (6.0.2)
  - System.Text.Json (9.0.7)
  - Microsoft.Toolkit.Uwp.Notifications (7.1.3)
  - Humanizer.Core (2.14.1)

### 2. StreamingPlayerNET.Common (Shared Models)
- **Target Framework**: `net9.0`
- **Type**: Class Library
- **Dependencies**:
  - Humanizer.Core (2.14.1)
  - NLog (5.2.8)
  - System.Text.Json (9.0.7)

### 3. StreamingPlayerNET.Source.Base (Base Interfaces)
- **Target Framework**: `net9.0`
- **Type**: Class Library

### 4. StreamingPlayerNET.Source.YoutubeDL (YouTube Source)
- **Target Framework**: `net9.0`
- **Type**: Class Library

### 5. StreamingPlayerNET.Source.Spotify (Spotify Source)
- **Target Framework**: `net9.0`
- **Type**: Class Library

### 6. StreamingPlayerNET.Source.YouTubeMusic (YouTube Music Source)
- **Target Framework**: `net9.0`
- **Type**: Class Library

## Core Architecture

### Multi-Source Plugin System

Implement a plugin-based architecture where each streaming service is a separate source provider:

#### Base Interfaces (StreamingPlayerNET.Source.Base)

**ISourceProvider Interface:**
```csharp
public interface ISourceProvider
{
    string Name { get; }
    string ShortName { get; }
    string Description { get; }
    string Version { get; }
    bool IsAvailable { get; }
    ISearchService SearchService { get; }
    IDownloadService DownloadService { get; }
    IMetadataService MetadataService { get; }
    IPlaylistService PlaylistService { get; }
    ISourceSettings? Settings { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task DisposeAsync();
}
```

**Service Interfaces:**
- `ISearchService` - Search for songs and playlists
- `IDownloadService` - Download audio streams
- `IMetadataService` - Get song metadata
- `IPlaylistService` - Handle playlists
- `ISourceSettings` - Source-specific configuration

**BaseSourceProvider Abstract Class:**
- Provides common functionality for all source providers
- Handles initialization and disposal lifecycle
- Implements ISourceProvider interface

### Source Manager

Create a `SourceManager` singleton that:
- Registers and manages all source providers
- Provides unified access to enabled sources
- Handles initialization and disposal of all providers
- Offers methods to get providers by name or filter by settings

### Multi-Source Services

**MultiSourceSearchService:**
- Aggregates search results from all enabled sources
- Distributes search load across providers
- Adds source attribution to results
- Handles errors gracefully with fallback

**MultiSourceDownloadService:**
- Routes downloads to appropriate source providers
- Handles concurrent downloads with configurable limits
- Provides progress tracking and cancellation support

**MultiSourceMetadataService:**
- Retrieves metadata from appropriate sources
- Caches metadata to improve performance
- Handles missing metadata gracefully

## Data Models (StreamingPlayerNET.Common)

### Core Models

**Song Class:**
```csharp
public class Song
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string ChannelTitle { get; set; } = string.Empty;
    public string? Album { get; set; }
    public string? PlaylistName { get; set; }
    public string? Url { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
    public DateTime? UploadDate { get; set; }
    public long? ViewCount { get; set; }
    public long? LikeCount { get; set; }
    public string? Source { get; set; }
    
    // Playback state
    public PlaybackState State { get; set; } = PlaybackState.Stopped;
    public TimeSpan CurrentPosition { get; set; }
    public float Volume { get; set; } = 1.0f;
    
    // Performance tracking
    public Stopwatch? SongStopwatch { get; private set; }
    public AudioStreamInfo? SelectedStream { get; set; }
    
    public void StartSongTimer() { /* implementation */ }
    public void StopSongTimer() { /* implementation */ }
    public TimeSpan? GetCurrentSongTime() { /* implementation */ }
}
```

**QueueSong Class (inherits from Song):**
- Adds queue-specific properties like position, add time, play count
- Tracks playback statistics and timing

**Playlist Class:**
- Contains playlist metadata and list of songs
- Supports source attribution

**Queue Class:**
- Manages the current playback queue
- Provides add, remove, reorder functionality
- Implements event notifications for UI updates

**Configuration Class:**
- Comprehensive settings with categories:
  - Download settings (timeouts, concurrent limits, file thresholds)
  - Audio settings (volume, buffer size, quality preferences)
  - Playback settings (repeat modes, shuffle)
  - Search settings (result limits, timeouts)
  - UI settings (window size, panel visibility, themes)
  - Hotkey settings (all keyboard shortcuts)
  - Logging settings (levels, file management)
  - Advanced settings (caching, performance)

### Enums

**PlaybackState:**
- Stopped, Playing, Paused, Loading

**AudioQuality:**
- Low (128k), Medium (192k), High (256k), VeryHigh (320k)

**AppTheme:**
- Light, Dark, Black

**RepeatMode:**
- None, One, All

**LogLevel:**
- Trace, Debug, Info, Warning, Error, Fatal

## Core Services (StreamingPlayerNET)

### Music Player Service

**MusicPlayerService:**
- Orchestrates all playback operations
- Manages current song and playlist state
- Handles play, pause, stop, next, previous
- Integrates with search, download, and playback services
- Provides volume control and position seeking
- Manages queue playback and repeat/shuffle modes

### Audio Playback Service

**NAudioPlaybackService:**
- Uses NAudio for professional audio handling
- Supports streaming from URLs and local files
- Implements buffering and caching
- Provides real-time position and volume control
- Handles audio format detection and conversion
- Integrates with download service for streaming

### Configuration Service

**ConfigurationService:**
- Manages application settings persistence
- Uses JSON serialization for configuration files
- Provides singleton access to current configuration
- Handles configuration validation and defaults
- Supports hot-reloading of settings

### Theme Service

**ThemeService:**
- Complete theming system with Light, Dark, and Black themes
- Recursively applies themes to all UI controls
- Custom color schemes for each theme
- Specialized theming for different control types
- Custom tool strip renderer for menus
- Supports dynamic theme switching

### Global Hotkeys Service

**GlobalHotkeys:**
- Windows API integration for global keyboard hooks
- Media key support (play/pause, stop, next, previous, volume)
- Fallback mechanism for systems without media key support
- Configurable hotkey bindings
- Integration with Windows Media Session

### Queue Cache Service

**QueueCacheService:**
- Persists queue state between application sessions
- Automatic saving and loading of queue
- Handles queue serialization and deserialization
- Maintains playback position and statistics

### Toast Notification Service

**ToastNotificationService:**
- Windows toast notifications for playback events
- Song change notifications with metadata
- Error notifications for failed operations
- Uses Microsoft.Toolkit.Uwp.Notifications

### Windows Media Service

**WindowsMediaService:**
- Integration with Windows Media Session
- Handles media commands from system
- Provides media information to Windows
- Supports media key fallback

## User Interface (StreamingPlayerNET.UI)

### Main Form

**MainForm:**
- Modern WinForms interface with professional styling
- Tabbed interface for Search, Queue, Playlists, Downloads, Logs
- Collapsible panels for search and playlists
- Real-time progress tracking and time display
- Volume control with visual feedback
- Status bar with current song information

### UI Organization

Split the MainForm into logical partial classes:

**Core.cs:**
- Main form initialization and setup
- Service initialization and dependency injection
- Form lifecycle management

**EventHandlers.cs:**
- All UI event handlers
- Keyboard shortcuts and hotkeys
- Mouse interactions and context menus
- Menu item event handlers

**PlaybackControl.cs:**
- Playback control logic
- Queue management operations
- Repeat and shuffle functionality
- Volume and position control

**UIUpdates.cs:**
- UI update methods
- ListView population and updates
- Progress bar updates
- Status bar updates

**Configuration.cs:**
- Configuration application to UI
- Theme management
- Settings form integration
- Help and about dialogs

**WindowManagement.cs:**
- Window size and position management
- Panel visibility controls
- Splitter distance management
- Form state persistence

**ContextMenus.cs:**
- Right-click context menus
- Search result context menu
- Queue item context menu
- Playlist context menu

**Downloads.cs:**
- Download management UI
- Progress tracking display
- Download queue management

**Logs.cs:**
- Log display and management
- Log level filtering
- Log file access

**LoggingAndStatus.cs:**
- Status bar management
- Log message display
- Error reporting

### Settings Form

**SettingsForm:**
- Comprehensive settings interface
- Categorized settings with descriptions
- Real-time validation and preview
- Hotkey configuration interface
- Theme preview and selection

## Source Provider Implementations

### YouTube Source (StreamingPlayerNET.Source.YoutubeDL)

**YouTubeSourceProvider:**
- Implements ISourceProvider for YouTube
- Uses yt-dlp for video extraction
- Supports YouTube search and metadata
- Handles YouTube playlists

**Services:**
- YouTubeSearchService - YouTube search implementation
- YouTubeDownloadService - yt-dlp integration
- YouTubeMetadataService - YouTube metadata extraction
- YouTubePlaylistService - YouTube playlist handling

### Spotify Source (StreamingPlayerNET.Source.Spotify)

**SpotifySourceProvider:**
- Implements ISourceProvider for Spotify
- Requires Spotify API credentials
- Supports Spotify search and playlists

**Services:**
- SpotifySearchService - Spotify API search
- SpotifyDownloadService - Spotify stream handling
- SpotifyMetadataService - Spotify metadata
- SpotifyPlaylistService - Spotify playlists
- SpotifyAuthService - OAuth authentication

### YouTube Music Source (StreamingPlayerNET.Source.YouTubeMusic)

**YouTubeMusicSourceProvider:**
- Implements ISourceProvider for YouTube Music
- Specialized for music content
- Enhanced metadata for music tracks

**Services:**
- YouTubeMusicSearchService - YouTube Music search
- YouTubeMusicDownloadService - Music stream handling
- YouTubeMusicMetadataService - Music metadata
- YouTubeMusicPlaylistService - Music playlists

## Configuration and Settings

### Comprehensive Configuration System

The application should have 50+ configurable options across categories:

**Download Settings:**
- Large file threshold (MB)
- Partial download size (MB)
- Download timeout (seconds)
- Max concurrent downloads

**Audio Settings:**
- Default volume (0-100)
- Audio buffer size (KB)
- Preferred audio quality

**Playback Settings:**
- Default repeat mode
- Default shuffle state

**Search Settings:**
- Max search results
- Search timeout (seconds)

**UI Settings:**
- Window width/height
- Splitter distance
- Show playlists panel
- Show search panel
- Theme selection

**Hotkey Settings:**
- All keyboard shortcuts configurable
- Media key support
- Global hotkey enable/disable

**Logging Settings:**
- Log level
- Log file size limit
- Log file count limit

**Advanced Settings:**
- Cache settings
- Performance options
- Debug options

## Key Features

### 1. Multi-Source Search
- Search across all enabled sources simultaneously
- Unified results with source attribution
- Configurable result limits per source
- Progressive loading for responsive UI

### 2. Advanced Playback
- Full player controls (play, pause, stop, next, previous)
- Volume control with visual feedback
- Progress tracking with seek functionality
- Multiple repeat modes (None, One, All)
- Shuffle functionality
- Queue management with drag-and-drop

### 3. Queue Management
- Add songs from search results
- Remove songs from queue
- Reorder queue items
- Queue persistence between sessions
- Queue statistics and playback history

### 4. Theme System
- Light, Dark, and Black themes
- Complete UI theming
- Dynamic theme switching
- Theme persistence
- Keyboard shortcut for theme toggle

### 5. Global Hotkeys
- Media key support
- Configurable keyboard shortcuts
- Windows Media Session integration
- Fallback for systems without media keys

### 6. Comprehensive Logging
- NLog integration
- Multiple log levels
- File-based logging
- Log viewer in UI
- Performance tracking

### 7. Error Handling
- Graceful degradation when sources fail
- User-friendly error messages
- Automatic retry logic
- Comprehensive logging for debugging

### 8. Performance Optimizations
- Lazy loading of search results
- Background operations
- Smart caching system
- Memory-efficient streaming
- Concurrent download management

## Build and Deployment

### Project Files

**Main Project (StreamingPlayerNET.csproj):**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows10.0.17763.0</TargetFramework>
    <SupportedOSPlatformVersion>10.0.17763.0</SupportedOSPlatformVersion>
    <Nullable>enable</Nullable>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <NoWarn>$(NoWarn);CS8602;CS8600;CS8601;CS8605;CS1998;CS0067</NoWarn>
    <Version>1.0.0.5</Version>
  </PropertyGroup>
  <!-- Project references and package references -->
</Project>
```

### Configuration Files

**NLog.config:**
- Configure logging with file and console targets
- Set appropriate log levels
- Include performance tracking

**App Configuration:**
- JSON-based configuration storage
- Automatic configuration file creation
- Default settings provision

## Development Guidelines

### Code Organization
- Use partial classes for large forms
- Implement proper separation of concerns
- Use async/await throughout for responsiveness
- Implement comprehensive error handling
- Use NLog for all logging

### UI Design
- Modern, professional appearance
- Responsive design with proper scaling
- Consistent theming across all controls
- Intuitive user experience
- Accessibility considerations

### Performance
- Lazy loading where appropriate
- Background operations for long-running tasks
- Efficient memory management
- Smart caching strategies
- Concurrent operation support

### Error Handling
- Graceful degradation
- User-friendly error messages
- Comprehensive logging
- Automatic retry mechanisms
- Fallback options

## Testing Requirements

The application should:
- Compile without warnings or errors
- Handle network failures gracefully
- Support multiple concurrent operations
- Maintain responsive UI during operations
- Persist settings and queue between sessions
- Work with all supported Windows versions (7+)

## Documentation

Include comprehensive documentation:
- README.md with feature overview and usage instructions
- Architecture documentation
- Configuration guide
- Troubleshooting guide
- Development setup instructions

This prompt should result in a complete, fully functional music streaming application with professional-grade architecture, comprehensive features, and modern UI design. 