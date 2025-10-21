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
}
