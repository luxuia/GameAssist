using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace GameAssist;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private bool _isRunning;
    private string _statusText = "Ready";
    private bool _isDota2Active;
    private string? _lastSuggestion;
    private BitmapImage? _previewImage;
    private int _captureCount;
    private DateTime _lastCaptureTime;

    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsStopped)); }
    }

    public bool IsStopped => !_isRunning;

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public bool IsDota2Active
    {
        get => _isDota2Active;
        set { _isDota2Active = value; OnPropertyChanged(); }
    }

    public string? LastSuggestion
    {
        get => _lastSuggestion;
        set { _lastSuggestion = value; OnPropertyChanged(); }
    }

    public BitmapImage? PreviewImage
    {
        get => _previewImage;
        set { _previewImage = value; OnPropertyChanged(); }
    }

    public int CaptureCount
    {
        get => _captureCount;
        set { _captureCount = value; OnPropertyChanged(); }
    }

    public DateTime LastCaptureTime
    {
        get => _lastCaptureTime;
        set { _lastCaptureTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(LastCaptureTimeDisplay)); }
    }

    public string LastCaptureTimeDisplay => _lastCaptureTime == default ? "Never" : _lastCaptureTime.ToString("HH:mm:ss");

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateCaptureInfo()
    {
        CaptureCount++;
        LastCaptureTime = DateTime.Now;
    }
}
