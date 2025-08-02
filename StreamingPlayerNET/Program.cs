using NLog;
using StreamingPlayerNET.UI;
using StreamingPlayerNET.Services;
using System.Reflection;

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
            
            // Initialize NLog - try external file first, then embedded resource
            var nlogConfigPath = "NLog.config";
            if (File.Exists(nlogConfigPath))
            {
                LogManager.Setup().LoadConfigurationFromFile(nlogConfigPath);
            }
            else
            {
                // Load from embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "StreamingPlayerNET.NLog.config";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    var configContent = reader.ReadToEnd();
                    var config = NLog.Config.XmlLoggingConfiguration.CreateFromXmlString(configContent);
                    LogManager.Configuration = config;
                }
                else
                {
                    throw new InvalidOperationException($"Could not find NLog configuration in external file '{nlogConfigPath}' or embedded resource '{resourceName}'");
                }
            }
            
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