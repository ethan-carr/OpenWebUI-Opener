using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using WinFormsBrushes = System.Drawing.Brushes;
using WinFormsIcon = System.Drawing.Icon;

namespace OpenWebUIOpener;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private NotifyIcon notifyIcon = null!;
    private Process? process;
    private AppConfig Config => AppConfig.Instance;

    public MainWindow()
    {
        InitializeComponent();
        
        // Set window title from config
        Title = Config.ApplicationTitle;
        
        InitializeTrayIcon();
        this.Loaded += MainWindow_Loaded;
        UpdateUI();
    }

    private void InitializeTrayIcon()
    {
        notifyIcon = new NotifyIcon();
        
        // Try to use custom icon, fallback to system icon
        try
        {
            // Try multiple possible locations for the icon
            string[] possiblePaths = {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "icon.ico"),
                System.IO.Path.Combine(AppContext.BaseDirectory, "icon.ico")
            };
            
            bool iconLoaded = false;
            foreach (string iconPath in possiblePaths)
            {
                if (File.Exists(iconPath))
                {
                    notifyIcon.Icon = new WinFormsIcon(iconPath);
                    iconLoaded = true;
                    break;
                }
            }
            
            if (!iconLoaded)
            {
                notifyIcon.Icon = SystemIcons.Application;
            }
        }
        catch
        {
            notifyIcon.Icon = SystemIcons.Application;
        }
        
        notifyIcon.Text = Config.ApplicationTitle;
        notifyIcon.DoubleClick += (s, e) => { Show(); WindowState = WindowState.Normal; };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show", null, (s, e) => { Show(); WindowState = WindowState.Normal; });
        contextMenu.Items.Add("Open Web UI", null, (s, e) => OpenWebUI());
        contextMenu.Items.Add("Settings", null, (s, e) => OpenSettings());
        contextMenu.Items.Add("Exit", null, (s, e) => { process?.Kill(); Close(); });
        notifyIcon.ContextMenuStrip = contextMenu;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await StartOpenWebUI();
    }

    private void UpdateUI()
    {
        Dispatcher.Invoke(() =>
        {
            bool isRunning = process != null && !process.HasExited;
            
            // Update status indicator
            StatusIndicator.Fill = new SolidColorBrush(isRunning ? Colors.LimeGreen : Colors.Red);
            StatusText.Text = isRunning ? "Running" : "Stopped";
            StatusText.Foreground = new SolidColorBrush(isRunning ? Colors.LimeGreen : Colors.Red);
        });
    }

    private async Task StartOpenWebUI()
    {
        AppendOutput($"Starting {Config.ApplicationTitle}...");
        AppendOutput($"Command: {Config.Command} {Config.Arguments}");
        AppendOutput($"Working Directory: {Config.GetEffectiveWorkingDirectory()}");

        process = new Process();
        process.StartInfo.FileName = Config.Command;
        process.StartInfo.Arguments = Config.Arguments;
        process.StartInfo.WorkingDirectory = Config.GetEffectiveWorkingDirectory();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        process.StartInfo.CreateNoWindow = true;

        // Set environment variables to force UTF-8 encoding
        process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
        process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";

        process.OutputDataReceived += (s, e) => Dispatcher.Invoke(() => AppendFilteredOutput(e.Data, false));
        process.ErrorDataReceived += (s, e) => Dispatcher.Invoke(() => AppendFilteredOutput(e.Data, true));

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for specified delay to show initial output
            await Task.Delay(Config.StartupDelay);

            // Then minimize to tray if configured to do so
            if (Config.StartMinimized)
            {
                WindowState = WindowState.Minimized;
                Hide();
                notifyIcon.Visible = true;
                AppendOutput($"{Config.ApplicationTitle} started. Minimized to tray.");
            }
            else
            {
                AppendOutput($"{Config.ApplicationTitle} started.");
            }
            
            UpdateUI();
        }
        catch (Exception ex)
        {
            AppendOutput($"Error starting {Config.ApplicationTitle}: {ex.Message}");
            UpdateUI();
        }
    }

    private void AppendOutput(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        AppendColoredText(text, System.Windows.Media.Brushes.White);
    }

    private void AppendColoredText(string text, System.Windows.Media.Brush color)
    {
        var paragraph = new Paragraph();
        var run = new Run(text + Environment.NewLine)
        {
            Foreground = color
        };
        paragraph.Inlines.Add(run);
        OutputTextBox.Document.Blocks.Add(paragraph);
        OutputTextBox.ScrollToEnd();
    }

    private void AppendFilteredOutput(string? text, bool isError)
    {
        if (string.IsNullOrEmpty(text)) return;

        // Filter out verbose traceback lines and local variables
        if (text.Contains("+----- locals -----+") || 
            text.Contains("+-------------------------------- locals ---------------------------------+") ||
            text.Contains("| +") ||
            text.StartsWith("| |") ||
            text.Contains("Traceback (most recent call last)") ||
            text.Contains("at 0x") ||
            (text.Contains("\\u") && text.Length > 100)) // Skip long lines with unicode escapes
        {
            return; // Skip verbose traceback details
        }

        // Simplify error messages
        if (isError && text.Contains("UnicodeEncodeError"))
        {
            AppendColoredText("ERROR: Unicode encoding issue with OpenWebUI output (continuing...)", System.Windows.Media.Brushes.Red);
            return;
        }

        // Clean up the text and remove problematic unicode characters
        string cleanText = text;
        try
        {
            // Replace problematic unicode block characters with simpler text
            cleanText = cleanText.Replace("\u2588", "#");
            cleanText = cleanText.Replace("\u2557", "+");
            cleanText = cleanText.Replace("\u2550", "-");
            cleanText = cleanText.Replace("\u2554", "+");
            cleanText = cleanText.Replace("\u255d", "+");
            cleanText = cleanText.Replace("\u2551", "|");
        }
        catch
        {
            cleanText = "[OpenWebUI output contains special characters]";
        }

        // Parse and colorize based on log level
        AppendColorizedLogLine(cleanText);
    }

    private void AppendColorizedLogLine(string text)
    {
        // Regex pattern to match log lines like: "2025-09-17 01:56:34.800 | INFO | ..."
        var logPattern = @"^(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d{3})\s*\|\s*(INFO|ERROR|WARNING|DEBUG|TRACE|WARN|CRITICAL)\s*\|\s*(.*)$";
        var match = Regex.Match(text, logPattern);

        if (match.Success)
        {
            var timestamp = match.Groups[1].Value;
            var logLevel = match.Groups[2].Value;
            var message = match.Groups[3].Value;

            var paragraph = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
            
            // Add timestamp in gray
            var timestampRun = new Run(timestamp)
            {
                Foreground = System.Windows.Media.Brushes.Gray,
                FontFamily = new System.Windows.Media.FontFamily("Consolas")
            };
            paragraph.Inlines.Add(timestampRun);
            
            paragraph.Inlines.Add(new Run(" | ") { Foreground = System.Windows.Media.Brushes.Gray });
            
            // Add log level with color
            var logLevelBrush = GetLogLevelColor(logLevel);
            var logLevelRun = new Run(logLevel)
            {
                Foreground = logLevelBrush,
                FontWeight = FontWeights.Bold,
                FontFamily = new System.Windows.Media.FontFamily("Consolas")
            };
            paragraph.Inlines.Add(logLevelRun);
            
            paragraph.Inlines.Add(new Run(" | ") { Foreground = System.Windows.Media.Brushes.Gray });
            
            // Add message with appropriate color
            var messageColor = logLevel == "ERROR" || logLevel == "CRITICAL" ? System.Windows.Media.Brushes.LightCoral : System.Windows.Media.Brushes.LightGray;
            var messageRun = new Run(message)
            {
                Foreground = messageColor,
                FontFamily = new System.Windows.Media.FontFamily("Consolas")
            };
            paragraph.Inlines.Add(messageRun);
            
            paragraph.Inlines.Add(new Run(Environment.NewLine));
            OutputTextBox.Document.Blocks.Add(paragraph);
        }
        else
        {
            // Not a standard log line, use default coloring
            var color = text.ToUpper().Contains("ERROR") ? System.Windows.Media.Brushes.LightCoral : System.Windows.Media.Brushes.LightGray;
            AppendColoredText(text, color);
        }
        
        OutputTextBox.ScrollToEnd();
    }

    private System.Windows.Media.Brush GetLogLevelColor(string logLevel)
    {
        return logLevel.ToUpper() switch
        {
            "INFO" => System.Windows.Media.Brushes.LightBlue,
            "ERROR" => System.Windows.Media.Brushes.Red,
            "CRITICAL" => System.Windows.Media.Brushes.DarkRed,
            "WARNING" or "WARN" => System.Windows.Media.Brushes.Orange,
            "DEBUG" => System.Windows.Media.Brushes.LightGreen,
            "TRACE" => System.Windows.Media.Brushes.Magenta,
            _ => System.Windows.Media.Brushes.White
        };
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (process != null && !process.HasExited)
        {
            e.Cancel = true;
            Hide();
            AppendOutput("Application hidden to tray. Use tray icon to show or exit.");
        }
        else
        {
            notifyIcon.Dispose();
        }
    }

    // New event handlers for the modern UI
    private void LaunchBrowserButton_Click(object sender, RoutedEventArgs e)
    {
        OpenWebUI();
    }
    
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        OpenSettings();
    }

    private void OpenWebUI()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Config.WebUrl,
                UseShellExecute = true
            });
            AppendOutput($"Opening web browser to {Config.WebUrl}");
        }
        catch (Exception ex)
        {
            AppendOutput($"Error opening web browser: {ex.Message}");
        }
    }
    
    private void OpenSettings()
    {
        var settingsDialog = new SettingsDialog
        {
            Owner = this
        };
        
        if (settingsDialog.ShowDialog() == true)
        {
            // Update UI with new settings
            Title = Config.ApplicationTitle;
            notifyIcon.Text = Config.ApplicationTitle;
            
            // Show restart message if needed
            if (process != null && !process.HasExited)
            {
                AppendOutput("Settings saved. Please restart the application for all changes to take effect.");
            }
            else
            {
                AppendOutput("Settings saved.");
            }
        }
    }
}