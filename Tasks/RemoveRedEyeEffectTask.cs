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
            RgbColor rgb = new(argb);
            HslColor hsl = rgb.ToHsl();

            // Step 2: Context-free red-eye detection (based only on THIS pixel)
            // Red-eye criteria:
            // - Hue in red range (0-40° or 330-360°)
            // - Saturation above a threshold (vivid color, not grayish)
            // - Red channel dominant in RGB
            // - Minimum red brightness
            bool isRedEye = (hsl.H >= 0 && hsl.H <= 40 || hsl.H >= 330) &&
                            hsl.S > saturationThreshold &&
                            hsl.L > 0.1 && hsl.L < 0.9 &&
                            rgb.R > rgb.G && rgb.R > rgb.B &&
                            rgb.R > minRedValue;

            if (isRedEye)
            {
                // Step 3: ADAPTIVE Hue shift transformation
                // Calculate an adaptation factor based on original color intensity
                // More saturated and brighter red → darker blue (lower hue)
                // Less saturated or darker red → lighter blue/cyan (higher hue)
                double adaptationFactor = Math.Clamp(hsl.S * hsl.L * 0.85, 0.0, 1.0);

                // EXPERIMENTS with different target hues:
                // Base: 200° (blue-cyan) for neutral red
                // Range: 140° (darker blue) to 220° (lighter cyan)
                // Adaptation: Brighter/more saturated red → darker blue
                const double baseHue = 200.0;
                const double hueRange = 40.0; // Range of variation
                double newHue = baseHue - adaptationFactor * hueRange;
                // Result: 200° - (0..1 * 60) = 200° down to 140°

                // Also adapt saturation: brighter red → more saturated blue
                double newSaturation = Math.Clamp(hsl.S * (0.75 + adaptationFactor * 0.40), 0.2, 0.95);

                // Keep original lightness to preserve the brightness gradient
                double newLightness = hsl.L * 0.4875;

                // Step 4: HSL → RGB conversion
                HslColor corrected = new(newHue, newSaturation, newLightness, hsl.A);
                RgbColor correctedRgb = RgbColor.FromHsl(corrected);

                data[i] = correctedRgb.ToArgb();
            }
        });
    }
}
