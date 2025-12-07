using System.Windows;
using System.Windows.Media.Imaging;
using RasterGraphics.Exercises;

namespace RasterGraphics.Tasks;

public class BezierSplineTask(int width, int height) : IExerciseInterface
{
    public WriteableBitmap Execute()
    {
        var vram = new VRam(width, height);

        // Fill with white background
        vram.FillWhite();

        // Now draw your spline on white background
        Point[] controlPoints =
        [
            new(50, 300), new(150, 100), new(300, 400), new(450, 150), new(600, 350), new(750, 200)
        ];

        DrawBezierSpline(vram, controlPoints, 0xFF000000, discretizationStep: 0.01); // Black line on white

        // Draw control points in red
        foreach (var point in controlPoints)
        {
            DrawControlPoint(vram, (int)Math.Round(point.X), (int)Math.Round(point.Y), 0xFFFF0000);
        }

        return vram.GetBitmap();
    }

    /// <summary>
    /// Draw Bezier spline through given control points.
    /// Implements spline based on cubic Bezier curves (MS Word curve tool emulation).
    /// </summary>
    /// <param name="vram">Video RAM buffer</param>
    /// <param name="points">Control points P1, P2, ..., Pn where n >= 3</param>
    /// <param name="color">Line color</param>
    /// <param name="discretizationStep">Step size for curve generation (smaller = smoother)</param>
    private static void DrawBezierSpline(VRam vram, Point[] points, uint color, double discretizationStep = 0.01)
    {
        int n = points.Length;
        if (n < 3)
            return;

        // Step 2: Extend sequence with P0 = P1 and P(n+1) = Pn
        Point[] extendedPoints = new Point[n + 2];
        extendedPoints[0] = points[0]; // P0 = P1
        for (int i = 0; i < n; i++)
        {
            extendedPoints[i + 1] = points[i];
        }

        extendedPoints[n + 1] = points[n - 1]; // P(n+1) = Pn

        // Step 3: Create L and R points for each Pi
        // Li = Pi - (P(i+1) - P(i-1)) / 6
        // Ri = Pi + (P(i+1) - P(i-1)) / 6
        Point[] l = new Point[n + 2];
        Point[] r = new Point[n + 2];

        for (int i = 1; i <= n; i++)
        {
            Point pPrev = extendedPoints[i - 1];
            Point pCurrent = extendedPoints[i];
            Point pNext = extendedPoints[i + 1];

            l[i] = new Point(
                pCurrent.X - (pNext.X - pPrev.X) / 6.0,
                pCurrent.Y - (pNext.Y - pPrev.Y) / 6.0
            );

            r[i] = new Point(
                pCurrent.X + (pNext.X - pPrev.X) / 6.0,
                pCurrent.Y + (pNext.Y - pPrev.Y) / 6.0
            );
        }

        // Step 4: For i = 1, ..., n-1, create cubic Bezier curves
        // Using quartet of points: Pi, Ri, L(i+1), P(i+1)
        for (int i = 1; i < n; i++)
        {
            Point p0 = extendedPoints[i];
            Point p1 = r[i];
            Point p2 = l[i + 1];
            Point p3 = extendedPoints[i + 1];

            // Use Wu's algorithm with small step for best quality
            DrawCubicBezierSegmentWu(vram, p0, p1, p2, p3, color, discretizationStep);
        }
    }

    /// <summary>
    /// Draw single cubic Bezier curve segment.
    /// Generates curve points with specified discretization step.
    /// Approximates curve with piecewise linear segments using line drawing implementation.
    /// </summary>
    private static void DrawCubicBezierSegment(VRam vram, Point p0, Point p1, Point p2, Point p3, uint color, double step)
    {
        Point prevPoint = p0;

        for (double t = step; t <= 1.0; t += step)
        {
            Point point = EvaluateCubicBezier(p0, p1, p2, p3, t);

            // Approximate curve with line segments
            Cv05_LineDrawing.DrawLineBresenham(vram,
                (int)Math.Round(prevPoint.X), (int)Math.Round(prevPoint.Y),
                (int)Math.Round(point.X), (int)Math.Round(point.Y),
                color);

            prevPoint = point;
        }

        // Ensure we reach the end point exactly
        Cv05_LineDrawing.DrawLineBresenham(vram,
            (int)Math.Round(prevPoint.X), (int)Math.Round(prevPoint.Y),
            (int)Math.Round(p3.X), (int)Math.Round(p3.Y),
            color);
    }

