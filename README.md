# StreamingPlayerNET 🎵

A **fully functional** .NET WinForms application for browsing and playing music from multiple streaming sources with maximum Windows compatibility, featuring a modern multi-source architecture.

##  Screenshots

![](https://files.catbox.moe/isghdb.png) ![](https://files.catbox.moe/yb9c7v.png)

## ✅ **COMPLETED FEATURES**

### 🎯 **Core Functionality**
- ✅ **Multi-Source Architecture**: Unified interface for various streaming services
- ✅ **Real-Time Search**: Search across all enabled sources simultaneously
- ✅ **High-Quality Audio Playback**: Streams audio using yt-dlp and NAudio
- ✅ **Modern WinForms UI**: Clean, responsive interface with professional styling
- ✅ **Automatic Setup**: Downloads yt-dlp binary automatically on first run
- ✅ **Error-Free Build**: No compilation warnings or errors

### 🎮 **Player Features**
- ✅ **Full Player Controls**: Play, pause, stop, volume control
- ✅ **Progress Tracking**: Real-time progress bar and time display
- ✅ **Audio Quality Selection**: Configurable audio quality (128-320 kbps)
- ✅ **Error Handling**: Graceful fallback between different extraction methods
- ✅ **Smart Caching**: Intelligent temporary file management for smooth playback
- ✅ **Queue Management**: Full queue with add, remove, reorder functionality
- ✅ **Repeat & Shuffle**: Multiple repeat modes (None, One, All) and shuffle
- ✅ **Queue Persistence**: Automatic queue caching between app restarts

### 🔍 **Search & Browse**
- ✅ **Multi-Source Search**: Search across multiple streaming services simultaneously
- ✅ **Rich Metadata**: Song titles, artists, duration, and thumbnails
- ✅ **Source Attribution**: Clear indication of which source each result comes from
- ✅ **Configurable Results**: Adjustable maximum search results (default: 50)
- ✅ **Batch Loading**: Loads results progressively for responsive UI

### 🎵 **Playlist Management**
- ✅ **Multi-Source Playlists**: Access playlists from all enabled sources
- ✅ **Queue Persistence**: Queue automatically saved and restored between sessions
- ✅ **Local Queue Management**: Add, remove, and reorder songs in queue
- ✅ **Playlist Browsing**: Browse and play from source-specific playlists

### 🎨 **User Interface**
- ✅ **Dark Mode Support**: Complete dark/light theme switching with `Ctrl+T`
- ✅ **Configurable Hotkeys**: Full keyboard shortcut system with customizable bindings
- ✅ **Global Hotkeys**: Media key support with fallback for Windows Media Session
- ✅ **Panel Management**: Collapsible search and playlist panels
- ✅ **Status Display**: Configurable status bar with song information
- ✅ **Settings Form**: Comprehensive configuration interface

### ⚙️ **Configuration & Settings**
- ✅ **Comprehensive Settings**: 50+ configurable options across all categories
- ✅ **Hotkey Customization**: All keyboard shortcuts fully customizable
- ✅ **Audio Settings**: Volume, buffer size, quality preferences
- ✅ **Download Settings**: Timeout, concurrent downloads, file thresholds
- ✅ **UI Preferences**: Window size, panel visibility, theme settings
- ✅ **Logging Configuration**: Adjustable log levels and file management

### 🔧 **Advanced Features**
- ✅ **Multi-Source Architecture**: Plugin-based source provider system
- ✅ **Source Management**: Enable/disable individual sources
- ✅ **Error Recovery**: Automatic fallback between extraction methods
- ✅ **Performance Optimization**: Lazy loading and background operations
- ✅ **Memory Management**: Automatic cleanup of temporary files
- ✅ **Toast Notifications**: System notifications for playback events

## 🛠️ Technical Stack

- **Framework**: .NET 9.0 WinForms (Windows 7+ compatible)
- **Multi-Source Integration**
- **Audio Playback**: NAudio for professional audio handling
- **Architecture**: Async/await with proper error handling and UI responsiveness
- **Logging**: NLog for comprehensive logging and debugging

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or higher
- Windows 7 or higher
- Internet connection (for streaming and yt-dlp download)

### Installation & Setup
1. **Clone the repository**:
   ```bash
   git clone <repo-url>
   cd StreamingPlayerNET
   ```

2. **Build and run**:
   ```bash
   dotnet build
   dotnet run
   ```

3. **First run setup**:
   - Application will automatically download yt-dlp binary
   - Configure your preferred sources in Settings (Ctrl+,)
   - No additional configuration required for basic functionality!

### Usage

#### 🔍 **Searching for Music**
1. Enter a song name, artist, or album in the search box
2. Press Enter or click the Search button
3. Browse through results from all enabled sources
4. See song titles, artists, duration, and source for each result

#### 🎵 **Playing Music**
1. **Double-click** any search result to start playing
2. Use the player controls:
   - ▶️ **Play/Resume**: Start or resume playback
   - ⏸️ **Pause**: Pause current song
   - ⏹️ **Stop**: Stop playback completely
   - 🔊 **Volume**: Adjust volume from 0-100%
   - 🔄 **Repeat**: Cycle through repeat modes (None/One/All)
   - 🔀 **Shuffle**: Toggle shuffle mode

#### 📋 **Managing Queue**
- **Add Songs**: Double-click search results to add to queue
- **Remove Songs**: Right-click queue items to remove
- **Reorder**: Drag and drop to reorder queue items
- **Persistence**: Queue automatically saves and restores between sessions

#### 🎨 **Customizing Interface**
- **Dark Mode**: Press `Ctrl+T` or use View → Dark Mode
- **Panel Visibility**: Toggle search/playlist panels with `Ctrl+S`/`Ctrl+P`
- **Settings**: Press `Ctrl+,` to open comprehensive settings

## 🏗️ Architecture

```
StreamingPlayerNET/
├── StreamingPlayerNET/ (Main Application)
│   ├── Services/
│   │   ├── MultiSourceSearchService.cs
│   │   ├── MultiSourceDownloadService.cs
│   │   ├── MultiSourceMetadataService.cs
│   │   ├── SourceManager.cs
│   │   ├── ThemeService.cs
│   │   ├── GlobalHotkeys.cs
│   │   ├── QueueCacheService.cs
│   │   └── ConfigurationService.cs
│   └── UI/MainForm/
│       ├── Core.cs
│       ├── EventHandlers.cs
│       ├── PlaybackControl.cs
│       └── UIUpdates.cs
├── StreamingPlayerNET.Source.Base/ (Base Interfaces)
└── StreamingPlayerNET.Common/ (Shared Models)
```

### Key Components

- **Multi-Source Architecture**: Unified interface for multiple streaming services
- **Source Manager**: Centralized management of all source providers
- **Theme Service**: Complete dark/light mode theming system
- **Global Hotkeys**: Media key support with fallback mechanisms
- **Queue Caching**: Persistent queue state between sessions
- **Configuration System**: Comprehensive settings management

## 🔧 Configuration

The application provides extensive configuration options:

### Audio Settings
- **Default Volume**: 0-100% volume control
- **Audio Quality**: Low (128k) to Very High (320k) bitrates
- **Buffer Size**: Configurable audio buffer for smooth playback

### Download Settings
- **Concurrent Downloads**: Up to 3 simultaneous downloads
- **Timeout Settings**: Configurable timeouts for all operations
- **File Thresholds**: Smart handling of large files

### UI Settings
- **Dark Mode**: Complete theme switching
- **Window Management**: Remembered window size and position
- **Panel Visibility**: Configurable interface layout

### Hotkeys
- **Media Controls**: Play/pause, stop, next/previous track
- **Volume Control**: Volume up/down shortcuts
- **Interface**: Panel toggles, settings, help
- **Theme**: Dark mode toggle (`Ctrl+T`)

## 🎯 Advanced Features

### Multi-Source Strategy
1. **Unified Search**: Search across all enabled sources simultaneously
2. **Smart Routing**: Automatic source detection for downloads
3. **Fallback System**: Graceful degradation when sources fail
4. **Source Attribution**: Clear indication of result sources

### Performance Optimizations
- **Lazy Loading**: Search results load progressively
- **Background Operations**: Downloads and setup happen in background
- **Smart Caching**: Intelligent file and queue caching
- **Memory Efficient**: Streams audio without keeping large files

### Error Recovery
- **Graceful Degradation**: Falls back to alternative methods
- **User-Friendly Messages**: Clear error messages with suggested actions
- **Retry Logic**: Automatically retries failed operations
- **Stable Playback**: Continues playing even if one source fails

## 🔮 Future Enhancements

### Planned Features
- [ ] **Enhanced Playlist Management**: Create and manage local playlists
- [ ] **Download & Offline**: Save songs for offline listening
- [ ] **Equalizer**: Audio enhancement and sound effects
- [ ] **Lyrics Integration**: Real-time lyrics display
- [ ] **System Tray**: Minimize to system tray with notifications
- [ ] **Cross-Platform**: Expand to Linux and macOS using Avalonia

### Technical Improvements
- [ ] **Advanced Caching**: Smart caching for frequently played songs
- [ ] **Quality Selection**: Manual audio quality selection per source
- [ ] **Format Options**: Support for different audio formats
- [ ] **Streaming Optimization**: Better buffering and streaming
- [ ] **Plugin System**: Extensible architecture for additional sources

## 🤝 Contributing

Contributions are welcome! Areas of interest:
- **UI/UX Enhancements**: Improve the user interface
- **Performance Optimization**: Make the app faster and more efficient
- **Feature Additions**: Add new functionality
- **Bug Fixes**: Report and fix issues
- **Cross-Platform**: Help with Avalonia/MAUI ports

## 📞 Support

If you encounter issues:
1. Check that you have a stable internet connection
2. Verify your source configurations in Settings (Ctrl+,)
3. Try a different search term or video
4. Restart the application to refresh services
5. Check the status bar for error messages
6. Review logs in `%APPDATA%\StreamingPlayerNET\logs\`

---

**🎵 Enjoy your music! This player gives you access to multiple streaming services with a clean, desktop interface and powerful features.**