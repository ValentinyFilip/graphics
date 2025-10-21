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
        for (int i = 0; i < vram.Height; i++)
        {
            for (int j = 0; j < vram.Width; j++)
            {
                RgbColor pixel = new(vram.GetPixel(j, i));

                byte gray = (byte)(pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f);

                vram.SetPixel(j, i, new RgbColor(gray, gray, gray));
            }
        }
    }

    public static void SaturateImage(VRam vram, float ratio)
    {
        for (int i = 0; i < vram.Height; i++)
        {
            for (int j = 0; j < vram.Width; j++)
            {
                HslColor pixel = new(vram.GetPixel(j, i));

                double saturation = Math.Clamp(pixel.S * ratio, 0.0f, 1.0f);

                vram.SetPixel(j, i, new HslColor(pixel.H, saturation, pixel.L));
            }
        }
    }

    public static void HueShift(VRam vram, float shift)
    {
        for (int i = 0; i < vram.Height; i++)
        {
            for (int j = 0; j < vram.Width; j++)
            {
                HslColor pixel = new(vram.GetPixel(j, i));

                vram.SetPixel(j, i, new HslColor(pixel.H + shift, pixel.S, pixel.L));
            }
        }
    }
}
