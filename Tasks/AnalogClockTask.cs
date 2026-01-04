using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RasterGraphics.Exercises;

namespace RasterGraphics.Tasks;

public class AnalogClockTask : IExerciseInterface
{
    public WriteableBitmap Execute()
    {
        var dial = LoadImageAsWriteable("cifernik.png");
        var hourHand = LoadImageAsWriteable("hodinovka.png");
        var minuteHand = LoadImageAsWriteable("minutovka.png");
        var secondHand = LoadImageAsWriteable("sekundovka.png");

        var (hourAngle, minuteAngle, secondAngle) = CalculateClockAngles(8, 18, 35);

        var resultVram = new VRam(dial.PixelWidth, dial.PixelHeight);

        CopyImageToVRam(resultVram, dial);

        ComposeRotatedHand(resultVram, hourHand, hourAngle, dial.PixelWidth / 2.0, dial.PixelHeight / 2.0);
        ComposeRotatedHand(resultVram, minuteHand, minuteAngle, dial.PixelWidth / 2.0, dial.PixelHeight / 2.0);
        ComposeRotatedHand(resultVram, secondHand, secondAngle, dial.PixelWidth / 2.0, dial.PixelHeight / 2.0);

        SaveResult(resultVram.GetBitmap(), "clock_081835.png");

        return resultVram.GetBitmap();
    }

