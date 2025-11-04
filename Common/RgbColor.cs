namespace RasterGraphics.Common;

public readonly struct RgbColor
{
    public RgbColor(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public RgbColor(int argb)
    {
        A = (byte)((argb >> 24) & 0xFF);
        R = (byte)((argb >> 16) & 0xFF);
        G = (byte)((argb >> 8) & 0xFF);
        B = (byte)(argb & 0xFF);
    }

    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }

    public int ToArgb() => (A << 24) | (R << 16) | (G << 8) | B;

    // Convert RGB to HSL
    public HslColor ToHsl()
    {
        double r = R / 255.0;
        double g = G / 255.0;
        double b = B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double h = 0, s = 0, l = (max + min) / 2.0;

        if (delta > double.Epsilon) // Use epsilon instead of exact 0
        {
            s = l < 0.5 ? delta / (max + min) : delta / (2.0 - max - min);

            if (Math.Abs(max - r) < 0.00001)
                h = ((g - b) / delta + (g < b ? 6 : 0)) / 6.0;
            else if (Math.Abs(max - g) < 0.00001)
                h = ((b - r) / delta + 2) / 6.0;
            else
                h = ((r - g) / delta + 4) / 6.0;

            h *= 360.0;
        }

        return new HslColor(h, s, l, A / 255.0);
    }

    // Create RGB from HSL - FIXED VERSION
    public static RgbColor FromHsl(HslColor hsl)
    {
        double r, g, b;

        if (hsl.S < double.Epsilon) // Grayscale
        {
            r = g = b = hsl.L;
        }
        else
        {
            double q = hsl.L < 0.5 ? hsl.L * (1 + hsl.S) : hsl.L + hsl.S - hsl.L * hsl.S;
            double p = 2 * hsl.L - q;
            double hk = hsl.H / 360.0;

            r = HueToRgb(p, q, hk + 1.0 / 3.0);
            g = HueToRgb(p, q, hk);
            b = HueToRgb(p, q, hk - 1.0 / 3.0);
        }

        return new RgbColor(
            (byte)Math.Clamp((int)(r * 255 + 0.5), 0, 255),
            (byte)Math.Clamp((int)(g * 255 + 0.5), 0, 255),
            (byte)Math.Clamp((int)(b * 255 + 0.5), 0, 255),
            (byte)(hsl.A * 255)
        );
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }
}
