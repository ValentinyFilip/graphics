using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterGraphics.Exercises;

public class Cv05_LineDrawing(int width, int height) : IExerciseInterface
{
    public WriteableBitmap Execute()
    {
        var vram = new VRam(width, height);

        // Draw test pattern - star shape
        int centerX = width / 2;
        int centerY = height / 2;
        int radius = Math.Min(width, height) / 3;

        for (int i = 0; i < 16; i++)
        {
            double angle = i * Math.PI * 2 / 16;
            int x = centerX + (int)(Math.Cos(angle) * radius);
            int y = centerY + (int)(Math.Sin(angle) * radius);

            DrawLineBresenham(vram, centerX, centerY, x, y, 0xFFFFFFFF);
        }

        return vram.GetBitmap();
    }

    /// <summary>
    /// Native line drawing using WPF DrawingContext (hardware accelerated).
    /// This uses WPF's rendering pipeline which is GPU-accelerated.
    /// </summary>
    public static WriteableBitmap DrawLineNative(int width, int height, int x0, int y0, int x1, int y1, Color color)
    {
        // Create a DrawingVisual
        DrawingVisual drawingVisual = new();

        using (DrawingContext dc = drawingVisual.RenderOpen())
        {
            // Draw line using WPF's native rendering
            Pen pen = new Pen(new SolidColorBrush(color), 1.0);
            pen.Freeze(); // Freeze for performance

            dc.DrawLine(pen, new Point(x0, y0), new Point(x1, y1));
        }

        // Render to bitmap
        RenderTargetBitmap renderBitmap = new(
            width, height, 96, 96, PixelFormats.Pbgra32);

        renderBitmap.Render(drawingVisual);

        // Convert to WriteableBitmap
        WriteableBitmap writeableBitmap = new(renderBitmap);

        return writeableBitmap;
    }

    /// <summary>
    /// Native line drawing for multiple lines (more efficient for batch drawing).
    /// </summary>
    public static WriteableBitmap DrawLinesNative(int width, int height, List<(Point start, Point end)> lines, Color color)
    {
        DrawingVisual drawingVisual = new();

        using (DrawingContext dc = drawingVisual.RenderOpen())
        {
            // Clear background
            dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));

            Pen pen = new Pen(new SolidColorBrush(color), 1.0);
            pen.Freeze();

            // Draw all lines
            foreach (var (start, end) in lines)
            {
                dc.DrawLine(pen, start, end);
            }
        }

        RenderTargetBitmap renderBitmap = new(
            width, height, 96, 96, PixelFormats.Pbgra32);

        renderBitmap.Render(drawingVisual);

        return new WriteableBitmap(renderBitmap);
    }

    /// <summary>
    /// Native star pattern (for comparison with DDA/Bresenham).
    /// </summary>
    public static WriteableBitmap DrawStarPatternNative(int width, int height, Color color)
    {
        DrawingVisual drawingVisual = new();

        using (DrawingContext dc = drawingVisual.RenderOpen())
        {
            // Clear background
            dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));

            Pen pen = new Pen(new SolidColorBrush(color), 1.0);
            pen.Freeze();

            int centerX = width / 2;
            int centerY = height / 2;
            int radius = Math.Min(width, height) / 3;

            // Draw star pattern
            for (int i = 0; i < 16; i++)
            {
                double angle = i * Math.PI * 2 / 16;
                int x = centerX + (int)(Math.Cos(angle) * radius);
                int y = centerY + (int)(Math.Sin(angle) * radius);

                dc.DrawLine(pen,
                    new Point(centerX, centerY),
                    new Point(x, y));
            }
        }

        RenderTargetBitmap renderBitmap = new(
            width, height, 96, 96, PixelFormats.Pbgra32);

        renderBitmap.Render(drawingVisual);

        return new WriteableBitmap(renderBitmap);
    }

    /// <summary>
    /// DDA (Digital Differential Analyzer) line drawing algorithm.
    /// Uses floating point arithmetic.
    /// </summary>
    public static void DrawLineDDA(VRam vram, int x0, int y0, int x1, int y1, uint color)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;

        int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

        if (steps == 0)
        {
            SetPixelSafe(vram, x0, y0, color);
            return;
        }

        float xIncrement = dx / (float)steps;
        float yIncrement = dy / (float)steps;

        float x = x0;
        float y = y0;

        for (int i = 0; i <= steps; i++)
        {
            SetPixelSafe(vram, (int)Math.Round(x), (int)Math.Round(y), color);
            x += xIncrement;
            y += yIncrement;
        }
    }

    /// <summary>
    /// Bresenham line drawing algorithm.
    /// Uses only integer arithmetic (faster than DDA).
    /// </summary>
    public static void DrawLineBresenham(VRam vram, int x0, int y0, int x1, int y1, uint color)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            SetPixelSafe(vram, x0, y0, color);

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    /// <summary>
    /// Optimized Bresenham for thick lines.
    /// </summary>
    public static void DrawLineBresenhamThick(VRam vram, int x0, int y0, int x1, int y1, uint color, int thickness)
    {
        if (thickness == 1)
        {
            DrawLineBresenham(vram, x0, y0, x1, y1, color);
            return;
        }

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;
        int halfThick = thickness / 2;

        while (true)
        {
            // Draw circle at current point
            for (int ty = -halfThick; ty <= halfThick; ty++)
            {
                for (int tx = -halfThick; tx <= halfThick; tx++)
                {
                    if (tx * tx + ty * ty <= halfThick * halfThick)
                    {
                        SetPixelSafe(vram, x0 + tx, y0 + ty, color);
                    }
                }
            }

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private static void SetPixelSafe(VRam vram, int x, int y, uint color)
    {
        if (x < 0 || x >= vram.Width || y < 0 || y >= vram.Height)
            return;

        vram._rawData[y * vram.Width + x] = (int)color;
    }
}
