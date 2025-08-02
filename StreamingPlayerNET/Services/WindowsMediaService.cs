using StreamingPlayerNET.Common.Models;
using NLog;
using System.Runtime.InteropServices;

namespace StreamingPlayerNET.Services;

public class WindowsMediaService : IDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly MusicPlayerService _musicPlayerService;
    private readonly ConfigurationService _configService;
    private readonly IntPtr _windowHandle;
    
    // Windows Media Session Manager
    private bool _isInitialized = false;
    private bool _disposed = false;
    
    // Win32 constants for media keys
    private const int WM_APPCOMMAND = 0x0319;
    private const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
    private const int APPCOMMAND_MEDIA_STOP = 13;
    private const int APPCOMMAND_MEDIA_NEXTTRACK = 11;
    private const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;
    
    public event EventHandler<MediaCommand>? MediaCommandReceived;
    
    public WindowsMediaService(MusicPlayerService musicPlayerService, ConfigurationService configService, IntPtr windowHandle)
    {
        _musicPlayerService = musicPlayerService;
        _configService = configService;
        _windowHandle = windowHandle;
        
        // Wire up music player events
        _musicPlayerService.SongChanged += OnSongChanged;
        _musicPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _musicPlayerService.PositionChanged += OnPositionChanged;
        
        InitializeMediaSession();
    }
    
    private void InitializeMediaSession()
    {
        try
        {
            // For Windows Forms applications, we'll use a different approach
            // The media session will be handled through Windows message processing
            // and system tray integration
            
            if (_windowHandle != IntPtr.Zero)
            {
                _isInitialized = true;
                Logger.Info("Windows Media Service initialized successfully using Win32 approach");
            }
            else
            {
                Logger.Warn("Invalid window handle, Windows Media integration disabled");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize Windows Media Service");
        }
    }
    
    public bool ProcessMediaCommand(Message m)
    {
        if (m.Msg == WM_APPCOMMAND)
        {
            try
            {
                int cmd = (int)((m.LParam.ToInt64() >> 16) & 0xffff);
                
                var command = cmd switch
                {
                    APPCOMMAND_MEDIA_PLAY_PAUSE => MediaCommand.Play, // Will toggle based on current state
                    APPCOMMAND_MEDIA_STOP => MediaCommand.Stop,
                    APPCOMMAND_MEDIA_NEXTTRACK => MediaCommand.Next,
                    APPCOMMAND_MEDIA_PREVIOUSTRACK => MediaCommand.Previous,
                    _ => (MediaCommand?)null
                };
                
                if (command.HasValue)
                {
                    Logger.Info($"Received media command: {command.Value}");
                    
                    // Handle the command asynchronously
                    Task.Run(() => HandleMediaCommand(command.Value));
                    
                    // Notify any listeners
                    MediaCommandReceived?.Invoke(this, command.Value);
                    
                    return true; // Message handled
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error handling media command");
            }
        }
        
        return false; // Message not handled
    }
    
    public void UpdateMediaInfo(Song? song, PlaybackState state)
    {
        if (!_isInitialized || _disposed) return;
        
        try
        {
            if (song == null || state == PlaybackState.Stopped)
            {
                Logger.Debug("Media session cleared");
                return;
            }
            
            // For now, we'll just log the media info
            // In a full implementation, this could update the Windows taskbar thumbnail,
            // system tray tooltip, or other Windows integration points
            Logger.Debug($"Media info updated: {song.Title} by {song.Artist} - {GetStatusText(state)}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to update media info");
        }
    }
    
    private string GetStatusText(PlaybackState state)
    {
        return state switch
        {
            PlaybackState.Playing => "Playing",
            PlaybackState.Paused => "Paused",
            PlaybackState.Stopped => "Stopped",
            PlaybackState.Loading => "Loading",
            _ => "Unknown"
        };
    }
    

    
    private void OnSongChanged(object? sender, Song song)
    {
        UpdateMediaInfo(song, _musicPlayerService.GetPlaybackState());
    }
    
    private void OnPlaybackStateChanged(object? sender, PlaybackState state)
    {
                    var currentSong = _musicPlayerService.CurrentSong;
        UpdateMediaInfo(currentSong, state);
    }
    
    private void OnPositionChanged(object? sender, TimeSpan position)
    {
        // The SystemMediaTransportControls doesn't need continuous position updates
        // It will automatically show the current position for active playback
    }
    
    public void HandleMediaCommand(MediaCommand command)
    {
        try
        {
            switch (command)
            {
                case MediaCommand.Play:
                    // For play command from media keys, we'll toggle play/pause
                    if (_musicPlayerService.IsPaused)
                        _musicPlayerService.Resume();
                    else if (_musicPlayerService.IsPlaying)
                        _musicPlayerService.Pause();
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
                    break;
                    
                case MediaCommand.VolumeDown:
                    // Volume controls are typically handled by the system
                    break;
                    
                default:
                    Logger.Warn($"Unhandled media command: {command}");
                    break;
            }
            
            Logger.Info($"Handled media command: {command}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to handle media command: {command}");
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        try
        {
            // Clean up media session
            if (_isInitialized)
            {
                // Nothing specific to clean up for Win32 approach
            }
            
            // Unsubscribe from music player events
            _musicPlayerService.SongChanged -= OnSongChanged;
            _musicPlayerService.PlaybackStateChanged -= OnPlaybackStateChanged;
            _musicPlayerService.PositionChanged -= OnPositionChanged;
            
            _disposed = true;
            Logger.Info("Windows Media Service disposed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error disposing Windows Media Service");
        }
    }
    

}

public enum MediaCommand
{
    Play,
    Pause,
    Stop,
    Next,
    Previous,
    VolumeUp,
    VolumeDown
}