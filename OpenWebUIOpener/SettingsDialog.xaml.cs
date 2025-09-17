using System;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace OpenWebUIOpener
{
    public partial class SettingsDialog : Window
    {
        private readonly AppConfig _config;

        public SettingsDialog()
        {
            InitializeComponent();
            _config = AppConfig.Instance;
            LoadSettings();
        }

        private void LoadSettings()
        {
            TxtAppTitle.Text = _config.ApplicationTitle;
            TxtWebUrl.Text = _config.WebUrl;
            TxtCommand.Text = _config.Command;
            TxtArguments.Text = _config.Arguments;
            TxtWorkingDirectory.Text = _config.WorkingDirectory;
            TxtStartupDelay.Text = _config.StartupDelay.ToString();
            ChkStartMinimized.IsChecked = _config.StartMinimized;
        }

        private void BtnBrowseWorkingDir_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Working Directory",
                UseDescriptionForTitle = true,
                SelectedPath = !string.IsNullOrWhiteSpace(_config.WorkingDirectory)
                    ? _config.WorkingDirectory
                    : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TxtWorkingDirectory.Text = dialog.SelectedPath;
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                    "Are you sure you want to reset all settings to their default values?",
                    "Confirm Reset",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _config.ResetToDefaults();
                LoadSettings();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate URL
            if (!Uri.TryCreate(TxtWebUrl.Text, UriKind.Absolute, out _))
            {
                MessageBox.Show("Please enter a valid URL", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtWebUrl.Focus();
                return;
            }

            // Validate command
            if (string.IsNullOrWhiteSpace(TxtCommand.Text))
            {
                MessageBox.Show("Command cannot be empty", "Invalid Command", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCommand.Focus();
                return;
            }

            // Validate startup delay
            if (!int.TryParse(TxtStartupDelay.Text, out int delay) || delay < 0)
            {
                MessageBox.Show("Startup delay must be a non-negative number", "Invalid Delay", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtStartupDelay.Focus();
                return;
            }

            // Save settings
            _config.ApplicationTitle = TxtAppTitle.Text;
            _config.WebUrl = TxtWebUrl.Text;
            _config.Command = TxtCommand.Text;
            _config.Arguments = TxtArguments.Text;
            _config.WorkingDirectory = TxtWorkingDirectory.Text;
            _config.StartupDelay = delay;
            _config.StartMinimized = ChkStartMinimized.IsChecked ?? false;
            _config.Save();

            DialogResult = true;
            Close();
        }
    }
}