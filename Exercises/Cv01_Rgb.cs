using System.Windows.Media.Imaging;

namespace RasterGraphics.Exercises;

public class Cv01Rgb(int width, int height) : IExerciseInterface
{
    /// <summary>
    /// x všechny odstníny červené, y všechny možné odstíny zelené, b = 128
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public WriteableBitmap Execute()
    {
        var vram = new VRam(width, height);
        const int blueColor = 128;
        int greenColor = 255;

        for (int i = 0; i < vram.Height; i++)
        {
            for (int j = 0; j < vram.Width; j++)
            {
                vram.SetPixel(j, i, i, greenColor, blueColor);
                greenColor--;
            }
            greenColor = 255;
        }

        return vram.GetBitmap();
    }
}
