using System.Windows.Media.Imaging;
using RasterGraphics.Common;

namespace RasterGraphics.Exercises;

public class Cv03Convolution(int width, int height) : IExerciseInterface
{
    public WriteableBitmap Execute()
    {
        var vram = new VRam(width, height);

        return vram.GetBitmap();
    }

    public static void Convolution(VRam vram, in Kernel kernel)
    {
        int[] sourceData = vram._rawData;
        int[] outputData = new int[sourceData.Length];

        int width = vram.Width, height = vram.Height;
        int[,] kernelData = kernel.Data;
        int kernelSize = kernel.Height;
        int halfSize = kernelSize / 2;

        // Precompute kernel sum once
        int kernelSum = 0;
        for (int ky = 0; ky < kernelSize; ky++)
        for (int kx = 0; kx < kernelSize; kx++)
            kernelSum += kernelData[ky, kx];

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int sumR = 0, sumG = 0, sumB = 0;

                for (int ky = 0; ky < kernelSize; ky++)
                {
                    int py = Math.Clamp(y + ky - halfSize, 0, height - 1);
                    int rowOffset = py * width;

                    for (int kx = 0; kx < kernelSize; kx++)
                    {
                        int px = Math.Clamp(x + kx - halfSize, 0, width - 1);
                        int argb = sourceData[rowOffset + px];
                        int kernelValue = kernelData[ky, kx];

                        sumR += kernelValue * ((argb >> 16) & 0xFF);
                        sumG += kernelValue * ((argb >> 8) & 0xFF);
                        sumB += kernelValue * (argb & 0xFF);
                    }
                }

                int r = sumR / kernelSum;
                int g = sumG / kernelSum;
                int b = sumB / kernelSum;

                outputData[y * width + x] = (255 << 24) | (r << 16) | (g << 8) | b;
            }
        });

        Array.Copy(outputData, sourceData, outputData.Length);
    }

    public static void ConvolutionLaplacianGray(VRam vram, in Kernel kernel)
    {
        int[] src = vram._rawData;
        int[] dst = new int[src.Length];
        int[] accBuffer = new int[src.Length];
        int w = vram.Width, h = vram.Height;
        int[,] k = kernel.Data;
        int kernelSize = kernel.Height;
        int halfSize = kernelSize / 2;
        object lockObj = new();

        int maxAbs = 1;

        Parallel.For(0, h, y =>
        {
            int localMax = 0;
            for (int x = 0; x < w; x++)
            {
                int acc = 0;
                for (int ky = 0; ky < kernelSize; ky++)
                {
                    int py = Math.Clamp(y + ky - halfSize, 0, h - 1);
                    int row = py * w;

                    for (int kx = 0; kx < kernelSize; kx++)
                    {
                        int px = Math.Clamp(x + kx - halfSize, 0, w - 1);
                        int argb = src[row + px];
                        int r = (argb >> 16) & 0xFF, g = (argb >> 8) & 0xFF, b = argb & 0xFF;
                        int gray = (77 * r + 150 * g + 29 * b) >> 8;
                        acc += k[ky, kx] * gray;
                    }
                }

                accBuffer[y * w + x] = acc;
                int absAcc = Math.Abs(acc);
                if (absAcc > localMax) localMax = absAcc;
            }

            lock (lockObj)
            {
                if (localMax > maxAbs) maxAbs = localMax;
            }
        });

        float scale = 255.0f / maxAbs;
        Parallel.For(0, src.Length, i =>
        {
            int v = (int)(Math.Abs(accBuffer[i]) * scale);
            v = Math.Clamp(v, 0, 255);
            dst[i] = (255 << 24) | (v << 16) | (v << 8) | v;
        });

        Array.Copy(dst, src, src.Length);
    }
}
