using GameAssist.Models;
using System.Windows;
using System.Windows.Controls;

namespace GameAssist.Views;

public partial class SettingsWindow : Window
{
    private AppConfig _config;
    private ApiProvider _currentProvider = ApiProvider.OpenAI;
    private bool _isInitialized = false;

    public SettingsWindow(AppConfig config)
    {
        InitializeComponent();
        _config = config;
        SetupEventHandlers();
        LoadSettings();
    }

    private void SetupEventHandlers()
    {
        MaxTokensSlider.ValueChanged += (s, e) => MaxTokensLabel.Text = ((int)e.NewValue).ToString();
        TemperatureSlider.ValueChanged += (s, e) => TemperatureLabel.Text = e.NewValue.ToString("F1");
        IntervalSlider.ValueChanged += (s, e) => IntervalLabel.Text = $"{(int)e.NewValue}s";
        OverlayWidthSlider.ValueChanged += (s, e) => OverlayWidthLabel.Text = ((int)e.NewValue).ToString();
        FontSizeSlider.ValueChanged += (s, e) => FontSizeLabel.Text = ((int)e.NewValue).ToString();
        AutoHideSlider.ValueChanged += (s, e) => AutoHideLabel.Text = $"{(int)e.NewValue}s";
        CompressionQualitySlider.ValueChanged += (s, e) => CompressionQualityLabel.Text = ((int)e.NewValue).ToString();
        MaxImageSizeSlider.ValueChanged += (s, e) => MaxImageSizeLabel.Text = $"{(int)e.NewValue} KB";
    }

    private void LoadSettings()
    {
        // Load API Provider
        _currentProvider = _config.Provider;
        ProviderBox.SelectedIndex = _config.Provider switch
        {
            ApiProvider.ZhipuAI => 1,
            ApiProvider.Doubao => 2,
            _ => 0
        };

        // Load model settings
        MaxTokensSlider.Value = _config.MaxTokens;
        MaxTokensLabel.Text = _config.MaxTokens.ToString();
        TemperatureSlider.Value = _config.Temperature;
        TemperatureLabel.Text = _config.Temperature.ToString("F1");

        // Load analysis settings
        IntervalSlider.Value = _config.IntervalSeconds;
        IntervalLabel.Text = $"{_config.IntervalSeconds}s";
        Dota2OnlyCheckBox.IsChecked = _config.Dota2Only;
        ShowPreviewCheckBox.IsChecked = _config.ShowPreview;
        AutoStartCheckBox.IsChecked = _config.AutoStart;

        // Load overlay settings
        OverlayEnabledCheckBox.IsChecked = _config.OverlayEnabled;
        OverlayWidthSlider.Value = _config.OverlayWidth;
        OverlayWidthLabel.Text = _config.OverlayWidth.ToString();
        FontSizeSlider.Value = _config.OverlayFontSize;
        FontSizeLabel.Text = _config.OverlayFontSize.ToString();
        AutoHideSlider.Value = _config.OverlayAutoHideSeconds;
        AutoHideLabel.Text = $"{_config.OverlayAutoHideSeconds}s";

        // Load prompt settings
        PromptTypeBox.SelectedIndex = (int)_config.SelectedPrompt;
        CustomPromptBox.Text = _config.CustomPrompt;
        UpdateCustomPromptState();

        // Load compression settings
        EnableCompressionCheckBox.IsChecked = _config.EnableCompression;
        CompressionQualitySlider.Value = _config.CompressionQuality;
        CompressionQualityLabel.Text = _config.CompressionQuality.ToString();
        MaxImageSizeSlider.Value = _config.MaxImageSizeKB;
        MaxImageSizeLabel.Text = $"{_config.MaxImageSizeKB} KB";

        // Update provider UI after layout is complete
        EventHandler? layoutHandler = null;
        layoutHandler = (s, e) =>
        {
            this.LayoutUpdated -= layoutHandler;
            UpdateApiProviderUI();
            _isInitialized = true;
        };
        this.LayoutUpdated += layoutHandler;
    }