    /// <summary>
    /// Load image as WriteableBitmap
    /// </summary>
    private static WriteableBitmap LoadImageAsWriteable(string filename)
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Images", filename);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Image not found: {path}");
        }

        var bitmapImage = new BitmapImage(new Uri(path));
        var writeableBitmap = new WriteableBitmap(
            bitmapImage.PixelWidth,
            bitmapImage.PixelHeight,
            96, 96,
            PixelFormats.Pbgra32,
            null);

        int[] pixels = new int[bitmapImage.PixelWidth * bitmapImage.PixelHeight];
        int stride = bitmapImage.PixelWidth * 4;
        bitmapImage.CopyPixels(pixels, stride, 0);
        writeableBitmap.WritePixels(new Int32Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight), pixels, stride, 0);

        return writeableBitmap;
    }

    /// <summary>
    /// Copy WriteableBitmap to VRam
    /// </summary>
    private static void CopyImageToVRam(VRam vram, WriteableBitmap source)
    {
        int width = source.PixelWidth;
        int height = source.PixelHeight;

        int[] sourceData = new int[width * height];
        int stride = width * 4;
        source.CopyPixels(sourceData, stride, 0);

        int copyWidth = Math.Min(width, vram.Width);
        int copyHeight = Math.Min(height, vram.Height);

        Parallel.For(0, copyHeight, y =>
        {
            int srcRowOffset = y * width;
            int dstRowOffset = y * vram.Width;

            for (int x = 0; x < copyWidth; x++)
            {
                int sourceIdx = srcRowOffset + x;
                int destIdx = dstRowOffset + x;
                vram._rawData[destIdx] = sourceData[sourceIdx];
            }
        });
    }

    /// <summary>
    /// Calculate precise angles for clock hands at given time
    /// </summary>
    private static (double hourAngle, double minuteAngle, double secondAngle) CalculateClockAngles(int hours, int minutes, int seconds)
    {
        hours = hours % 12;

        double hourAngle = (hours * 30.0) + (minutes * 0.5) + (seconds * 0.008333);

        double minuteAngle = (minutes * 6.0) + (seconds * 0.1);

        double secondAngle = seconds * 6.0;

        return (hourAngle, minuteAngle, secondAngle);
    }

    /// <summary>
    /// Compose rotated hand onto dial with proper alpha blending
    /// </summary>
    private static void ComposeRotatedHand(VRam dialVram, WriteableBitmap handImage, double angleDegrees, double centerX, double centerY)
    {
        var rotatedHandVram = RotateImageAroundCenter(handImage, angleDegrees);

        int handWidth = rotatedHandVram.Width;
        int handHeight = rotatedHandVram.Height;
        int offsetX = (int)(centerX - handWidth / 2.0);
        int offsetY = (int)(centerY - handHeight / 2.0);

        AlphaBlendHand(dialVram, rotatedHandVram, offsetX, offsetY);
    }

    /// <summary>
    /// Rotate image around its center using bilinear interpolation
    /// </summary>
    private static VRam RotateImageAroundCenter(WriteableBitmap source, double angleDegrees)
    {
        double angleRad = angleDegrees * Math.PI / 180.0;
        int width = source.PixelWidth;
        int height = source.PixelHeight;
        double centerX = width / 2.0;
        double centerY = height / 2.0;

        var result = new VRam(width, height);

        int[] sourceData = new int[width * height];
        int stride = width * 4;
        source.CopyPixels(sourceData, stride, 0);

        double cosA = Math.Cos(angleRad);
        double sinA = Math.Sin(angleRad);

        Parallel.For(0, height, y =>
        {
            int rowOffset = y * width;
            int[] destData = result._rawData;

            for (int x = 0; x < width; x++)
            {
                double dx = x - centerX;
                double dy = y - centerY;

                double srcX = dx * cosA + dy * sinA + centerX;
                double srcY = -dx * sinA + dy * cosA + centerY;

                uint pixel = SampleBilinear(sourceData, width, height, srcX, srcY);
                destData[rowOffset + x] = (int)pixel;
            }
        });

        return result;
    }

    /// <summary>
    /// Bilinear interpolation for smooth rotation
    /// </summary>
    private static uint SampleBilinear(int[] imageData, int width, int height, double x, double y)
    {
        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        double fx = x - x0;
        double fy = y - y0;

        x0 = Math.Clamp(x0, 0, width - 1);
        y0 = Math.Clamp(y0, 0, height - 1);
        int x1 = Math.Min(x0 + 1, width - 1);
        int y1 = Math.Min(y0 + 1, height - 1);

        uint p00 = (uint)imageData[y0 * width + x0];
        uint p10 = (uint)imageData[y0 * width + x1];
        uint p01 = (uint)imageData[y1 * width + x0];
        uint p11 = (uint)imageData[y1 * width + x1];

        uint top = LerpColor(p00, p10, fx);
        uint bottom = LerpColor(p01, p11, fx);

        return LerpColor(top, bottom, fy);
    }

    /// <summary>
    /// Linear interpolation between two colors
    /// </summary>
    private static uint LerpColor(uint c1, uint c2, double t)
    {
        byte a1 = (byte)((c1 >> 24) & 0xFF);
        byte r1 = (byte)((c1 >> 16) & 0xFF);
        byte g1 = (byte)((c1 >> 8) & 0xFF);
        byte b1 = (byte)(c1 & 0xFF);

        byte a2 = (byte)((c2 >> 24) & 0xFF);
        byte r2 = (byte)((c2 >> 16) & 0xFF);
        byte g2 = (byte)((c2 >> 8) & 0xFF);
        byte b2 = (byte)(c2 & 0xFF);

        byte a = (byte)Math.Clamp(a1 + (a2 - a1) * t, 0, 255);
        byte r = (byte)Math.Clamp(r1 + (r2 - r1) * t, 0, 255);
        byte g = (byte)Math.Clamp(g1 + (g2 - g1) * t, 0, 255);
        byte b = (byte)Math.Clamp(b1 + (b2 - b1) * t, 0, 255);

        return (uint)((a << 24) | (r << 16) | (g << 8) | b);
    }

    /// <summary>
    /// Alpha blend hand onto dial at specified offset
    /// </summary>
    private static void AlphaBlendHand(VRam dialVram, VRam handVram, int offsetX, int offsetY)
    {
        int dialWidth = dialVram.Width;
        int dialHeight = dialVram.Height;
        int handWidth = handVram.Width;
        int handHeight = handVram.Height;

        Parallel.For(0, handHeight, handY =>
        {
            for (int handX = 0; handX < handWidth; handX++)
            {
                int dialX = offsetX + handX;
                int dialY = offsetY + handY;

                if (dialX >= 0 && dialX < dialWidth && dialY >= 0 && dialY < dialHeight)
                {
                    int handIdx = handY * handWidth + handX;
                    int dialIdx = dialY * dialWidth + dialX;

                    uint handPixel = (uint)handVram._rawData[handIdx];
                    uint dialPixel = (uint)dialVram._rawData[dialIdx];

                    if (((handPixel >> 24) & 0xFF) < 255)
                    {
                        dialVram._rawData[dialIdx] = AlphaBlend(dialPixel, handPixel);
                    }
                }
            }
        });
    }

    /// <summary>
    /// Alpha blending between background and foreground
    /// </summary>
    private static int AlphaBlend(uint background, uint foreground)
    {
        byte bgA = (byte)((background >> 24) & 0xFF);
        byte bgR = (byte)((background >> 16) & 0xFF);
        byte bgG = (byte)((background >> 8) & 0xFF);
        byte bgB = (byte)(background & 0xFF);

        byte fgA = (byte)((foreground >> 24) & 0xFF);
        byte fgR = (byte)((foreground >> 16) & 0xFF);
        byte fgG = (byte)((foreground >> 8) & 0xFF);
        byte fgB = (byte)(foreground & 0xFF);

        double alpha = fgA / 255.0;
        if (alpha == 0) return (int)background;

        byte r = (byte)Math.Clamp(bgR * (1 - alpha) + fgR * alpha, 0, 255);
        byte g = (byte)Math.Clamp(bgG * (1 - alpha) + fgG * alpha, 0, 255);
        byte b = (byte)Math.Clamp(bgB * (1 - alpha) + fgB * alpha, 0, 255);
        byte a = (byte)Math.Max(bgA, fgA);

        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    /// <summary>
    /// Save result as PNG
    /// </summary>
    private static void SaveResult(WriteableBitmap bitmap, string filename)
    {
        string outputDir = Path.Combine(Environment.CurrentDirectory, "Results");
        Directory.CreateDirectory(outputDir);

        string path = Path.Combine(outputDir, filename);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var stream = File.Create(path);
        encoder.Save(stream);

        Console.WriteLine($"Result saved: {path}");
    }
}
