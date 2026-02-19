using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GameAssist.Views;

public partial class OverlayWindow : Window
{
    private DispatcherTimer? _autoHideTimer;
    private double _fontSize = 16;
    private int _autoHideSeconds = 30;
    private bool _isDragging = false;
    private Point _dragStartPoint;
    private Point _windowStartPoint;

    public OverlayWindow()
    {
        InitializeComponent();
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
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Only start dragging if left button is clicked on the drag area
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            _windowStartPoint = new Point(Left, Top);
            this.CaptureMouse();
        }
    }

    private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // If click is on the drag handle, start dragging
        if (DragHandle.IsMouseOver)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            _windowStartPoint = new Point(Left, Top);
            this.CaptureMouse();
            e.Handled = true;
        }
    }

    private void DragHandle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Handle drag handle click
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            _windowStartPoint = new Point(Left, Top);
            this.CaptureMouse();
            e.Handled = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isDragging)
        {
            Point currentPoint = e.GetPosition(this);
            double deltaX = currentPoint.X - _dragStartPoint.X;
            double deltaY = currentPoint.Y - _dragStartPoint.Y;

            Left = _windowStartPoint.X + deltaX;
            Top = _windowStartPoint.Y + deltaY;
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (_isDragging)
        {
            _isDragging = false;
            this.ReleaseMouseCapture();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
