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

    // TODO: so that edge detection works
    public static void Convolution(VRam vram, in Kernel kernel)
    {
        int[] sourceData = vram._rawData;
        int[] outputData = new int[sourceData.Length];

        int width = vram.Width, height = vram.Height;
        int[,] kernelData = kernel.Data;
        int kernelHeight = kernel.Height, kernelWidth = kernel.Width;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sumR = 0, sumG = 0, sumB = 0, kernelSum = 0;

                for (int ky = 0; ky < kernelHeight; ky++)
                {
                    int py = y + ky - 1;
                    if (py < 0 || py >= height) continue;

                    int rowOffset = py * width; // Pre-calculate row offset

                    for (int kx = 0; kx < kernelWidth; kx++)
                    {
                        int px = x + kx - 1;
                        if (px < 0 || px >= width) continue;

                        // Direct 1D array access - single index calculation
                        int argb = sourceData[rowOffset + px];
                        int kernelValue = kernelData[ky, kx];

                        sumR += kernelValue * ((argb >> 16) & 0xFF);
                        sumG += kernelValue * ((argb >> 8) & 0xFF);
                        sumB += kernelValue * (argb & 0xFF);
                        kernelSum += kernelValue;
                    }
                }

                int r = sumR / kernelSum;
                int g = sumG / kernelSum;
                int b = sumB / kernelSum;

                outputData[y * width + x] = (255 << 24) | (r << 16) | (g << 8) | b;
            }
        }

        Array.Copy(outputData, sourceData, outputData.Length);
    }

    public static void ConvolutionLaplacianGray(VRam vram, in Kernel kernel)
    {
        int[] src = vram._rawData;
        int[] dst = new int[src.Length];
        int w = vram.Width, h = vram.Height;
        int[,] k = kernel.Data;
        int kh = kernel.Height, kw = kernel.Width;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int acc = 0;
                for (int ky = 0; ky < kh; ky++)
                {
                    int py = Math.Clamp(y + ky - kh / 2, 0, h - 1);
                    int row = py * w;
                    for (int kx = 0; kx < kw; kx++)
                    {
                        int px = Math.Clamp(x + kx - kw / 2, 0, w - 1);
                        int argb = src[row + px];
                        int r = (argb >> 16) & 0xFF;
                        int g = (argb >> 8) & 0xFF;
                        int b = argb & 0xFF;
                        int gray = (77 * r + 150 * g + 29 * b) >> 8; // ~0.299,0.587,0.114
                        acc += k[ky, kx] * gray;
                    }
                }
                int v = (int)(Math.Abs(acc) * 6.0f);
                v = Math.Clamp(v, 0, 255);
                dst[y * w + x] = (255 << 24) | (v << 16) | (v << 8) | v;
            }
        }

        Array.Copy(dst, src, src.Length);
    }
}
