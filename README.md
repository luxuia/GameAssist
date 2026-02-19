# Dota 2 Game Assistant (Dota 2 游戏助手)

一个基于 WPF 的 Dota 2 实时游戏助手，使用 OpenAI GPT-4o Vision API 分析游戏截图并提供策略建议。

## 简介

本项目是一个专为 Dota 2 玩家设计的智能游戏辅助工具。通过自动捕获游戏画面并调用 AI 进行分析，为玩家提供实时的游戏策略和建议。

## 主要功能

- 🔍 **自动检测 Dota 2**：自动识别并捕获 Dota 2 游戏窗口
- 🖼️ **屏幕截图**：定期截取游戏画面
- 🤖 **AI 分析**：使用 OpenAI GPT-4o Vision API 分析游戏局势
- 💡 **智能建议**：显示实时游戏策略和建议
- 🎨 **透明叠加层**：在游戏画面上显示建议的透明窗口
- ⚙️ **可配置设置**：调整 API 配置、截图间隔和叠加层样式
- 🧪 **调试模式**：无需运行游戏即可手动上传截图测试

## 系统要求

- .NET 10.0-windows
- Windows 操作系统
- OpenAI API 密钥
- Dota 2 游戏

## 安装

### 前置要求

1. 安装 [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. 安装 [Dota 2](https://store.steampowered.com/app/570/Dota_2/)
3. 从 [OpenAI Platform](https://platform.openai.com/api-keys) 获取 API 密钥

### 从源码构建

```bash
# 克隆仓库
git clone https://github.com/luxuia/GameAssist.git
cd GameAssist

# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行应用
dotnet run
```

### 发布版本

```bash
# 发布为独立可执行文件
dotnet publish -c Release -r win-x64 --self-contained

# 发布为框架依赖型（体积更小）
dotnet publish -c Release -r win-x64
```

## 配置

应用程序使用存储在 `%AppData%\GameAssist\config.json` 的配置文件。您可以配置：

- **OpenAI 设置**：API 密钥、端点、模型和参数
- **截图设置**：间隔时间、仅 Dota 2 模式
- **叠加层设置**：可见性、尺寸、字体大小、自动隐藏时长
- **提示词设置**：预定义提示词或自定义提示词

示例配置：
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

## 使用方法

### 首次运行

1. 运行应用程序
2. 进入 **设置** 配置 OpenAI API 密钥
3. 点击 **测试 API** 验证连接
4. 点击 **保存设置**

### 游戏时使用

1. 启动 Dota 2 游戏
2. 助手会自动检测游戏
3. 根据配置的间隔定期截图
4. AI 建议将显示在主窗口中
5. 点击 **显示叠加层** 在游戏画面上显示建议

### 调试模式

不运行 Dota 2 时：
1. 点击 **上传截图** 选择游戏截图
2. AI 将分析图片并提供建议
3. 使用 **显示叠加层** 测试叠加层功能

## 架构

```
GameAssist/
├── Models/          # 数据模型
├── Services/        # 业务逻辑服务
├── Views/           # UI 窗口
├── NativeMethods.cs # Windows API P/Invoke
├── MainWindow.xaml  # 主应用窗口
└── App.xaml         # 应用程序入口点
```

### 核心组件

- **MainWindow**：主应用窗口，包含控件和状态显示
- **SettingsWindow**：API 和应用设置配置界面
- **OverlayWindow**：显示建议的透明窗口
- **OpenAIService**：处理与 OpenAI API 的通信
- **ProcessDetectionService**：检测 Dota 2 游戏窗口
- **ScreenCaptureService**：捕获游戏窗口截图

## 开发

### 使用的技术

- WPF (Windows Presentation Foundation)
- C# 12+
- .NET 10.0
- OpenAI GPT-4o Vision API
- Windows API (P/Invoke)

### 代码结构

- MVVM 模式实现
- 支持依赖注入
- 使用 async/await 实现非阻塞操作
- 公共 API 包含 XML 文档注释

### 构建命令

```bash
# 开发构建
dotnet build

# 发布构建
dotnet build -c Release

# 运行测试
dotnet test

# 清理
dotnet clean
```

## 贡献

欢迎提交问题和改进请求！

## 许可证

本项目仅供个人和教育使用。

## 免责声明

本工具仅用于教育目的。请负责任地使用，并遵守 OpenAI 的服务条款和 Dota 2 的使用条款。

---

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