    private void UpdateApiProviderUI()
    {
        if (_currentProvider == ApiProvider.ZhipuAI)
        {
            ApiKeyLabel.Text = "Zhipu AI API Key (id.secret):";
            ApiKeyBox.Password = _config.ZhipuApiKey;
            EndpointBox.Text = _config.ZhipuEndpoint;
            FilterModelList("ZhipuAI");

            // Select Zhipu model if current model is not Zhipu
            if (_config.ModelName.StartsWith("gpt") || _config.ModelName.StartsWith("doubao") || string.IsNullOrEmpty(_config.ModelName))
            {
                SelectModel("glm-4.6v");
            }
            else
            {
                SelectModel(_config.ModelName);
            }
        }
        else if (_currentProvider == ApiProvider.Doubao)
        {
            ApiKeyLabel.Text = "Doubao API Key:";
            ApiKeyBox.Password = _config.DoubaoApiKey;
            EndpointBox.Text = _config.DoubaoEndpoint;
            FilterModelList("Doubao");

            // Select Doubao model if current model is not Doubao
            if (_config.ModelName.StartsWith("gpt") || _config.ModelName.StartsWith("glm") || string.IsNullOrEmpty(_config.ModelName))
            {
                SelectModel("doubao-seed-2-0-code-preview-260215");
            }
            else
            {
                SelectModel(_config.ModelName);
            }
        }
        else
        {
            ApiKeyLabel.Text = "OpenAI API Key:";
            ApiKeyBox.Password = _config.OpenAiApiKey;
            EndpointBox.Text = _config.OpenAiEndpoint;
            FilterModelList("OpenAI");

            // Select OpenAI model if current model is Zhipu or Doubao
            if (_config.ModelName.StartsWith("glm") || _config.ModelName.StartsWith("doubao"))
            {
                SelectModel(_config.GetDefaultModel());
            }
            else
            {
                SelectModel(_config.ModelName);
            }
        }
    }

