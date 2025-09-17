using System;
using System.IO;
using System.Text.Json;

namespace OpenWebUIOpener;

public class AppConfig
{
    // Default configuration values
    private const string DEFAULT_WEB_URL = "http://localhost:8080";
    private const string DEFAULT_COMMAND = "open-webui";
    private const string DEFAULT_ARGS = "serve";
    private const string DEFAULT_WORKING_DIR = ""; // Empty means use user profile directory
    private const int DEFAULT_STARTUP_DELAY = 20000; // 20 seconds
    private const bool DEFAULT_START_MINIMIZED = false;
    private const string CONFIG_FILE_NAME = "config.json";

    // Public properties
    public string WebUrl { get; set; } = DEFAULT_WEB_URL;
    public string Command { get; set; } = DEFAULT_COMMAND;
    public string Arguments { get; set; } = DEFAULT_ARGS;
    public string WorkingDirectory { get; set; } = DEFAULT_WORKING_DIR;
    public int StartupDelay { get; set; } = DEFAULT_STARTUP_DELAY;
    public bool StartMinimized { get; set; } = DEFAULT_START_MINIMIZED;
    public string ApplicationTitle { get; set; } = "OpenWebUI Launcher";

    // Environment variable names
    private const string ENV_WEB_URL = "OPENWEBUI_URL";
    private const string ENV_COMMAND = "OPENWEBUI_COMMAND";
    private const string ENV_ARGS = "OPENWEBUI_ARGS";
    private const string ENV_WORKING_DIR = "OPENWEBUI_WORKING_DIR";
    private const string ENV_STARTUP_DELAY = "OPENWEBUI_STARTUP_DELAY";
    private const string ENV_START_MINIMIZED = "OPENWEBUI_START_MINIMIZED";
    private const string ENV_APP_TITLE = "OPENWEBUI_APP_TITLE";

    // Singleton instance
    private static AppConfig? _instance;

    // Get the singleton instance
    public static AppConfig Instance
    {
        get
        {
            _instance ??= Load();
            return _instance;
        }
    }

    // Private constructor to enforce singleton pattern
    private AppConfig() { }

    // Load configuration from file or environment variables
    private static AppConfig Load()
    {
        var config = new AppConfig();
        
        // Try to load from config file first
        try
        {
            string configPath = GetConfigFilePath();
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                var loadedConfig = JsonSerializer.Deserialize<AppConfig>(json);
                if (loadedConfig != null)
                {
                    config = loadedConfig;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config file: {ex.Message}");
        }

        // Then override with environment variables if they exist
        config.ApplyEnvironmentVariables();
        
        return config;
    }

    // Apply any environment variables that are set
    private void ApplyEnvironmentVariables()
    {
        string? envValue;
        
        envValue = Environment.GetEnvironmentVariable(ENV_WEB_URL);
        if (!string.IsNullOrWhiteSpace(envValue))
            WebUrl = envValue;
            
        envValue = Environment.GetEnvironmentVariable(ENV_COMMAND);
        if (!string.IsNullOrWhiteSpace(envValue))
            Command = envValue;
            
        envValue = Environment.GetEnvironmentVariable(ENV_ARGS);
        if (!string.IsNullOrWhiteSpace(envValue))
            Arguments = envValue;
            
        envValue = Environment.GetEnvironmentVariable(ENV_WORKING_DIR);
        if (!string.IsNullOrWhiteSpace(envValue))
            WorkingDirectory = envValue;
            
        envValue = Environment.GetEnvironmentVariable(ENV_STARTUP_DELAY);
        if (!string.IsNullOrWhiteSpace(envValue) && int.TryParse(envValue, out int delay))
            StartupDelay = delay;
            
        envValue = Environment.GetEnvironmentVariable(ENV_START_MINIMIZED);
        if (!string.IsNullOrWhiteSpace(envValue) && bool.TryParse(envValue, out bool startMinimized))
            StartMinimized = startMinimized;
            
        envValue = Environment.GetEnvironmentVariable(ENV_APP_TITLE);
        if (!string.IsNullOrWhiteSpace(envValue))
            ApplicationTitle = envValue;
    }

    // Save current configuration to file
    public void Save()
    {
        try
        {
            string configPath = GetConfigFilePath();
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config file: {ex.Message}");
        }
    }

    // Reset configuration to default values
    public void ResetToDefaults()
    {
        WebUrl = DEFAULT_WEB_URL;
        Command = DEFAULT_COMMAND;
        Arguments = DEFAULT_ARGS;
        WorkingDirectory = DEFAULT_WORKING_DIR;
        StartupDelay = DEFAULT_STARTUP_DELAY;
        StartMinimized = DEFAULT_START_MINIMIZED;
        ApplicationTitle = "OpenWebUI Launcher";
    }

    // Get the path to the config file
    private static string GetConfigFilePath()
    {
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OpenWebUIOpener"
        );
        
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }
        
        return Path.Combine(appDataPath, CONFIG_FILE_NAME);
    }

    // Get the effective working directory
    public string GetEffectiveWorkingDirectory()
    {
        if (!string.IsNullOrWhiteSpace(WorkingDirectory))
        {
            return WorkingDirectory;
        }
        
        // Default to user profile directory if not specified
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}