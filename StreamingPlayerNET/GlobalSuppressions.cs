using System.Diagnostics.CodeAnalysis;

// Suppress warnings for expected Windows Forms behavior
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows Forms application is expected to run on Windows only")]

// Suppress unused event warnings for Windows Media Service
[assembly: SuppressMessage("Design", "CS0067:The event is never used", Justification = "Event is part of public API and may be used by consumers", Scope = "member", Target = "~E:StreamingPlayerNET.Services.WindowsMediaService.MediaCommandReceived")]