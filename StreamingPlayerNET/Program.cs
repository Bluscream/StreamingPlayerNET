using NLog;
using StreamingPlayerNET.UI;
using StreamingPlayerNET.Services;

namespace StreamingPlayerNET;

static class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static LogService? _logService;
    private static ConfigurationService? _configService;
    
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            // Create logs directory in settings folder
            var settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StreamingPlayerNET");
            var logsFolder = Path.Combine(settingsFolder, "logs");
            if (!Directory.Exists(logsFolder))
            {
                Directory.CreateDirectory(logsFolder);
            }
            
            // Initialize NLog
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");
            
            // Initialize configuration service first
            _configService = new ConfigurationService();
            
            // Initialize LogService with configuration service to respect debug settings
            _logService = new LogService(_configService);
            
            Logger.Info("=== StreamingPlayerNET Application Starting ===");
            Logger.Info($"Operating System: {Environment.OSVersion}");
            Logger.Info($".NET Version: {Environment.Version}");
            Logger.Info($"Working Directory: {Environment.CurrentDirectory}");
            
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Logger.Debug("Application configuration initialized");
            
            Logger.Info("Starting main application form");
            Application.Run(new MainForm(_logService, _configService));
            
            Logger.Info("Application form closed, main thread ending");
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Fatal error during application startup");
            MessageBox.Show($"Fatal error during startup: {ex.Message}", "Application Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Logger.Info("=== StreamingPlayerNET Application Ended ===");
            _logService?.Dispose();
            LogManager.Shutdown();
        }
    }    
}