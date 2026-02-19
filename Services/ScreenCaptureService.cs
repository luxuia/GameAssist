using System.Drawing;
using System.Drawing.Imaging;

namespace GameAssist.Services;

public class ScreenCaptureService
{
    public Bitmap? CaptureForegroundWindow()
    {
        IntPtr hWnd = NativeMethods.GetForegroundWindow();
        if (hWnd == IntPtr.Zero) return null;

        return CaptureWindow(hWnd);
    }

    public Bitmap? CaptureWindow(IntPtr hWnd)
    {
        if (!NativeMethods.GetWindowRect(hWnd, out RECT rect))
            return null;

        int width = Math.Max(1, rect.Right - rect.Left);
        int height = Math.Max(1, rect.Bottom - rect.Top);

        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
        }

        return bmp;
    }

    public byte[] ConvertBitmapToBytes(Bitmap bmp, ImageFormat format)
    {
        using var ms = new System.IO.MemoryStream();
        bmp.Save(ms, format);
        return ms.ToArray();
    }
}
