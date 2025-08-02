# M4A Audio Support

This document describes the m4a audio format support that has been added to StreamingPlayerNET.

## Overview

The application now supports playing m4a (AAC) audio files through multiple methods:

1. **Local File Playback**: Users can open and play local m4a files directly
2. **Streaming Playback**: m4a files from streaming sources (YouTube Music, etc.) are supported
3. **Cached Playback**: Downloaded m4a files are properly cached and can be played offline

## Supported Audio Formats

The application now supports the following audio formats:

- **MP3** (.mp3) - MPEG Audio Layer III
- **M4A** (.m4a) - AAC Audio in MP4 container
- **WAV** (.wav) - Waveform Audio File Format
- **FLAC** (.flac) - Free Lossless Audio Codec
- **AAC** (.aac) - Advanced Audio Coding
- **OGG** (.ogg) - OGG Vorbis
- **Opus** (.opus) - Opus Audio Codec
- **WebM** (.webm) - WebM Audio (typically Opus)

## How to Use M4A Support

### Opening Local M4A Files

1. **File Menu**: Go to `File` → `Open File...` (or press `Ctrl+O`)
2. **File Dialog**: Select your m4a file from the file dialog
3. **Automatic Playback**: The file will be added to the queue and start playing automatically
4. **Queue Management**: The file will appear in the queue tab for further management

### Streaming M4A Files

M4a files from streaming sources (like YouTube Music) are automatically supported:

1. **Search**: Search for music as usual
2. **Automatic Detection**: The system automatically detects and selects m4a streams when available
3. **Quality Selection**: Higher quality m4a streams are preferred when available
4. **Caching**: Downloaded m4a files are cached for offline playback

## Technical Implementation

### Audio Format Detection

The application uses the `AudioFormatUtils` class to:

- Detect supported audio formats
- Validate file extensions
- Provide format descriptions and MIME types
- Map file extensions to codec names

### Playback Engine

The `NAudioPlaybackService` has been enhanced to:

- Use `AudioFileReader` for most formats including m4a
- Fall back to `MediaFoundationReader` for problematic m4a files
- Provide detailed error messages for unsupported formats
- Handle format-specific playback issues

### Caching System

The `CachingService` has been improved to:

- Properly validate cached audio files
- Support m4a file extension detection
- Handle m4a codec mapping
- Ensure cached files are in supported formats

## File Association

The application can:

- Open m4a files with the default system application
- Show cached m4a files in Windows Explorer
- Handle file associations for m4a files

## Error Handling

The application provides clear error messages for:

- Unsupported audio formats
- Corrupted or invalid m4a files
- Playback failures
- File access issues

## Performance Considerations

- M4a files are typically smaller than MP3 files at the same quality
- AAC codec provides better compression efficiency
- Cached m4a files load quickly for offline playback
- Streaming m4a files are buffered efficiently

## Troubleshooting

### Common Issues

1. **"Unsupported Format" Error**
   - Ensure the file has a valid .m4a extension
   - Check that the file is not corrupted
   - Verify the file contains valid AAC audio

2. **Playback Issues**
   - Try restarting the application
   - Check if the file plays in other media players
   - Verify system audio drivers are up to date

3. **Caching Problems**
   - Clear the application cache if files are corrupted
   - Check available disk space
   - Verify file permissions

### Getting Help

If you encounter issues with m4a playback:

1. Check the application logs for detailed error messages
2. Verify the m4a file plays in other applications
3. Try converting the file to a different format if needed
4. Report issues with specific file details and error messages

## Future Enhancements

Planned improvements for m4a support:

- Enhanced metadata extraction from m4a files
- Better quality selection for streaming m4a
- Improved caching strategies for m4a files
- Support for m4a playlists and chapters 