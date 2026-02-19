using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace GameAssist
{
    public class TestImageCreator
    {
        public static byte[] CreateTestImage()
        {
            // Create a simple 1920x1080 image for testing
            using (var bitmap = new Bitmap(1920, 1080, PixelFormat.Format24bppRgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // Fill with background color
                    graphics.FillRectangle(Brushes.Black, 0, 0, 1920, 1080);

                    // Add a message
                    using (var font = new Font("Arial", 24))
                    {
                        graphics.DrawString("Dota 2 Test Screenshot", font, Brushes.White, 50, 50);
                        graphics.DrawString("GLM-4V API Test", font, Brushes.Yellow, 50, 100);
                    }
                }

                // Convert to byte array
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return stream.ToArray();
                }
            }
        }

        public static void SaveTestImage(string filePath)
        {
            byte[] imageBytes = CreateTestImage();
            File.WriteAllBytes(filePath, imageBytes);
            Console.WriteLine($"Test image saved to: {filePath}");
        }
    }
}