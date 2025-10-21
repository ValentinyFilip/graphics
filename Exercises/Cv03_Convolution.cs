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
        int[,] sourceData = vram._rawData;
        int[,] outputData = new int[vram.Height, vram.Width];

        int[,] kernelData = kernel.Data;
        int kernelHeight = kernel.Height;
        int kernelWidth = kernel.Width;

        for (int y = 0; y < vram.Height; y++)
        {
            for (int x = 0; x < vram.Width; x++)
            {
                int sumR = 0, sumG = 0, sumB = 0, kernelSum = 0;

                for (int yk = 0; yk < kernelHeight; yk++)
                {
                    int py = y + yk - kernelHeight / 2;
                    for (int xk = 0; xk < kernelWidth; xk++)
                    {
                        int px = x + xk - kernelWidth / 2;

                        if (px < 0 || py < 0 || px >= vram.Width || py >= vram.Height)
                            continue;

                        int argb = sourceData[py, px];
                        int kernelValue = kernelData[yk, xk];

                        sumR += kernelValue * ((argb >> 16) & 0xFF);
                        sumG += kernelValue * ((argb >> 8) & 0xFF);
                        sumB += kernelValue * (argb & 0xFF);
                        kernelSum += kernelValue;
                    }
                }

                int r = Math.Clamp(sumR / kernelSum, 0, 255);
                int g = Math.Clamp(sumG / kernelSum, 0, 255);
                int b = Math.Clamp(sumB / kernelSum, 0, 255);

                outputData[y, x] = (255 << 24) | (r << 16) | (g << 8) | b;
            }
        }

        Array.Copy(outputData, sourceData, outputData.Length);
    }
}
