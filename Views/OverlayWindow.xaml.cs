using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GameAssist.Views;

public partial class OverlayWindow : Window
{
    private DispatcherTimer? _autoHideTimer;
    private double _fontSize = 16;
    private int _autoHideSeconds = 30;

    public OverlayWindow()
    {
        InitializeComponent();
        // 显示测试文本，方便调试拖动功能
        SuggestionTextBlock.Text = "这是一个测试文本，用于调试浮动窗口的拖动功能。\n\n您可以尝试拖动此窗口来测试是否还有抖动问题。\n\n如果您看到此文本，说明窗口已经正确显示。";
        MainGrid.Opacity = 1;
    }

    public void Configure(int overlayWidth, int fontSize, int autoHideSeconds)
    {
        _fontSize = fontSize;
        _autoHideSeconds = autoHideSeconds;
        SuggestionBorder.MaxWidth = overlayWidth;
        SuggestionTextBlock.FontSize = fontSize;
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
        Dispatcher.Invoke(() =>
        {
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

            // Reset auto-hide timer
            ResetAutoHideTimer();
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
            // 获取当前窗口的位置
            _startLeft = Left;
            _startTop = Top;

            Point startPoint = e.GetPosition(null);
            _startX = startPoint.X;
            _startY = startPoint.Y;

            Console.WriteLine($"MouseDown - Start Point: X={_startX}, Y={_startY}");
            Console.WriteLine($"Current Window Position: Left={_startLeft}, Top={_startTop}");
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

            // 只有当偏移量超过一定阈值时才更新窗口位置，避免微小偏移导致的抖动
            const double movementThreshold = 1.0;
            if (Math.Abs(deltaX) > movementThreshold || Math.Abs(deltaY) > movementThreshold)
            {
                double newLeft = _startLeft + deltaX;
                double newTop = _startTop + deltaY;

                // 使用WPF的属性设置窗口位置，而不是Windows API，这样可以使用浮点数坐标
                Left = newLeft;
                Top = newTop;

                Console.WriteLine($"MouseMove - Current Point: X={currentX}, Y={currentY}");
                Console.WriteLine($"MouseMove - Delta: X={deltaX}, Y={deltaY}");
                Console.WriteLine($"MouseMove - New Position: Left={newLeft}, Top={newTop}");

                // 更新起始位置，使下一次移动基于当前位置计算偏移量
                _startLeft = newLeft;
                _startTop = newTop;
                _startX = currentX;
                _startY = currentY;
            }
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
}