    /// <summary>
    /// Evaluate cubic Bezier curve at parameter t using standard formula.
    /// P(t) = (1-t)³*P0 + 3*(1-t)²*t*P1 + 3*(1-t)*t²*P2 + t³*P3
    /// Optimized implementation (textbook chapter 6.4).
    /// </summary>
    private static Point EvaluateCubicBezier(Point p0, Point p1, Point p2, Point p3, double t)
    {
        double u = 1 - t;
        double uu = u * u;
        double uuu = uu * u;
        double tt = t * t;
        double ttt = tt * t;

        double x = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X;
        double y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y;

        return new Point(x, y);
    }

    /// <summary>
    /// Alternative implementation: De Casteljau algorithm.
    /// More numerically stable for high-precision requirements.
    /// </summary>
    private static Point EvaluateCubicBezierDeCasteljau(Point p0, Point p1, Point p2, Point p3, double t)
    {
        // First level interpolation
        Point p01 = Lerp(p0, p1, t);
        Point p12 = Lerp(p1, p2, t);
        Point p23 = Lerp(p2, p3, t);

        // Second level interpolation
        Point p012 = Lerp(p01, p12, t);
        Point p123 = Lerp(p12, p23, t);

        // Third level interpolation - final point on curve
        Point p0123 = Lerp(p012, p123, t);

        return p0123;
    }

    /// <summary>
    /// Linear interpolation between two points.
    /// </summary>
    private static Point Lerp(Point a, Point b, double t)
    {
        return new Point(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t
        );
    }

    /// <summary>
    /// Alternative spline drawing with adaptive subdivision.
    /// Automatically adjusts sampling density based on curve curvature.
    /// More efficient for complex curves with varying curvature.
    /// </summary>
    public static void DrawBezierSplineAdaptive(VRam vram, Point[] points, uint color, double tolerance = 1.0)
    {
        int n = points.Length;
        if (n < 3)
            return;

        // Steps 2 and 3: Same as standard implementation
        Point[] extendedPoints = new Point[n + 2];
        extendedPoints[0] = points[0];
        for (int i = 0; i < n; i++)
        {
            extendedPoints[i + 1] = points[i];
        }

        extendedPoints[n + 1] = points[n - 1];

        Point[] l = new Point[n + 2];
        Point[] r = new Point[n + 2];

        for (int i = 1; i <= n; i++)
        {
            Point pPrev = extendedPoints[i - 1];
            Point pCurrent = extendedPoints[i];
            Point pNext = extendedPoints[i + 1];

            l[i] = new Point(
                pCurrent.X - (pNext.X - pPrev.X) / 6.0,
                pCurrent.Y - (pNext.Y - pPrev.Y) / 6.0
            );

            r[i] = new Point(
                pCurrent.X + (pNext.X - pPrev.X) / 6.0,
                pCurrent.Y + (pNext.Y - pPrev.Y) / 6.0
            );
        }

        // Draw segments with adaptive subdivision
        for (int i = 1; i < n; i++)
        {
            Point p0 = extendedPoints[i];
            Point p1 = r[i];
            Point p2 = l[i + 1];
            Point p3 = extendedPoints[i + 1];

            SubdivideCubicBezier(vram, p0, p1, p2, p3, color, tolerance);
        }
    }

