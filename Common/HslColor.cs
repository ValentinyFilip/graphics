namespace RasterGraphics.Common;

public readonly struct HslColor
{
    public HslColor(double h, double s, double l, double a = 1.0)
    {
        H = h % 360.0;
        S = Math.Clamp(s, 0.0, 1.0);
        L = Math.Clamp(l, 0.0, 1.0);
        A = Math.Clamp(a, 0.0, 1.0);
    }

    public HslColor(int argb)
    {
        // Extract RGB bytes
        byte a = (byte)((argb >> 24) & 0xFF);
        byte r = (byte)((argb >> 16) & 0xFF);
        byte g = (byte)((argb >> 8) & 0xFF);
        byte b = (byte)(argb & 0xFF);

        // Convert to [0, 1] range
        double rNorm = r / 255.0;
        double gNorm = g / 255.0;
        double bNorm = b / 255.0;

        // Find min and max
        double max = Math.Max(Math.Max(rNorm, gNorm), bNorm);
        double min = Math.Min(Math.Min(rNorm, gNorm), bNorm);
        double delta = max - min;

        // Calculate Lightness
        L = (max + min) / 2.0;

        if (delta < float.Epsilon) // Use epsilon comparison instead of == 0
        {
            // Achromatic (gray)
            H = 0;
            S = 0;
        }
        else
        {
            // Calculate Saturation
            S = (L > 0.5) ? delta / (2.0 - max - min) : delta / (max + min);

            // Calculate Hue
            double hue;
            if (Math.Abs(max - rNorm) < float.Epsilon) // Epsilon comparison
            {
                hue = (gNorm - bNorm) / delta + (gNorm < bNorm ? 6 : 0);
            }
            else if (Math.Abs(max - gNorm) < float.Epsilon) // Epsilon comparison
            {
                hue = (bNorm - rNorm) / delta + 2.0;
            }
            else // max == bNorm
            {
                hue = (rNorm - gNorm) / delta + 4.0;
            }

            H = (hue / 6.0) * 360.0;
        }

        A = a / 255.0;
    }


    public double H { get; } // Hue: 0-360
    public double S { get; } // Saturation: 0-1
    public double L { get; } // Lightness: 0-1
    public double A { get; } // Alpha: 0-1
}
