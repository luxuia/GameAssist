using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GameAssist.Models;

namespace GameAssist.Views;

public partial class OverlayWindow : Window
{
    private DispatcherTimer? _autoHideTimer;
    private double _fontSize = 16;
    private int _autoHideSeconds = 30;
    private bool _autoHideEnabled = false;

    // Event for prompt type change
    public event EventHandler<PromptType>? PromptTypeChanged;

    private PromptType _currentPromptType;
    public PromptType CurrentPromptType
    {
        get => _currentPromptType;
        set
        {
            if (_currentPromptType != value)
            {
                _currentPromptType = value;
                PromptTypeChanged?.Invoke(this, value);
            }
        }
    }

    public OverlayWindow()
    {
        InitializeComponent();
        // 显示测试文本，方便调试拖动功能
        SuggestionTextBlock.Text = "这是一个测试文本，用于调试浮动窗口的拖动功能。\n\n您可以尝试拖动此窗口来测试是否还有抖动问题。\n\n如果您看到此文本，说明窗口已经正确显示。";
        MainGrid.Opacity = 1;

        // 设置默认的 prompt type 为 Default
        SetPromptType(PromptType.Default);
    }

    public void Configure(int overlayWidth, int fontSize, int autoHideSeconds, bool autoHideEnabled)
    {
        _fontSize = fontSize;
        _autoHideSeconds = autoHideSeconds;
        _autoHideEnabled = autoHideEnabled;
        SuggestionBorder.MaxWidth = overlayWidth;
        SuggestionTextBlock.FontSize = fontSize;
    }

    public void SetPromptType(PromptType promptType)
    {
        _currentPromptType = promptType;
        int promptValue = (int)promptType;

        // Update ComboBox selection
        foreach (ComboBoxItem item in PromptTypeComboBox.Items)
        {
            if (item.Tag is string tagStr && int.TryParse(tagStr, out int parsedTagValue) && parsedTagValue == promptValue)
            {
                PromptTypeComboBox.SelectedItem = item;
                break;
            }
            else if (item.Tag is int intTagValue && intTagValue == promptValue)
            {
                PromptTypeComboBox.SelectedItem = item;
                break;
            }
        }
    }

    public void PositionOverWindow(IntPtr targetWindow, RECT targetRect)
    {
        Left = targetRect.Left;
        Top = targetRect.Top;
        Width = targetRect.Right - targetRect.Left;
        Height = targetRect.Bottom - targetRect.Top;
    }

    public void ShowSuggestion(string text)
    {
        Console.WriteLine($"OverlayWindow.ShowSuggestion called with text length: {text.Length}");

        Dispatcher.Invoke(() =>
        {
            Console.WriteLine($"Setting suggestion text: {text}");
            SuggestionTextBlock.Text = text;
            MainGrid.Opacity = 0;

            if (!IsVisible)
                Show();

            // Fade in animation
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            MainGrid.BeginAnimation(OpacityProperty, fadeIn);

            // Reset auto-hide timer only if enabled
            if (_autoHideEnabled)
            {
                ResetAutoHideTimer();
            }
        });
    }

    public void HideSuggestion()
    {
        Dispatcher.Invoke(() =>
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            fadeOut.Completed += (s, e) =>
            {
                if (_autoHideTimer == null || !_autoHideTimer.IsEnabled)
                {
                    Hide();
                }
            };

            MainGrid.BeginAnimation(OpacityProperty, fadeOut);
        });
    }

    private void ResetAutoHideTimer()
    {
        _autoHideTimer?.Stop();
        _autoHideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(_autoHideSeconds)
        };
        _autoHideTimer.Tick += (s, e) =>
        {
            _autoHideTimer?.Stop();
            HideSuggestion();
        };
        _autoHideTimer.Start();
    }

    protected override void OnClosed(EventArgs e)
    {
        _autoHideTimer?.Stop();
        base.OnClosed(e);
    }

    // Window drag handlers
    private double _startX;
    private double _startY;
    private double _startLeft;
    private double _startTop;

    private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DragHandle.IsMouseOver)
        {
            // 获取当前窗口的句柄和位置
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            RECT rect;
            if (NativeMethods.GetWindowRect(hWnd, out rect))
            {
                _startLeft = rect.Left;
                _startTop = rect.Top;
            }
            else
            {
                _startLeft = Left;
                _startTop = Top;
            }

            // 获取鼠标相对于屏幕的位置
            Point startPoint = e.GetPosition(null);
            _startX = startPoint.X;
            _startY = startPoint.Y;

            Console.WriteLine($"MouseDown - Start Point: X={_startX}, Y={_startY}");
            Console.WriteLine($"Current Window Position: Left={_startLeft}, Top={_startTop}");
            Console.WriteLine($"Current Window Position (int): Left={(int)Math.Round(_startLeft)}, Top={(int)Math.Round(_startTop)}");

            this.CaptureMouse();
            e.Handled = true;
        }
    }

    private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (this.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
        {
            Point currentPoint = e.GetPosition(null);
            double currentX = currentPoint.X;
            double currentY = currentPoint.Y;

            double deltaX = currentX - _startX;
            double deltaY = currentY - _startY;

            // 直接使用整数坐标更新窗口位置，避免小数像素导致的抖动
            int newLeft = (int)Math.Round(_startLeft + deltaX);
            int newTop = (int)Math.Round(_startTop + deltaY);

            // 使用Windows API直接设置窗口位置
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            NativeMethods.SetWindowPos(
                hWnd,
                IntPtr.Zero,
                newLeft,
                newTop,
                0,
                0,
                NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE
            );

            Console.WriteLine($"MouseMove - Current Point: X={currentX}, Y={currentY}");
            Console.WriteLine($"MouseMove - Delta: X={deltaX}, Y={deltaY}");
            Console.WriteLine($"MouseMove - New Position: Left={newLeft}, Top={newTop}");
        }
    }

    private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (this.IsMouseCaptured)
        {
            Console.WriteLine($"MouseUp - Final Position: Left={Left}, Top={Top}");
            this.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (this.IsMouseCaptured)
        {
            this.ReleaseMouseCapture();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void PromptTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem comboBoxItem)
            {
                if (comboBoxItem.Tag is string tagStr && int.TryParse(tagStr, out int parsedTagValue))
                {
                    var promptType = (PromptType)parsedTagValue;
                    Console.WriteLine($"PromptTypeComboBox_SelectionChanged: Selected {promptType}");
                    CurrentPromptType = promptType;
                }
                else if (comboBoxItem.Tag is int intTagValue)
                {
                    var promptType = (PromptType)intTagValue;
                    Console.WriteLine($"PromptTypeComboBox_SelectionChanged: Selected {promptType}");
                    CurrentPromptType = promptType;
                }
                else
                {
                    Console.WriteLine($"PromptTypeComboBox_SelectionChanged: Tag is not a valid PromptType - {comboBoxItem.Tag}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PromptTypeComboBox_SelectionChanged Error: {ex}");
        }
    }
}
