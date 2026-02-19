# Dota 2 Game Assistant - 项目概述

## 项目简介

Dota 2 Game Assistant 是一个基于 WPF 开发的实时游戏辅助应用，通过捕获 Dota 2 游戏画面并调用 OpenAI GPT-4o Vision API 进行分析，为玩家提供实时游戏建议。

### 主要功能
- 自动检测 Dota 2 进程窗口
- 定时捕获游戏屏幕截图
- 手动上传游戏截图进行分析（调试用）
- 调用 OpenAI GPT-4o Vision API 分析游戏画面
- 在游戏窗口上显示透明叠加层展示建议
- 可配置的分析间隔、叠加层样式、提示词等

---

## 技术栈

### 核心框架
- **.NET 10.0-windows** - 应用程序框架
- **WPF (Windows Presentation Foundation)** - UI 框架
- **C# 12+** - 编程语言

### 主要依赖包
- `System.Drawing.Common 8.0.0` - 屏幕捕获和图像处理

### Windows API (P/Invoke)
- `user32.dll` - 窗口管理 API
  - `GetForegroundWindow()` - 获取前景窗口句柄
  - `GetWindowRect()` - 获取窗口矩形区域
  - `GetWindowThreadProcessId()` - 获取窗口对应的进程 ID
  - `GetWindowText()` - 获取窗口标题

### 外部服务
- **OpenAI GPT-4o Vision API** - 图像分析服务
  - 端点: `https://api.openai.com/v1/chat/completions`
  - 传输方式: JSON (base64 编码的图像数据)
- **Zhipu AI GLM-4V-6 Vision API** - 图像分析服务（支持 GLM-4V-6 模型）
  - 端点: `https://open.bigmodel.cn/api/paas/v4/chat/completions`
  - 传输方式: JSON (base64 编码的图像数据)
  - 认证方式: JWT token (需使用 id.secret 格式的 API Key)

---

## 项目结构

```
GameAssist/
├── NativeMethods.cs              # Windows API P/Invoke 声明
├── App.xaml                      # 应用程序入口 XAML
├── App.xaml.cs                   # 应用程序入口代码
├── MainWindow.xaml               # 主窗口 UI
├── MainWindow.xaml.cs            # 主窗口逻辑
├── MainWindowViewModel.cs        # 主窗口 MVVM 视图模型
│
├── Models/                       # 数据模型
│   ├── AppConfig.cs              # 应用配置模型
│   └── Dota2Prompts.cs           # Dota 2 预定义提示词
│
├── Services/                     # 业务服务
│   ├── OpenAIService.cs          # OpenAI API 服务
│   ├── ProcessDetectionService.cs # 进程检测服务
│   └── ScreenCaptureService.cs   # 屏幕捕获服务
│
├── Views/                        # 视图窗口
│   ├── SettingsWindow.xaml       # 设置窗口 UI
│   ├── SettingsWindow.xaml.cs    # 设置窗口逻辑
│   ├── OverlayWindow.xaml        # 叠加层窗口 UI
│   └── OverlayWindow.xaml.cs     # 叠加层窗口逻辑
│
└── GameAssist.csproj             # 项目配置文件
```

### 文件职责说明

| 文件 | 职责 |
|------|------|
| `NativeMethods.cs` | 定义 Windows API P/Invoke 声明和 RECT 结构体 |
| `App.xaml/cs` | WPF 应用程序入口点 |
| `MainWindow.xaml/cs` | 主窗口界面和交互逻辑 |
| `MainWindowViewModel.cs` | 主窗口的 MVVM 数据绑定 |
| `Models/AppConfig.cs` | 配置加载/保存、配置属性定义 |
| `Models/Dota2Prompts.cs` | 预定义的 Dota 2 分析提示词 |
| `Services/OpenAIService.cs` | 与 OpenAI API 通信的业务逻辑 |
| `Services/ProcessDetectionService.cs` | 检测 Dota 2 进程和窗口状态 |
| `Services/ScreenCaptureService.cs` | 捕获窗口屏幕截图 |
| `Views/SettingsWindow.xaml/cs` | 设置界面，包括 API 配置和测试 |
| `Views/OverlayWindow.xaml/cs` | 透明的叠加层窗口，显示建议 |

---

## 编码规范

