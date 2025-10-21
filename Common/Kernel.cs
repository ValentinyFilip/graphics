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
}
