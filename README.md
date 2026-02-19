# Dota 2 Game Assistant

A WPF-based real-time game assistant for Dota 2 that uses OpenAI GPT-4o Vision API to analyze game screenshots and provide strategic suggestions.

## Features

- 🔍 **Automatic Dota 2 Detection**: Automatically detects and captures Dota 2 game windows
- 🖼️ **Screen Capture**: Periodic screenshots of the game screen
- 🤖 **AI Analysis**: Uses OpenAI GPT-4o Vision API to analyze game situations
- 💡 **Smart Suggestions**: Displays real-time game strategy and suggestions
- 🎨 **Transparent Overlay**: Shows suggestions on an overlay window
- ⚙️ **Configurable Settings**: Adjustable API configuration, capture intervals, and overlay styles
- 🧪 **Debug Mode**: Manual screenshot upload for testing without running the game

## Requirements

- .NET 10.0-windows
- Windows operating system
- OpenAI API key
- Dota 2 game

## Installation

### Prerequisites

1. Install [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Install [Dota 2](https://store.steampowered.com/app/570/Dota_2/)
3. Get an OpenAI API key from [OpenAI Platform](https://platform.openai.com/api-keys)

### Build from Source

```bash
# Clone the repository
git clone https://github.com/luxuia/GameAssist.git
cd GameAssist

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

### Publish for Distribution

```bash
# Publish as self-contained executable
dotnet publish -c Release -r win-x64 --self-contained

# Publish as framework-dependent (smaller size)
dotnet publish -c Release -r win-x64
```

## Configuration

The application uses a configuration file stored at `%AppData%\GameAssist\config.json`. You can configure:

- **OpenAI Settings**: API key, endpoint, model, and parameters
- **Capture Settings**: Interval seconds, Dota 2 only mode
- **Overlay Settings**: Visibility, dimensions, font size, auto-hide duration
- **Prompt Settings**: Pre-defined prompts or custom prompts

Example configuration:
```json
{
  "OpenAiApiKey": "sk-your-api-key-here",
  "OpenAiEndpoint": "https://api.openai.com/v1/chat/completions",
  "ModelName": "gpt-4o",
  "MaxTokens": 500,
  "Temperature": 0.7,
  "IntervalSeconds": 60,
  "Dota2Only": true,
  "OverlayEnabled": true,
  "OverlayAutoHideSeconds": 30,
  "AutoStart": false
}
```

## Usage

### First Run

1. Run the application
2. Go to **Settings** and configure your OpenAI API key
3. Click **Test API** to verify the connection
4. Click **Save Settings**

### Playing with Dota 2

1. Start Dota 2 game
2. The assistant will automatically detect the game
3. Screenshots will be captured periodically based on your configured interval
4. AI suggestions will be displayed in the main window
5. Click **Show Overlay** to display suggestions on an overlay window

### Debug Mode

Without running Dota 2:
1. Click **Upload Screenshot** to select a game screenshot
2. The AI will analyze the image and provide suggestions
3. Use **Show Overlay** to test the overlay functionality

## Architecture

```
GameAssist/
├── Models/          # Data models
├── Services/        # Business logic services
├── Views/           # UI windows
├── NativeMethods.cs # Windows API P/Invoke
├── MainWindow.xaml  # Main application window
└── App.xaml         # Application entry point
```

### Key Components

- **MainWindow**: Main application window with controls and status
- **SettingsWindow**: Configuration interface for API and application settings
- **OverlayWindow**: Transparent window for displaying suggestions
- **OpenAIService**: Handles communication with OpenAI API
- **ProcessDetectionService**: Detects Dota 2 game window
- **ScreenCaptureService**: Captures screenshots of the game window

## Development

### Technologies Used

- WPF (Windows Presentation Foundation)
- C# 12+
- .NET 10.0
- OpenAI GPT-4o Vision API
- Windows API (P/Invoke)

### Code Structure

- MVVM pattern implementation
- Dependency injection ready
- Async/await for non-blocking operations
- XML documentation for public APIs

### Build Commands

```bash
# Development build
dotnet build

# Release build
dotnet build -c Release

# Run tests
dotnet test

# Clean
dotnet clean
```

## Contributing

Feel free to submit issues and enhancement requests!

## License

This project is for personal and educational use.

## Disclaimer

This tool is for educational purposes. Use it responsibly and in accordance with OpenAI's terms of service and Dota 2's terms of use.