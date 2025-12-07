using System.Windows.Media.Imaging;

namespace RasterGraphics.Exercises;

public class Cv04_Compression(int width, int height) : IExerciseInterface
{
    public WriteableBitmap Execute()
    {
        var vram = new VRam(width, height);
        return vram.GetBitmap();
    }

    /// <summary>
    /// RLE (Run-Length Encoding) compression for grayscale image.
    /// Returns list of (count, value) pairs.
    /// </summary>
    public static List<(byte Count, byte Value)> CompressRle(byte[] grayscaleData)
    {
        var result = new List<(byte Count, byte Value)>();
        if (grayscaleData.Length == 0)
            return result;

        byte currentValue = grayscaleData[0];
        byte count = 1;

        for (int i = 1; i < grayscaleData.Length; i++)
        {
            byte v = grayscaleData[i];

            if (v == currentValue && count < byte.MaxValue)
            {
                count++;
            }
            else
            {
                result.Add((count, currentValue));
                currentValue = v;
                count = 1;
            }
        }

        // Add last run
        result.Add((count, currentValue));

        return result;
    }

    /// <summary>
    /// Decompress RLE data back to original grayscale array.
    /// </summary>
    public static byte[] DecompressRle(List<(byte Count, byte Value)> compressed, int expectedLength)
    {
        var output = new byte[expectedLength];
        int index = 0;

        foreach (var (count, value) in compressed)
        {
            for (int i = 0; i < count; i++)
            {
                if (index >= expectedLength)
                    break;
                output[index++] = value;
            }
        }

        return output;
    }

    /// <summary>
    /// Convert VRam to grayscale byte array (0-255).
    /// Uses standard luminance formula: Y = 0.299*R + 0.587*G + 0.114*B
    /// </summary>
    public static byte[] ToGrayscaleBuffer(VRam vram)
    {
        int[] data = vram._rawData;
        byte[] gray = new byte[data.Length];

        Parallel.For(0, data.Length, i =>
        {
            int argb = data[i];
            int r = (argb >> 16) & 0xFF;
            int g = (argb >> 8) & 0xFF;
            int b = argb & 0xFF;

            int y = (int)(0.299 * r + 0.587 * g + 0.114 * b);
            gray[i] = (byte)Math.Clamp(y, 0, 255);
        });

        return gray;
    }

    /// <summary>
    /// Convert grayscale byte array back to VRam (as grayscale RGB image).
    /// </summary>
    public static void FromGrayscaleBuffer(VRam vram, byte[] gray)
    {
        int[] data = vram._rawData;

        Parallel.For(0, data.Length, i =>
        {
            byte g = gray[i];
            int argb = (255 << 24) | (g << 16) | (g << 8) | g;
            data[i] = argb;
        });
    }

    /// <summary>
    /// Simple lossy quantization: reduce gray levels to N levels.
    /// Example: 256 levels -> 16 levels (4-bit grayscale)
    /// </summary>
    public static void QuantizeGrayscale(byte[] gray, int levels)
    {
        if (levels <= 0 || levels > 256)
            return;

        int step = 256 / levels;

        Parallel.For(0, gray.Length, i =>
        {
            gray[i] = (byte)((gray[i] / step) * step);
        });
    }
}
