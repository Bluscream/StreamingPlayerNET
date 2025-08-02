using System.Diagnostics;
using NLog;
using System.Windows.Forms;

namespace StreamingPlayerNET.Services;

public class FileAssociationService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Opens a file in its associated application
    /// </summary>
    /// <param name="filePath">The path to the file to open</param>
    /// <returns>True if the file was successfully opened, false otherwise</returns>
    public bool OpenFileInAssociatedApp(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Logger.Warn("Cannot open file: file path is null or empty");
            return false;
        }
        
        if (!File.Exists(filePath))
        {
            Logger.Warn($"Cannot open file: file does not exist: {filePath}");
            return false;
        }
        
        try
        {
            Logger.Info($"Opening file in associated application: {filePath}");
            
            // Use Process.Start to open the file with its associated application
            var processStartInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = "open"
            };
            
            Process.Start(processStartInfo);
            
            Logger.Info($"Successfully opened file: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to open file in associated application: {filePath}");
            return false;
        }
    }
    
    /// <summary>
    /// Opens a file in its associated application and shows a dialog if it fails
    /// </summary>
    /// <param name="filePath">The path to the file to open</param>
    /// <param name="showErrorDialog">Whether to show an error dialog if opening fails</param>
    /// <returns>True if the file was successfully opened, false otherwise</returns>
    public bool OpenFileInAssociatedApp(string filePath, bool showErrorDialog = false)
    {
        var success = OpenFileInAssociatedApp(filePath);
        
        if (!success && showErrorDialog)
        {
            var fileName = Path.GetFileName(filePath);
            var message = $"Failed to open file '{fileName}' in its associated application.\n\n" +
                         $"File path: {filePath}\n\n" +
                         "The file may not have an associated application, or the application may not be installed.";
            
            MessageBox.Show(message, "File Open Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        return success;
    }
} 