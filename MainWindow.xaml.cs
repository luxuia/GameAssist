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
        var apiKey = _config.Provider == ApiProvider.ZhipuAI
            ? _config.ZhipuApiKey
            : _config.OpenAiApiKey;
        var endpoint = _config.Provider == ApiProvider.ZhipuAI
            ? _config.ZhipuEndpoint
            : _config.OpenAiEndpoint;

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
        var providerName = _config.Provider == ApiProvider.ZhipuAI ? "Zhipu AI" : "OpenAI";
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
        var apiKey = _config.Provider == ApiProvider.ZhipuAI
            ? _config.ZhipuApiKey
            : _config.OpenAiApiKey;
        var providerName = _config.Provider == ApiProvider.ZhipuAI ? "Zhipu AI" : "OpenAI";

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

                var apiKey = _config.Provider == ApiProvider.ZhipuAI
                    ? _config.ZhipuApiKey
                    : _config.OpenAiApiKey;
                var providerName = _config.Provider == ApiProvider.ZhipuAI ? "Zhipu AI" : "OpenAI";

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
        if (!string.IsNullOrEmpty(_viewModel.LastSuggestion) && _lastUploadedImageBytes != null)
        {
            ShowOverlay(_viewModel.LastSuggestion, _lastUploadedImageWidth, _lastUploadedImageHeight);
            UpdateStatus("Overlay displayed.");
        }
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
            _overlayWindow.Configure(_config.OverlayWidth, _config.OverlayFontSize, _config.OverlayAutoHideSeconds);
        }

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

    protected override void OnClosed(EventArgs e)
    {
        StopTimer();
        _overlayWindow?.Close();
        base.OnClosed(e);
    }
}