### 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| 命名空间 | PascalCase | `GameAssist`, `GameAssist.Services` |
| 类名 | PascalCase | `OpenAIService`, `MainWindowViewModel` |
| 接口名 | PascalCase，以 I 开头 | (暂无接口) |
| 方法 | PascalCase | `CaptureWindow()`, `AnalyzeImageAsync()` |
| 属性 | PascalCase | `IsRunning`, `StatusText` |
| 私有字段 | _camelCase | `_config`, `_timer` |
| 常量 | PascalCase | `Dota2ProcessName` |
| 参数 | camelCase | `imageBytes`, `cancellationToken` |

### 异步方法规范
- 异步方法名称以 `Async` 结尾
- 使用 `CancellationToken` 参数支持取消操作
- 在需要时正确使用 `ConfigureAwait(false)`

### XML 文档注释
- 公共类型和成员应包含 XML 文档注释
- 示例:
```csharp
/// <summary>
/// 分析图像并返回 AI 建议
/// </summary>
/// <param name="imageBytes">图像字节数组（PNG 格式）</param>
/// <param name="prompt">分析提示词</param>
/// <param name="cancellationToken">取消令牌</param>
/// <returns>AI 生成的建议文本</returns>
public async Task<string?> AnalyzeImageAsync(byte[] imageBytes, string prompt, CancellationToken cancellationToken = default)
```

### 异常处理
- 使用具体的异常类型
- 异常消息应清晰说明问题
- 在日志中记录异常堆栈（生产环境）

### 配置管理
- 配置文件位置: `%AppData%\GameAssist\config.json`
- 使用 `AppConfig.Load()` 加载配置
- 使用 `AppConfig.Save(config)` 保存配置

### MVVM 模式
- ViewModel 实现 `INotifyPropertyChanged` 接口
- 使用 `CallerMemberName` 简化 `OnPropertyChanged` 调用
- UI 通过 `DataContext` 绑定到 ViewModel

### JSON 序列化
- 使用 `System.Text.Json`
- 配置 `JsonSerializerOptions`:
  - `WriteIndented = true` (配置文件可读性)
  - `PropertyNameCaseInsensitive = true` (默认设置)

### UI 线程安全
- 使用 `Dispatcher.Invoke` 或 `Dispatcher.InvokeAsync` 更新 UI
- 长时间操作使用 async/await 避免阻塞 UI 线程

### 代码组织原则
1. **单一职责**: 每个类只负责一个明确的功能
2. **依赖注入**: 通过构造函数传递依赖服务
3. **可测试性**: 服务类支持通过构造函数注入模拟对象

### 文件组织
- `Services/` 目录存放业务逻辑服务
- `Models/` 目录存放数据模型
- `Views/` 目录存放窗口视图
- 相关的 XAML 和 code-behind 文件放在同一位置

---

## 开发注意事项

### 添加新功能时
1. 首先更新相应的 Model（如果涉及配置）
2. 在 Services 层添加业务逻辑
3. 在 ViewModel 中添加属性和通知
4. 更新 View 的数据绑定

### 修改 OpenAI API 集成时
- 修改 `OpenAIService.cs`
- 确保请求格式符合 OpenAI API 规范
- 使用 JSON 格式而非 multipart/form-data
- 图像数据使用 base64 编码

### 修改 GLM-4V-6 API 集成时
- GLM-4V-6 使用与 GLM-4V 相同的 API 端点，但模型名称为 `glm-4v-6`
- 图像 URL 格式：`data:image/jpeg;base64,{base64Image}`
- 添加 `detail: "high"` 参数以提高分析质量
- 需要使用 JWT token 认证，使用 id.secret 格式的 API Key

### 测试 API 集成
- 使用 Settings 窗口的 "Test API" 按钮
- **OpenAI**: 需要先配置有效的 OpenAI API Key
- **Zhipu AI**: 需要配置 id.secret 格式的 API Key，推荐使用 glm-4v-6 模型

### 调试功能
- **上传截图（Upload Screenshot）**: 无需运行 Dota 2，直接上传本地截图进行分析
  - 点击 "Upload Screenshot" 按钮选择图片文件
  - 支持 PNG、JPG、JPEG、BMP 格式
  - 分析结果会显示在主窗口的 "AI Suggestions" 区域
  - 可用于测试不同的 AI 服务（OpenAI/GLM）
- **显示叠加层（Show Overlay）**: 手动显示透明叠加层
  - 上传截图并获取分析结果后可用
  - 点击 "Show Overlay" 按钮在当前窗口上显示建议
  - 用于调试叠加层样式和效果

### Git 提交规范
- **提交信息格式**:
  ```
  <类型>: <简要说明>

  <详细说明（可选）>

  Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
  ```
