# OpenWebUI Launcher

A simple, user-friendly Windows launcher for [OpenWebUI](https://github.com/open-webui/open-webui), making it easy to start, monitor, and access the interface with a single click.

![OpenWebUI Launcher Screenshot](screenshot.png)

## Features

- **Easy Launch**: Start OpenWebUI with a single click, no command line needed
- **System Tray Integration**: Runs in the background with minimal resources
- **Real-time Console Output**: Monitor startup and logs directly in the application
- **Quick Access**: One-click access to the web interface
- **Fully Configurable**: Customize command paths, arguments, URLs and more
- **Modern UI**: Clean, dark-themed interface

## Installation

### Prerequisites

1. Make sure you have [OpenWebUI](https://github.com/open-webui/open-webui) already installed on your system.
2. .NET 9 Runtime or SDK is required (the application is built with .NET 9).

### Download & Installation

1. Download the latest release from the [Releases page](https://github.com/yourusername/openwebui-launcher/releases).
2. Extract the ZIP file to a folder of your choice.
3. Run `OpenWebUIOpener.exe`.

## Configuration

The application can be configured in multiple ways:

### Using the Settings Dialog

1. Click the **⚙️ Settings** button in the main window or access Settings from the system tray icon's context menu.
2. Adjust the settings as needed:
   - **Application Title**: Customize the app window title
   - **Web URL**: The URL to open when clicking "Launch Browser" (default: http://localhost:8080)
   - **Command**: The command to execute (default: open-webui)
   - **Command Arguments**: Arguments to pass to the command (default: serve)
   - **Working Directory**: Where to execute the command (empty = user profile directory)
   - **Startup Delay**: Time in milliseconds to wait before minimizing to tray (if enabled)
   - **Start Minimized**: Start the application minimized to the system tray

### Using the Configuration File

Configuration is stored in `%APPDATA%\OpenWebUIOpener\config.json`. You can edit this file directly if you prefer:

```json
{
  "WebUrl": "http://localhost:8080",
  "Command": "open-webui",
  "Arguments": "serve",
  "WorkingDirectory": "",
  "StartupDelay": 20000,
  "StartMinimized": false,
  "ApplicationTitle": "OpenWebUI Launcher"
}
```

### Using Environment Variables

You can override settings using environment variables:

| Environment Variable | Description | Default |
|---------------------|-------------|---------|
| OPENWEBUI_URL | URL to open in browser | http://localhost:8080 |
| OPENWEBUI_COMMAND | Command to execute | open-webui |
| OPENWEBUI_ARGS | Command arguments | serve |
| OPENWEBUI_WORKING_DIR | Working directory | User profile folder |
| OPENWEBUI_STARTUP_DELAY | Delay before minimizing (ms) | 20000 |
| OPENWEBUI_START_MINIMIZED | Start minimized to tray | false |
| OPENWEBUI_APP_TITLE | Application window title | OpenWebUI Launcher |

## Common Customizations

### Running with a Different Port

If you're running OpenWebUI on a non-default port:

1. Open Settings
2. Change the Web URL to match your port (e.g., `http://localhost:3000`)
3. Save settings

### Using with a Custom Command Path

If the `open-webui` command isn't in your PATH:

1. Open Settings
2. Set the Command to the full path (e.g., `C:\Users\YourUsername\AppData\Local\Programs\Python\Python310\Scripts\open-webui.exe`)
3. Save settings

### Starting with Different Arguments

To customize how OpenWebUI starts:

1. Open Settings
2. Modify the Command Arguments field (e.g., `serve --port 8888`)
3. Save settings

## Troubleshooting

### Command Not Found

If you see an error like "The system cannot find the file specified":

1. Make sure OpenWebUI is installed and available in your PATH
2. Try specifying the full path to the executable in Settings
3. Check if the working directory is set correctly

### WebUI Not Loading

If the browser opens but the interface doesn't load:

1. Check the console output for error messages
2. Verify that the URL in settings matches what OpenWebUI is using
3. Ensure no firewall is blocking the connection

### Unicode Characters Not Displaying Correctly

The application includes special handling for Unicode/UTF-8 output. If you still see display issues:

1. Make sure your system is configured to use UTF-8
2. The application automatically sets PYTHONIOENCODING=utf-8 for the subprocess

## Building from Source

1. Clone this repository
2. Open the solution in Visual Studio 2022 or newer
3. Build the solution in Release mode
4. The compiled application will be in the bin/Release directory

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [OpenWebUI](https://github.com/open-webui/open-webui) for the amazing web interface