    /// <summary>
    /// Adaptive subdivision for cubic Bezier curves.
    /// Recursively subdivides curve until it's flat enough to approximate with straight line.
    /// </summary>
    private static void SubdivideCubicBezier(VRam vram, Point p0, Point p1, Point p2, Point p3, uint color, double tolerance)
    {
        // Calculate flatness (maximum distance from control points to line P0-P3)
        double flatness = CalculateFlatness(p0, p1, p2, p3);

        if (flatness < tolerance)
        {
            // Curve is flat enough, draw straight line
            Cv05_LineDrawing.DrawLineBresenham(vram,
                (int)Math.Round(p0.X), (int)Math.Round(p0.Y),
                (int)Math.Round(p3.X), (int)Math.Round(p3.Y),
                color);
        }
        else
        {
            // Subdivide curve at t = 0.5 using De Casteljau
            Point p01 = Lerp(p0, p1, 0.5);
            Point p12 = Lerp(p1, p2, 0.5);
            Point p23 = Lerp(p2, p3, 0.5);
            Point p012 = Lerp(p01, p12, 0.5);
            Point p123 = Lerp(p12, p23, 0.5);
            Point p0123 = Lerp(p012, p123, 0.5);

            // Recursively draw both halves
            SubdivideCubicBezier(vram, p0, p01, p012, p0123, color, tolerance);
            SubdivideCubicBezier(vram, p0123, p123, p23, p3, color, tolerance);
        }
    }

    /// <summary>
    /// Calculate flatness metric of cubic Bezier curve.
    /// Returns maximum perpendicular distance from control points P1 and P2 to line P0-P3.
    /// </summary>
    private static double CalculateFlatness(Point p0, Point p1, Point p2, Point p3)
    {
        double d1 = DistancePointToLine(p1, p0, p3);
        double d2 = DistancePointToLine(p2, p0, p3);
        return Math.Max(d1, d2);
    }

    /// <summary>
    /// Calculate perpendicular distance from point to line segment.
    /// </summary>
    private static double DistancePointToLine(Point point, Point lineStart, Point lineEnd)
    {
        double dx = lineEnd.X - lineStart.X;
        double dy = lineEnd.Y - lineStart.Y;
        double lengthSq = dx * dx + dy * dy;

        if (lengthSq < 0.0001)
        {
            // Line is actually a point
            double pdx = point.X - lineStart.X;
            double pdy = point.Y - lineStart.Y;
            return Math.Sqrt(pdx * pdx + pdy * pdy);
        }

        // Calculate projection parameter
        double t = ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / lengthSq;
        t = Math.Clamp(t, 0, 1);

        // Calculate projection point
        double projX = lineStart.X + t * dx;
        double projY = lineStart.Y + t * dy;

        // Calculate distance
        double distX = point.X - projX;
        double distY = point.Y - projY;

        return Math.Sqrt(distX * distX + distY * distY);
    }