    private void FilterModelList(string providerTag)
    {
        // Store current selection
        string? currentModelName = null;
        if (ModelBox.SelectedItem is ComboBoxItem currentItem && currentItem.Visibility == Visibility.Visible)
        {
            currentModelName = currentItem.Content?.ToString();
        }

        for (int i = 0; i < ModelBox.Items.Count; i++)
        {
            if (ModelBox.Items[i] is ComboBoxItem item)
            {
                // Check if Tag is not null and matches provider
                if (item.Tag != null && item.Tag.ToString() == providerTag)
                {
                    item.Visibility = Visibility.Visible;
                }
                else
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Select the first visible item or restore previous selection if still visible
        ModelBox.SelectedIndex = -1;
        bool restored = false;
        if (!string.IsNullOrEmpty(currentModelName))
        {
            for (int i = 0; i < ModelBox.Items.Count; i++)
            {
                if (ModelBox.Items[i] is ComboBoxItem item &&
                    item.Visibility == Visibility.Visible &&
                    item.Content?.ToString() == currentModelName)
                {
                    ModelBox.SelectedIndex = i;
                    restored = true;
                    break;
                }
            }
        }

        // If couldn't restore, select first visible item
        if (!restored)
        {
            for (int i = 0; i < ModelBox.Items.Count; i++)
            {
                if (ModelBox.Items[i] is ComboBoxItem item && item.Visibility == Visibility.Visible)
                {
                    ModelBox.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    private void SelectModel(string modelName)
    {
        for (int i = 0; i < ModelBox.Items.Count; i++)
        {
            if (ModelBox.Items[i] is ComboBoxItem item &&
                item.Visibility == Visibility.Visible &&
                item.Content?.ToString() == modelName)
            {
                ModelBox.SelectedIndex = i;
                return;
            }
        }
    }

    private void SaveSettings()
    {
        _config.Provider = _currentProvider;

        if (_currentProvider == ApiProvider.ZhipuAI)
        {
            _config.ZhipuApiKey = ApiKeyBox.Password;
            _config.ZhipuEndpoint = EndpointBox.Text;
        }
        else if (_currentProvider == ApiProvider.Doubao)
        {
            _config.DoubaoApiKey = ApiKeyBox.Password;
            _config.DoubaoEndpoint = EndpointBox.Text;
        }
        else
        {
            _config.OpenAiApiKey = ApiKeyBox.Password;
            _config.OpenAiEndpoint = EndpointBox.Text;
        }

        _config.MaxTokens = (int)MaxTokensSlider.Value;
        _config.Temperature = TemperatureSlider.Value;

        if (ModelBox.SelectedItem is ComboBoxItem item)
        {
            _config.ModelName = item.Content?.ToString() ?? _config.GetDefaultModel();
        }

        _config.IntervalSeconds = (int)IntervalSlider.Value;
        _config.Dota2Only = Dota2OnlyCheckBox.IsChecked ?? true;
        _config.ShowPreview = ShowPreviewCheckBox.IsChecked ?? true;
        _config.AutoStart = AutoStartCheckBox.IsChecked ?? false;

        _config.OverlayEnabled = OverlayEnabledCheckBox.IsChecked ?? true;
        _config.OverlayWidth = (int)OverlayWidthSlider.Value;
        _config.OverlayFontSize = (int)FontSizeSlider.Value;
        _config.OverlayAutoHideSeconds = (int)AutoHideSlider.Value;

        _config.SelectedPrompt = (PromptType)PromptTypeBox.SelectedIndex;
        _config.CustomPrompt = CustomPromptBox.Text;

        _config.EnableCompression = EnableCompressionCheckBox.IsChecked ?? true;
        _config.CompressionQuality = (int)CompressionQualitySlider.Value;
        _config.MaxImageSizeKB = (long)MaxImageSizeSlider.Value;

        AppConfig.Save(_config);
    }

    private async void TestApi_Click(object sender, RoutedEventArgs e)
    {
        string apiKey = ApiKeyBox.Password;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            MessageBox.Show("Please enter an API key first.", "API Key Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var testButton = (Button)sender;
        testButton.IsEnabled = false;
        testButton.Content = "Testing...";

        try
        {
            var service = new Services.OpenAIService();

            string modelName = _config.GetDefaultModel();
            if (ModelBox.SelectedItem is ComboBoxItem item)
            {
                modelName = item.Content?.ToString() ?? _config.GetDefaultModel();
            }

            service.Configure(_currentProvider, apiKey, EndpointBox.Text, modelName,
                (int)MaxTokensSlider.Value, TemperatureSlider.Value);

            // Use a simple test image (1x1 transparent PNG)
            byte[] testImage = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
                0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F,
                0x15, 0xC4, 0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
                0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };

            var result = await service.AnalyzeImageAsync(testImage, "Reply with 'OK' if you can read this.");

            if (result != null && result.Contains("OK", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("API test successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"API test completed but unexpected response:\n{result}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"API test failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            testButton.IsEnabled = true;
            testButton.Content = "Test API";
        }
    }

    private void ProviderBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;

        if (ProviderBox.SelectedItem is ComboBoxItem item)
        {
            var providerTag = item.Tag?.ToString();
            _currentProvider = providerTag switch
            {
                "ZhipuAI" => ApiProvider.ZhipuAI,
                "Doubao" => ApiProvider.Doubao,
                _ => ApiProvider.OpenAI
            };
            UpdateApiProviderUI();
        }
    }

    private void ModelBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;

        // Model selection changed - automatically update provider if needed
        if (ModelBox.SelectedItem is ComboBoxItem item && item.Tag != null)
        {
            var providerTag = item.Tag.ToString();
            var newProvider = providerTag switch
            {
                "ZhipuAI" => ApiProvider.ZhipuAI,
                "Doubao" => ApiProvider.Doubao,
                _ => ApiProvider.OpenAI
            };

            if (newProvider != _currentProvider)
            {
                _currentProvider = newProvider;
                ProviderBox.SelectedIndex = newProvider switch
                {
                    ApiProvider.ZhipuAI => 1,
                    ApiProvider.Doubao => 2,
                    _ => 0
                };
                UpdateApiProviderUI();
                // Reselect the model since UpdateApiProviderUI might change selection
                SelectModel(item.Content?.ToString() ?? _config.GetDefaultModel());
            }
        }
    }

    private void PromptTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateCustomPromptState();
    }

    private void UpdateCustomPromptState()
    {
        bool isCustom = PromptTypeBox.SelectedIndex == (int)PromptType.Custom;
        CustomPromptBox.IsEnabled = isCustom;
        CustomPromptBox.Opacity = isCustom ? 1.0 : 0.5;

        if (!isCustom && PromptTypeBox.SelectedIndex >= 0)
        {
            var promptType = (PromptType)PromptTypeBox.SelectedIndex;
            CustomPromptBox.Text = Dota2Prompts.GetPrompt(promptType);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
