using GameAssist.Models;
using GameAssist.Services;
using GameAssist.Views;
using static GameAssist.Models.ApiProvider;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GameAssist;

public partial class MainWindow : Window
{
    private DispatcherTimer? _timer;
    private AppConfig _config;
    private MainWindowViewModel _viewModel;
    private OpenAIService _openAiService;
    private ProcessDetectionService _processDetectionService;
    private ScreenCaptureService _screenCaptureService;
    private OverlayWindow? _overlayWindow;
    private byte[]? _lastUploadedImageBytes;
    private int _lastUploadedImageWidth;
    private int _lastUploadedImageHeight;

    public MainWindow()
    {
        InitializeComponent();
        _config = AppConfig.Load();
        _viewModel = new MainWindowViewModel();
        _openAiService = new OpenAIService();
        _processDetectionService = new ProcessDetectionService();
        _screenCaptureService = new ScreenCaptureService();

        DataContext = _viewModel;
        InitializeServices();

        Loaded += MainWindow_Loaded;
    }

    private void InitializeServices()
    {
        var (apiKey, endpoint) = _config.Provider switch
        {
            ApiProvider.ZhipuAI => (_config.ZhipuApiKey, _config.ZhipuEndpoint),
            ApiProvider.Doubao => (_config.DoubaoApiKey, _config.DoubaoEndpoint),
            _ => (_config.OpenAiApiKey, _config.OpenAiEndpoint)
        };

        _openAiService.Configure(
            _config.Provider,
            apiKey,
            endpoint,
            _config.ModelName,
            _config.MaxTokens,
            _config.Temperature
        );
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var providerName = _config.Provider switch
        {
            ApiProvider.ZhipuAI => "Zhipu AI",
            ApiProvider.Doubao => "Doubao",
            _ => "OpenAI"
        };
        UpdateStatus($"Ready. Configure {providerName} API key in Settings to get started.");
        UpdateDota2Status();

        if (_config.AutoStart)
        {
            StartAssistant();
        }
    }

    private void StartTimer()
    {
        _timer?.Stop();

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(_config.IntervalSeconds);
        _timer.Tick += async (_, __) => await CaptureAndAnalyzeAsync();
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        StartAssistant();
    }

    private void StartAssistant()
    {
        var (apiKey, providerName) = _config.Provider switch
        {
            ApiProvider.ZhipuAI => (_config.ZhipuApiKey, "Zhipu AI"),
            ApiProvider.Doubao => (_config.DoubaoApiKey, "Doubao"),
            _ => (_config.OpenAiApiKey, "OpenAI")
        };

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            MessageBox.Show($"Please configure your {providerName} API key in Settings first.",
                "API Key Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!_openAiService.IsConfigured)
        {
            InitializeServices();
        }

        _viewModel.IsRunning = true;
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
        CaptureButton.IsEnabled = true;
        UpdateStatus("Running...");

        StartTimer();
        UpdateDota2Status();

        // Run one capture immediately
        _ = CaptureAndAnalyzeAsync();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        StopAssistant();
    }

    private void StopAssistant()
    {
        StopTimer();
        _viewModel.IsRunning = false;
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
        CaptureButton.IsEnabled = false;
        UpdateStatus("Stopped.");
        UpdateDota2Status();
    }

    private async void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        await CaptureAndAnalyzeAsync();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var wasRunning = _viewModel.IsRunning;
        if (wasRunning)
        {
            StopTimer();
        }

        var settingsWindow = new SettingsWindow(_config);
        var result = settingsWindow.ShowDialog();

        if (result == true)
        {
            _config = AppConfig.Load();
            InitializeServices();
            UpdateStatus("Settings saved.");

            if (wasRunning)
            {
                StartTimer();
            }
        }
        else if (wasRunning)
        {
            StartTimer();
        }
    }

    private async void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Screenshot",
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*",
            FilterIndex = 1
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                UpdateStatus("Loading screenshot...");

                // Load the image
                var bmp = new Bitmap(openFileDialog.FileName);
                _lastUploadedImageBytes = _screenCaptureService.ConvertBitmapToBytes(
                    bmp,
                    ImageFormat.Jpeg,
                    _config.EnableCompression,
                    _config.CompressionQuality,
                    _config.MaxImageSizeKB);
                _lastUploadedImageWidth = bmp.Width;
                _lastUploadedImageHeight = bmp.Height;

                // Update preview
                UpdatePreview(bmp);
                NoPreviewText.Visibility = Visibility.Collapsed;
                _viewModel.UpdateCaptureInfo();

                UpdateStatus("Analyzing uploaded screenshot...");

