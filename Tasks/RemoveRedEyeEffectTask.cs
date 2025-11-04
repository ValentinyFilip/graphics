using System.Windows.Media.Imaging;
using RasterGraphics.Common;
using RasterGraphics.Exercises;

namespace RasterGraphics.Tasks;

public class RemoveRedEyeEffectTask(int width, int height) : IExerciseInterface
{
    public WriteableBitmap Execute()
    {
        var vram = new VRam(width, height);

        return vram.GetBitmap();
    }

    public static void RemoveRedEyes(VRam vram, float saturationThreshold = 0.3f, int minRedValue = 50)
    {
        int[] data = vram._rawData;

        Parallel.For(0, data.Length, i =>
        {
            int argb = data[i];

            // Step 1: RGB → HSL conversion
            RgbColor rgb = new RgbColor(argb);
            HslColor hsl = rgb.ToHsl();

            // Step 2: Context-free red-eye detection (based only on THIS pixel)
            // Red-eye criteria:
            // - Hue in red range (0-40° or 330-360°)
            // - Saturation above threshold (vivid color, not grayish)
            // - Red channel dominant in RGB
            // - Minimum red brightness
            bool isRedEye = (hsl.H >= 0 && hsl.H <= 40 || hsl.H >= 330) &&
                            hsl.S > saturationThreshold &&
                            hsl.L > 0.1 && hsl.L < 0.9 &&
                            rgb.R > rgb.G && rgb.R > rgb.B &&
                            rgb.R > minRedValue;

            if (isRedEye)
            {
                // Step 3: Hue shift transformation
                // EXPERIMENTS with different target hues:
                // - 200° (blue-cyan): Natural for blue eyes, complementary to red
                // - 210° (cyan): Lighter, cooler tone
                // - 30-60° (amber): For brown eyes
                // CHOSEN: 200° provides most natural result
                double newHue = 200;

                // Slightly reduce saturation to avoid artificial appearance
                double newSaturation = hsl.S * 0.85;

                // Keep original lightness to preserve brightness gradient
                double newLightness = hsl.L;

                // Step 4: HSL → RGB conversion
                HslColor corrected = new HslColor(newHue, newSaturation, newLightness, hsl.A);
                RgbColor correctedRgb = RgbColor.FromHsl(corrected);

                data[i] = correctedRgb.ToArgb();
            }
        });
    }
}
