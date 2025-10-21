using System.Windows.Media;
using System.Windows.Media.Imaging;
using RasterGraphics.Common;

namespace RasterGraphics;

public class VRam(int width = 255, int height = 255)
{
    internal readonly int[,] _rawData = new int[height, width];
    public int Width { get; } = width;
    public int Height { get; } = height;

    public int GetPixel(int x, int y) => _rawData[y, x];

    public void SetPixel(int x, int y, int r, int g, int b) =>
        _rawData[y, x] = (255 << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) | (b & 0xFF);

    public void SetPixel(int x, int y, int r, int g, int b, int a) =>
        _rawData[y, x] = ((a & 0xFF) << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) | (b & 0xFF);

    // RGB struct overload
    public void SetPixel(int x, int y, RgbColor rgb) =>
        _rawData[y, x] = (rgb.A << 24) | (rgb.R << 16) | (rgb.G << 8) | rgb.B;

    // HSL struct overload - converts HSL to RGB first
    public void SetPixel(int x, int y, HslColor hsl)
    {
        var (r, g, b) = HslToRgb(hsl);
        _rawData[y, x] = ((byte)(hsl.A * 255) << 24) | (r << 16) | (g << 8) | b;
    }

    private static (byte r, byte g, byte b) HslToRgb(HslColor hsl)
    {
        byte r, g, b;

        if (hsl.S == 0)
        {
            // Achromatic (gray)
            r = g = b = (byte)(hsl.L * 255);
        }
        else
        {
            float v1, v2;
            float hue = (float)(hsl.H / 360.0);

            float s = (float)hsl.S;
            float l = (float)hsl.L;

            v2 = (l < 0.5)
                ? (l * (1 + s))
                : ((l + s) - (l * s));

            v1 = 2 * l - v2;

            r = (byte)(255 * HueToRgb(v1, v2, hue + (1.0f / 3)));
            g = (byte)(255 * HueToRgb(v1, v2, hue));
            b = (byte)(255 * HueToRgb(v1, v2, hue - (1.0f / 3)));
        }

        return (r, g, b);
    }

    private static float HueToRgb(float v1, float v2, float vH)
    {
        if (vH < 0) vH += 1;
        if (vH > 1) vH -= 1;

        if ((6 * vH) < 1) return v1 + (v2 - v1) * 6 * vH;
        if ((2 * vH) < 1) return v2;
        if ((3 * vH) < 2) return v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6;

        return v1;
    }

    public WriteableBitmap GetBitmap()
    {
        WriteableBitmap bmp = new(Width, Height, 96, 96, PixelFormats.Bgra32, null);
        byte[] pixels = new byte[Width * Height * 4];
        int i = 0;
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                int argb = _rawData[y, x];
                pixels[i++] = (byte)argb; // Blue
                pixels[i++] = (byte)(argb >> 8); // Green
                pixels[i++] = (byte)(argb >> 16); // Red
                pixels[i++] = (byte)(argb >> 24); // Alpha
            }
        }

        bmp.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), pixels, Width * 4, 0);
        return bmp;
    }

    public void LoadFromBitmap(BitmapSource bitmap)
    {
        // Ensure the bitmap matches our VRAM dimensions
        if (bitmap.PixelWidth != width || bitmap.PixelHeight != height)
        {
            // Optionally resize the bitmap or throw an exception
            // For now, we'll scale it to fit VRAM size
            bitmap = new TransformedBitmap(bitmap, new ScaleTransform(
                (double)width / bitmap.PixelWidth,
                (double)height / bitmap.PixelHeight));
        }

        // Convert to Bgra32 format for easy processing
        FormatConvertedBitmap convertedBitmap = new(bitmap, PixelFormats.Bgra32, null, 0);

        int stride = width * 4;
        byte[] pixels = new byte[height * stride];

        // Copy pixel data from bitmap
        convertedBitmap.CopyPixels(pixels, stride, 0);

        // Convert from BGRA byte array to ARGB int array in rawData
        int i = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte b = pixels[i++];
                byte g = pixels[i++];
                byte r = pixels[i++];
                byte a = pixels[i++];

                _rawData[y, x] = (a << 24) | (r << 16) | (g << 8) | b;
            }
        }
    }

    public void CopyFrom(VRam source)
    {
        if (source.Width != Width || source.Height != Height)
        {
            throw new ArgumentException("Source VRAM dimensions must match.");
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _rawData[y, x] = source._rawData[y, x];
            }
        }
    }
}
