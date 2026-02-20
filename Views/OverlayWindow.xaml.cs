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

    private int _integerLeft;
    private int _integerTop;

    public new double Left
    {
        get => base.Left;
        set
        {
            int intValue = (int)Math.Round(value);
            if (intValue != _integerLeft)
            {
                _integerLeft = intValue;
                base.Left = _integerLeft;
            }
        }
    }

    public new double Top
    {
        get => base.Top;
        set
        {
            int intValue = (int)Math.Round(value);
            if (intValue != _integerTop)
            {
                _integerTop = intValue;
                base.Top = _integerTop;
            }
        }
    }

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
    private int _startX;
    private int _startY;
    private int _startLeft;
    private int _startTop;

    private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DragHandle.IsMouseOver)
        {
            // 获取当前窗口的句柄和位置
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            if (NativeMethods.GetWindowRect(hWnd, out RECT rect))
            {
                _startLeft = rect.Left;
                _startTop = rect.Top;
            }

            Point startPoint = e.GetPosition(null);
            _startX = (int)Math.Round(startPoint.X);
            _startY = (int)Math.Round(startPoint.Y);

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
            int currentX = (int)Math.Round(currentPoint.X);
            int currentY = (int)Math.Round(currentPoint.Y);

            int deltaX = currentX - _startX;
            int deltaY = currentY - _startY;

            int newLeft = _startLeft + deltaX;
            int newTop = _startTop + deltaY;

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
}