- **提交类型**:
  - `feat`: 新功能
  - `fix`: 修复bug
  - `docs`: 文档更新
  - `style`: 代码格式调整
  - `refactor`: 重构
  - `test`: 测试相关
  - `chore`: 构建或工具变动
  - `initial`: 初始提交
- **示例**:
  ```bash
  git commit -m "$(cat <<'EOF'
  feat: 添加 GLM-4V-6 支持

  - 更新 OpenAIService.cs 支持 GLM-4V-6 模型
  - 添加 JWT token 认证支持
  - 更新设置界面新增模型选项

  Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
  EOF
  )"
  ```

---

## 构建和运行

```bash
# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行应用
dotnet run

# 发布独立可执行文件
dotnet publish -c Release -r win-x64 --self-contained

---

## Git 提交规范

### 提交信息格式
采用 Angular 提交规范：
```
<类型>: <简要说明>

<详细说明（可选）>

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
```

### 提交类型
- `feat`: 新功能（feature）
- `fix`: 修复 bug（fix）
- `docs`: 文档更新（documentation）
- `style`: 代码格式调整（不影响功能）
- `refactor`: 重构（既不是新功能也不是修复 bug）
- `test`: 测试相关（增加或修改测试）
- `chore`: 构建或工具变动
- `initial`: 初始提交
- `config`: 配置文件修改

### 提交说明
- 简要说明应简洁明了（不超过 50 字符）
- 详细说明可以描述具体的改动内容
- 如果修复了某个 issue，可以添加 `Fixes #123`
- 每次提交都应包含 Co-Authored-By 标识

### 示例
```bash
# 新功能
git commit -m "$(cat <<'EOF'
feat: 添加 GLM-4V-6 模型支持

- 更新 OpenAIService.cs 支持 GLM-4V-6 API
- 添加 JWT token 认证支持
- 更新设置界面添加新模型选项
- 修复图像分析接口问题

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
EOF
)"

# 修复 bug
git commit -m "$(cat <<'EOF'
fix: 修复叠加层窗口闪烁问题

- 调整窗口透明度更新逻辑
- 使用 DoubleBuffered 减少闪烁
- 修复窗口位置计算错误

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
EOF
)"

# 文档更新
git commit -m "$(cat <<'EOF'
docs: 更新项目文档

- 添加 GLM-4V-6 API 使用说明
- 更新配置文件示例
- 添加 Git 提交规范
- 完善开发注意事项

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
EOF
)"
```

### 注意事项
- **必须严格测试后才能提交**
  - 提交前请运行 `git status` 检查修改的文件
  - 提交前请运行 `git diff` 查看具体的修改内容
  - 每次提交应该关注一个独立的功能或修复
  - 避免提交编译错误或测试失败的代码
  - 敏感信息（如 API Key）不应提交到仓库
- **测试要求**
  - 功能修改：完整测试所有相关功能
  - UI 修改：验证界面显示和交互正常
  - API 修改：测试与服务的交互
  - 配置修改：验证配置加载和保存正常
- **提交规范**
  - 小步提交，每次提交专注一个改动点
  - 修复问题时，在 commit message 中关联 issue
  - 重大修改前先创建分支开发
  - 重要修改需要经过代码 review
```

---

## 配置文件示例

### OpenAI 配置
```json
{
  "Provider": 0,
  "OpenAiApiKey": "sk-your-api-key-here",
  "OpenAiEndpoint": "https://api.openai.com/v1/chat/completions",
  "ModelName": "gpt-4o",
  "MaxTokens": 500,
  "Temperature": 0.7,
  "IntervalSeconds": 60,
  "Dota2Only": true,
  "ShowPreview": true,
  "SelectedPrompt": 0,
  "CustomPrompt": "",
  "OverlayEnabled": true,
  "OverlayWidth": 600,
  "OverlayFontSize": 16,
  "OverlayAutoHideSeconds": 30,
  "AutoStart": false
}
```

### Zhipu AI 配置
```json
{
  "Provider": 1,
  "ZhipuApiKey": "your-id.your-secret",
  "ZhipuEndpoint": "https://open.bigmodel.cn/api/paas/v4/chat/completions",
  "ModelName": "glm-4v-6",
  "MaxTokens": 500,
  "Temperature": 0.7,
  "IntervalSeconds": 60,
  "Dota2Only": true,
  "ShowPreview": true,
  "SelectedPrompt": 0,
  "CustomPrompt": "",
  "OverlayEnabled": true,
  "OverlayWidth": 600,
  "OverlayFontSize": 16,
  "OverlayAutoHideSeconds": 30,
  "AutoStart": false
}
```