                var (apiKey, providerName) = _config.Provider switch
                {
                    ApiProvider.ZhipuAI => (_config.ZhipuApiKey, "Zhipu AI"),
                    ApiProvider.Doubao => (_config.DoubaoApiKey, "Doubao"),
                    _ => (_config.OpenAiApiKey, "OpenAI")
                };

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    MessageBox.Show($"Please configure your {providerName} API key in Settings first.",
                        "API Key Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    UpdateStatus("API key required.");
                    bmp.Dispose();
                    return;
                }

                if (!_openAiService.IsConfigured)
                {
                    InitializeServices();
                }

                var prompt = Dota2Prompts.GetPrompt(_config.SelectedPrompt, _config.CustomPrompt);
                var suggestion = await _openAiService.AnalyzeImageAsync(_lastUploadedImageBytes, prompt);

                if (!string.IsNullOrEmpty(suggestion))
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _viewModel.LastSuggestion = suggestion;
                        SuggestionText.Text = suggestion;
                        ShowOverlayButton.IsEnabled = true;

                        // Show suggestion in overlay window
                        ShowOverlay(suggestion, bmp.Width, bmp.Height);
                    });

                    UpdateStatus($"Suggestion generated at {DateTime.Now:HH:mm:ss}");
                }
                else
                {
                    UpdateStatus("No suggestion returned.");
                }

                bmp.Dispose();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading image: {ex.Message}");
                MessageBox.Show($"Failed to load image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ShowOverlayButton_Click(object sender, RoutedEventArgs e)
    {
        // 即使没有截图，也能显示测试用的浮动窗口
        string testText = "这是一个测试文本，用于调试浮动窗口的拖动功能。\n\n您可以尝试拖动此窗口来测试是否还有抖动问题。\n\n如果您看到此文本，说明窗口已经正确显示。";
        ShowOverlay(testText, 800, 600);
        UpdateStatus("Overlay displayed.");
    }

    private async Task CaptureAndAnalyzeAsync()
    {
        if (!_viewModel.IsRunning) return;

        UpdateDota2Status();

        if (_config.Dota2Only && !_viewModel.IsDota2Active)
        {
            return;
        }

        try
        {
            var bmp = CaptureTargetWindow();
            if (bmp == null)
            {
                UpdateStatus("Failed to capture window.");
                return;
            }

            // Update preview
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdatePreview(bmp);
                NoPreviewText.Visibility = Visibility.Collapsed;
            });

            _viewModel.UpdateCaptureInfo();

            var imageBytes = _screenCaptureService.ConvertBitmapToBytes(
                bmp,
                ImageFormat.Jpeg,
                _config.EnableCompression,
                _config.CompressionQuality,
                _config.MaxImageSizeKB);
            var prompt = Dota2Prompts.GetPrompt(_config.SelectedPrompt, _config.CustomPrompt);

            UpdateStatus("Analyzing...");

            var suggestion = await _openAiService.AnalyzeImageAsync(imageBytes, prompt);

            if (!string.IsNullOrEmpty(suggestion))
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _viewModel.LastSuggestion = suggestion;
                    SuggestionText.Text = suggestion;

                    if (_config.OverlayEnabled && _viewModel.IsDota2Active)
                    {
                        ShowOverlay(suggestion, bmp.Width, bmp.Height);
                    }

                    // Save screenshot and suggestion
                    SaveScreenshotAndSuggestion(bmp, suggestion);
                });

                UpdateStatus($"Suggestion generated at {DateTime.Now:HH:mm:ss}");
            }
            else
            {
                UpdateStatus("No suggestion returned.");
            }

            bmp.Dispose();
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
            Console.WriteLine($"Error: {ex}");
        }
    }

    private Bitmap? CaptureTargetWindow()
    {
        if (_config.Dota2Only)
        {
            var dota2Window = FindDota2Window();
            if (dota2Window != IntPtr.Zero)
            {
                return _screenCaptureService.CaptureWindow(dota2Window);
            }
            return null;
        }
        else
        {
            return _screenCaptureService.CaptureForegroundWindow();
        }
    }

    private IntPtr FindDota2Window()
    {
        var hWnd = NativeMethods.GetForegroundWindow();
        if (hWnd == IntPtr.Zero) return IntPtr.Zero;

        if (_processDetectionService.IsDota2Window(hWnd))
        {
            return hWnd;
        }

        return IntPtr.Zero;
    }

    private void UpdatePreview(Bitmap bmp)
    {
        using var ms = new System.IO.MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        ms.Position = 0;
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = ms;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        PreviewImage.Source = bitmapImage;
    }

    private void ShowOverlay(string text, int width, int height)
    {
        if (_overlayWindow == null)
        {
            _overlayWindow = new OverlayWindow();
            _overlayWindow.Configure(_config.OverlayWidth, _config.OverlayFontSize, _config.OverlayAutoHideSeconds, _config.OverlayAutoHideEnabled);
            _overlayWindow.PromptTypeChanged += OverlayWindow_PromptTypeChanged;
        }

        // Set the current prompt type in the overlay window
        _overlayWindow.SetPromptType(_config.SelectedPrompt);

        var dota2Window = FindDota2Window();
        if (dota2Window != IntPtr.Zero && NativeMethods.GetWindowRect(dota2Window, out RECT rect))
        {
            _overlayWindow.PositionOverWindow(dota2Window, rect);
        }
        else
        {
            // Position over current foreground window as fallback
            var foregroundWindow = NativeMethods.GetForegroundWindow();
            if (foregroundWindow != IntPtr.Zero && NativeMethods.GetWindowRect(foregroundWindow, out RECT fgRect))
            {
                _overlayWindow.PositionOverWindow(foregroundWindow, fgRect);
            }
        }

        _overlayWindow.ShowSuggestion(text);
    }

    private void UpdateStatus(string status)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _viewModel.StatusText = status;
            StatusText.Text = status;
        });
    }

    private void UpdateDota2Status()
    {
        _viewModel.IsDota2Active = _processDetectionService.IsDota2Active();

        Application.Current.Dispatcher.Invoke(() =>
        {
            Dota2Indicator.Style = _viewModel.IsDota2Active
                ? (Style)Resources["Dota2ActiveStyle"]
                : (Style)Resources["Dota2InactiveStyle"];
        });
    }

    private async void OverlayWindow_PromptTypeChanged(object? sender, PromptType e)
    {
        _config.SelectedPrompt = e;
        AppConfig.Save(_config);

        // If there's a last uploaded image, reanalyze it
        if (_lastUploadedImageBytes != null)
        {
            await ReAnalyzeLastScreenshotAsync();
        }
        else
        {
            // Or capture a new screenshot for analysis
            await CaptureAndAnalyzeAsync();
        }
    }

    private async Task ReAnalyzeLastScreenshotAsync()
    {
        try
        {
            UpdateStatus("Reanalyzing with new prompt...");

            var prompt = Dota2Prompts.GetPrompt(_config.SelectedPrompt, _config.CustomPrompt);
            var suggestion = await _openAiService.AnalyzeImageAsync(_lastUploadedImageBytes!, prompt);

            if (!string.IsNullOrEmpty(suggestion))
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _viewModel.LastSuggestion = suggestion;
                    SuggestionText.Text = suggestion;

                    if (_config.OverlayEnabled)
                    {
                        Console.WriteLine($"Reanalyze: Calling ShowOverlay with suggestion length: {suggestion.Length}");
                        ShowOverlay(suggestion, _lastUploadedImageWidth, _lastUploadedImageHeight);
                    }
                });

                UpdateStatus($"Suggestion regenerated at {DateTime.Now:HH:mm:ss}");
            }
            else
            {
                UpdateStatus("No suggestion returned.");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error reanalyzing: {ex.Message}");
            Console.WriteLine($"Error: {ex}");
        }
    }

    private void SaveScreenshotAndSuggestion(Bitmap bmp, string suggestion)
    {
        try
        {
            string gameSessionFolder = GetGameSessionFolder();
            if (!System.IO.Directory.Exists(gameSessionFolder))
            {
                System.IO.Directory.CreateDirectory(gameSessionFolder);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string screenshotPath = System.IO.Path.Combine(gameSessionFolder, $"screenshot_{timestamp}.png");
            bmp.Save(screenshotPath, ImageFormat.Png);

            string suggestionPath = System.IO.Path.Combine(gameSessionFolder, $"suggestion_{timestamp}.txt");
            System.IO.File.WriteAllText(suggestionPath, suggestion);

            Console.WriteLine($"Screenshot and suggestion saved to: {gameSessionFolder}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving screenshot: {ex.Message}");
        }
    }

    private string GetGameSessionFolder()
    {
        string baseFolder = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Dota2Assistant",
            "Sessions");

        string today = DateTime.Now.ToString("yyyyMMdd");
        return System.IO.Path.Combine(baseFolder, today, $"Session_{DateTime.Now.ToString("HHmmss")}");
    }

    protected override void OnClosed(EventArgs e)
    {
        StopTimer();
        _overlayWindow?.Close();
        base.OnClosed(e);
    }
}
