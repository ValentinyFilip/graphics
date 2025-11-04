namespace RasterGraphics.Common;

public readonly struct Kernel(int[,] data)
{
    public int Width { get; } = data.GetLength(1);
    public int Height { get; } = data.GetLength(0);

    public int this[int y, int x] => Data[y, x];
    public readonly int[,] Data = data;

    public static Kernel BoxBlur3X3 => new(new int[,] {
        { 1, 1, 1 },
        { 1, 1, 1 },
        { 1, 1, 1 }
    });

    public static Kernel EdgeDetect => new(new int[,] {
        { -1, -1, -1 },
        { -1,  8, -1 },
        { -1, -1, -1 }
    });

    public static Kernel GaussianBlur3x3 => new(new int[,] {
        { 1, 2, 1 },
        { 2, 4, 2 },
        { 1, 2, 1 }
    });

    public static Kernel GaussianBlur5x5 => new(new int[,] {
        { 1,  4,  6,  4, 1 },
        { 4, 16, 24, 16, 4 },
        { 6, 24, 36, 24, 6 },
        { 4, 16, 24, 16, 4 },
        { 1,  4,  6,  4, 1 }
    });
}