    private static void DrawControlPoint(VRam vram, int x, int y, uint color)
    {
        int radius = 4;
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int px = x + dx;
                    int py = y + dy;
                    if (px >= 0 && px < vram.Width && py >= 0 && py < vram.Height)
                    {
                        vram._rawData[py * vram.Width + px] = (int)color;
                    }
                }
            }
        }
    }

    public static void DrawLineWu(VRam vram, double x0, double y0, double x1, double y1, uint color)
    {
        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

        if (steep)
        {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }

        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }

        double dx = x1 - x0;
        double dy = y1 - y0;
        double gradient = dy / dx;

        if (dx == 0.0)
            gradient = 1.0;

        // Handle first endpoint
        double xEnd = Math.Round(x0);
        double yEnd = y0 + gradient * (xEnd - x0);
        double xGap = 1.0 - Frac(x0 + 0.5);
        int xPixel1 = (int)xEnd;
        int yPixel1 = (int)yEnd;

        if (steep)
        {
            PlotAlpha(vram, yPixel1, xPixel1, (1.0 - Frac(yEnd)) * xGap, color);
            PlotAlpha(vram, yPixel1 + 1, xPixel1, Frac(yEnd) * xGap, color);
        }
        else
        {
            PlotAlpha(vram, xPixel1, yPixel1, (1.0 - Frac(yEnd)) * xGap, color);
            PlotAlpha(vram, xPixel1, yPixel1 + 1, Frac(yEnd) * xGap, color);
        }

        double intery = yEnd + gradient;

        // Handle second endpoint
        xEnd = Math.Round(x1);
        yEnd = y1 + gradient * (xEnd - x1);
        xGap = Frac(x1 + 0.5);
        int xPixel2 = (int)xEnd;
        int yPixel2 = (int)yEnd;

        if (steep)
        {
            PlotAlpha(vram, yPixel2, xPixel2, (1.0 - Frac(yEnd)) * xGap, color);
            PlotAlpha(vram, yPixel2 + 1, xPixel2, Frac(yEnd) * xGap, color);
        }
        else
        {
            PlotAlpha(vram, xPixel2, yPixel2, (1.0 - Frac(yEnd)) * xGap, color);
            PlotAlpha(vram, xPixel2, yPixel2 + 1, Frac(yEnd) * xGap, color);
        }

        // Main loop
        if (steep)
        {
            for (int x = xPixel1 + 1; x < xPixel2; x++)
            {
                PlotAlpha(vram, (int)intery, x, 1.0 - Frac(intery), color);
                PlotAlpha(vram, (int)intery + 1, x, Frac(intery), color);
                intery += gradient;
            }
        }
        else
        {
            for (int x = xPixel1 + 1; x < xPixel2; x++)
            {
                PlotAlpha(vram, x, (int)intery, 1.0 - Frac(intery), color);
                PlotAlpha(vram, x, (int)intery + 1, Frac(intery), color);
                intery += gradient;
            }
        }
    }

    private static double Frac(double x)
    {
        return x - Math.Floor(x);
    }

    private static void Swap(ref double a, ref double b)
    {
        (a, b) = (b, a);
    }

    private static void PlotAlpha(VRam vram, int x, int y, double alpha, uint color)
    {
        if (x < 0 || x >= vram.Width || y < 0 || y >= vram.Height)
            return;

        int idx = y * vram.Width + x;

        // Extract color components
        byte newA = (byte)((color >> 24) & 0xFF);
        byte newR = (byte)((color >> 16) & 0xFF);
        byte newG = (byte)((color >> 8) & 0xFF);
        byte newB = (byte)(color & 0xFF);

        // Get existing pixel
        int existing = vram._rawData[idx];
        byte oldA = (byte)((existing >> 24) & 0xFF);
        byte oldR = (byte)((existing >> 16) & 0xFF);
        byte oldG = (byte)((existing >> 8) & 0xFF);
        byte oldB = (byte)(existing & 0xFF);

        // Alpha blend
        double blend = alpha * (newA / 255.0);
        byte finalR = (byte)(newR * blend + oldR * (1 - blend));
        byte finalG = (byte)(newG * blend + oldG * (1 - blend));
        byte finalB = (byte)(newB * blend + oldB * (1 - blend));
        byte finalA = Math.Max(newA, oldA);

        vram._rawData[idx] = (finalA << 24) | (finalR << 16) | (finalG << 8) | finalB;
    }

    private static void DrawCubicBezierSegmentWu(VRam vram, Point p0, Point p1, Point p2, Point p3, uint color, double step)
    {
        Point prevPoint = p0;

        for (double t = step; t <= 1.0; t += step)
        {
            Point point = EvaluateCubicBezier(p0, p1, p2, p3, t);

            // Use Wu's algorithm instead of Bresenham
            DrawLineWu(vram, prevPoint.X, prevPoint.Y, point.X, point.Y, color);

            prevPoint = point;
        }

        // Final segment
        DrawLineWu(vram, prevPoint.X, prevPoint.Y, p3.X, p3.Y, color);
    }
}
