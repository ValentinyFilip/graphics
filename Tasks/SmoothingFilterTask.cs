using System.Windows.Media.Imaging;
using RasterGraphics.Common;
using RasterGraphics.Exercises;

namespace RasterGraphics.Tasks;

public class SmoothingFilterTask(int width, int height) : IExerciseInterface
{
    public WriteableBitmap Execute()
    {
        var vram = new VRam(width, height);

        return vram.GetBitmap();
    }

    public static void ConvolutionWithThreshold(VRam vram, in Kernel kernel, int threshold)
    {
        int[] sourceData = vram._rawData;
        int[] outputData = new int[sourceData.Length];

        int width = vram.Width, height = vram.Height;
        int[,] kernelData = kernel.Data;
        int kernelSize = kernel.Height;
        int halfSize = kernelSize / 2;

        // Precompute kernel sum
        int kernelSum = 0;
        for (int ky = 0; ky < kernelSize; ky++)
        for (int kx = 0; kx < kernelSize; kx++)
            kernelSum += kernelData[ky, kx];

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                int originalArgb = sourceData[idx];

                // Convert original pixel to grayscale using standard formula
                int origR = (originalArgb >> 16) & 0xFF;
                int origG = (originalArgb >> 8) & 0xFF;
                int origB = originalArgb & 0xFF;
                int originalGray = (int)(0.299 * origR + 0.587 * origG + 0.114 * origB);

                // Apply convolution (works on grayscale values)
                int sum = 0;

                for (int ky = 0; ky < kernelSize; ky++)
                {
                    int py = Math.Clamp(y + ky - halfSize, 0, height - 1);
                    int rowOffset = py * width;

                    for (int kx = 0; kx < kernelSize; kx++)
                    {
                        int px = Math.Clamp(x + kx - halfSize, 0, width - 1);
                        int argb = sourceData[rowOffset + px];

                        // Convert neighbor to grayscale
                        int r = (argb >> 16) & 0xFF;
                        int g = (argb >> 8) & 0xFF;
                        int b = argb & 0xFF;
                        int gray = (int)(0.299 * r + 0.587 * g + 0.114 * b);

                        int kernelValue = kernelData[ky, kx];
                        sum += kernelValue * gray;
                    }
                }

                int smoothedGray = sum / kernelSum;

                // Apply threshold rule
                int difference = Math.Abs(smoothedGray - originalGray);
                int resultGray;

                if (difference < threshold)
                {
                    // Use smoothed value
                    resultGray = smoothedGray;
                }
                else
                {
                    // Keep original (edge preservation)
                    resultGray = originalGray;
                }

                // Convert back to RGB (grayscale so R=G=B)
                resultGray = Math.Clamp(resultGray, 0, 255);
                outputData[idx] = (255 << 24) | (resultGray << 16) | (resultGray << 8) | resultGray;
            }
        });

        Array.Copy(outputData, sourceData, outputData.Length);
    }
}
