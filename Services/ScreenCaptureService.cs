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

    /// <summary>
    /// 将图片转换为字节数组，可选择启用压缩
    /// </summary>
    /// <param name="bmp">位图对象</param>
    /// <param name="format">图片格式</param>
    /// <param name="enableCompression">是否启用压缩</param>
    /// <param name="quality">压缩质量 (0-100)</param>
    /// <param name="maxSizeKB">最大图片大小 (KB)，压缩到该大小以下</param>
    /// <returns>图片字节数组</returns>
    public byte[] ConvertBitmapToBytes(Bitmap bmp, ImageFormat format, bool enableCompression = false, int quality = 80, long maxSizeKB = 2000)
    {
        using var ms = new System.IO.MemoryStream();

        if (!enableCompression || format != ImageFormat.Jpeg)
        {
            bmp.Save(ms, format);
            return ms.ToArray();
        }

        // 获取 JPEG 编码器
        var jpegCodec = GetEncoderInfo("image/jpeg");
        if (jpegCodec == null)
        {
            bmp.Save(ms, format);
            return ms.ToArray();
        }

        // 创建编码参数
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

        bmp.Save(ms, jpegCodec, encoderParams);
        var result = ms.ToArray();

        // 如果超过最大大小，逐步降低质量直到满足要求
        if (result.Length > maxSizeKB * 1024 && quality > 10)
        {
            var currentQuality = quality;
            while (result.Length > maxSizeKB * 1024 && currentQuality > 10)
            {
                currentQuality -= 10;
                ms.SetLength(0);
                ms.Position = 0;
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)currentQuality);
                bmp.Save(ms, jpegCodec, encoderParams);
                result = ms.ToArray();
            }
        }

        return result;
    }

    private ImageCodecInfo? GetEncoderInfo(string mimeType)
    {
        var codecs = ImageCodecInfo.GetImageEncoders();
        foreach (var codec in codecs)
        {
            if (codec.MimeType == mimeType)
            {
                return codec;
            }
        }
        return null;
    }
}
