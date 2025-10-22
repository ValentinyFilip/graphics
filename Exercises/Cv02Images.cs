using System.Windows.Media.Imaging;
using RasterGraphics.Common;

namespace RasterGraphics.Exercises;

public class Cv02Images(int width, int height) : IExerciseInterface
{
    public WriteableBitmap Execute()
    {
        var vram = new VRam(width, height);

        return vram.GetBitmap();
    }

    public static void GrayScale(VRam vram)
    {
        int[] data = vram._rawData;

        Parallel.For(0, data.Length, i =>
        {
            int argb = data[i];
            int r = (argb >> 16) & 0xFF;
            int g = (argb >> 8) & 0xFF;
            int b = argb & 0xFF;
            int gray = (77 * r + 150 * g + 29 * b) >> 8;
            data[i] = (255 << 24) | (gray << 16) | (gray << 8) | gray;
        });
    }

    public static void SaturateImage(VRam vram, float ratio)
    {
        int[] data = vram._rawData;

        Parallel.For(0, data.Length, i =>
        {
            HslColor hsl = new(data[i]);
            double newS = Math.Clamp(hsl.S * ratio, 0.0, 1.0);
            HslColor adjusted = new(hsl.H, newS, hsl.L, hsl.A);
            data[i] = HslToArgb(adjusted);
        });
    }

    public static void HueShift(VRam vram, float shift)
    {
        int[] data = vram._rawData;

        Parallel.For(0, data.Length, i =>
        {
            HslColor hsl = new(data[i]);
            double newH = (hsl.H + shift) % 360.0;
            if (newH < 0) newH += 360.0;
            HslColor adjusted = new(newH, hsl.S, hsl.L, hsl.A);
            data[i] = HslToArgb(adjusted);
        });
    }

    private static int HslToArgb(HslColor hsl)
    {
        int r, g, b;

        if (hsl.S == 0)
        {
            // Achromatic (gray)
            r = g = b = (int)(hsl.L * 255);
        }
        else
        {
            double v2 = hsl.L < 0.5 ? hsl.L * (1 + hsl.S) : (hsl.L + hsl.S) - (hsl.L * hsl.S);
            double v1 = 2 * hsl.L - v2;
            double h = hsl.H / 360.0;

            r = (int)(255 * HueToRgb(v1, v2, h + (1.0 / 3.0)));
            g = (int)(255 * HueToRgb(v1, v2, h));
            b = (int)(255 * HueToRgb(v1, v2, h - (1.0 / 3.0)));
        }

        int a = (int)(hsl.A * 255);
        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    private static double HueToRgb(double v1, double v2, double vH)
    {
        if (vH < 0) vH += 1;
        if (vH > 1) vH -= 1;
        if ((6 * vH) < 1) return v1 + (v2 - v1) * 6 * vH;
        if ((2 * vH) < 1) return v2;
        if ((3 * vH) < 2) return v1 + (v2 - v1) * ((2.0 / 3.0) - vH) * 6;
        return v1;
    }
}
