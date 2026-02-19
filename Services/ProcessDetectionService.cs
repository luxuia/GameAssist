using System.Diagnostics;

namespace GameAssist.Services;

public class ProcessDetectionService
{
    private const string Dota2ProcessName = "dota2";

    public bool IsDota2Running()
    {
        return Process.GetProcessesByName(Dota2ProcessName).Length > 0;
    }

    public bool IsDota2Active()
    {
        try
        {
            var foregroundWindow = NativeMethods.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return false;

            NativeMethods.GetWindowThreadProcessId(foregroundWindow, out uint processId);
            var process = Process.GetProcessById((int)processId);
            return string.Equals(process.ProcessName, Dota2ProcessName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public bool IsDota2Window(IntPtr hWnd)
    {
        try
        {
            if (hWnd == IntPtr.Zero)
                return false;

            NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);
            var process = Process.GetProcessById((int)processId);
            return string.Equals(process.ProcessName, Dota2ProcessName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
