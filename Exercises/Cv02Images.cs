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
            int argb = data[i];
            int r = (argb >> 16) & 0xFF;
            int g = (argb >> 8) & 0xFF;
            int b = argb & 0xFF;

            // RGB to HSL
            float rf = r / 255.0f;
            float gf = g / 255.0f;
            float bf = b / 255.0f;

            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float delta = max - min;

            float h = 0, s = 0, l = (max + min) / 2.0f;

            if (delta != 0)
            {
                s = l < 0.5f ? delta / (max + min) : delta / (2.0f - max - min);

                if (max == rf)
                    h = ((gf - bf) / delta + (gf < bf ? 6 : 0)) / 6.0f;
                else if (max == gf)
                    h = ((bf - rf) / delta + 2) / 6.0f;
                else
                    h = ((rf - gf) / delta + 4) / 6.0f;
            }

            // Apply saturation ratio
            s = Math.Clamp(s * ratio, 0.0f, 1.0f);

            // HSL to RGB
            int newR, newG, newB;
            if (s == 0)
            {
                // Achromatic (gray)
                newR = newG = newB = (int)(l * 255);
            }
            else
            {
                float v2 = l < 0.5f ? l * (1 + s) : (l + s) - (l * s);
                float v1 = 2 * l - v2;

                newR = (int)(255 * HueToRgb(v1, v2, h + (1.0f / 3.0f)));
                newG = (int)(255 * HueToRgb(v1, v2, h));
                newB = (int)(255 * HueToRgb(v1, v2, h - (1.0f / 3.0f)));
            }

            data[i] = (255 << 24) | (newR << 16) | (newG << 8) | newB;
        });
    }

    public static void HueShift(VRam vram, float shift)
    {
        int[] data = vram._rawData;

        Parallel.For(0, data.Length, i =>
        {
            int argb = data[i];
            int r = (argb >> 16) & 0xFF;
            int g = (argb >> 8) & 0xFF;
            int b = argb & 0xFF;

            // RGB to HSL
            float rf = r / 255.0f;
            float gf = g / 255.0f;
            float bf = b / 255.0f;

            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float delta = max - min;

            float h = 0, s = 0, l = (max + min) / 2.0f;

            if (delta != 0)
            {
                s = l < 0.5f ? delta / (max + min) : delta / (2.0f - max - min);

                if (max == rf)
                    h = ((gf - bf) / delta + (gf < bf ? 6 : 0)) / 6.0f;
                else if (max == gf)
                    h = ((bf - rf) / delta + 2) / 6.0f;
                else
                    h = ((rf - gf) / delta + 4) / 6.0f;
            }

            // Apply hue shift and wrap around 0-1
            h = (h + shift / 360.0f) % 1.0f;
            if (h < 0) h += 1.0f;

            // HSL to RGB
            int newR, newG, newB;
            if (s == 0)
            {
                newR = newG = newB = (int)(l * 255);
            }
            else
            {
                float v2 = l < 0.5f ? l * (1 + s) : (l + s) - (l * s);
                float v1 = 2 * l - v2;

                newR = (int)(255 * HueToRgb(v1, v2, h + (1.0f / 3.0f)));
                newG = (int)(255 * HueToRgb(v1, v2, h));
                newB = (int)(255 * HueToRgb(v1, v2, h - (1.0f / 3.0f)));
            }

            data[i] = (255 << 24) | (newR << 16) | (newG << 8) | newB;
        });
    }

    private static float HueToRgb(float v1, float v2, float vH)
    {
        if (vH < 0) vH += 1;
        if (vH > 1) vH -= 1;
        if ((6 * vH) < 1) return v1 + (v2 - v1) * 6 * vH;
        if ((2 * vH) < 1) return v2;
        if ((3 * vH) < 2) return v1 + (v2 - v1) * ((2.0f / 3.0f) - vH) * 6;
        return v1;
    }
}
