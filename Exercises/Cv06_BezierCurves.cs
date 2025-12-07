using System.Windows;
using System.Windows.Media.Imaging;

namespace RasterGraphics.Exercises;

public class Cv06_BezierCurves(int width, int height) : IExerciseInterface
{
    public WriteableBitmap Execute()
    {
        var vram = new VRam(width, height);

        // Example: Draw quadratic Bezier curve
        Point p0 = new(50, 200);
        Point p1 = new(150, 50);
        Point p2 = new(250, 200);

        DrawBezierQuadratic(vram, p0, p1, p2, 0xFF00FF00, 200);

        // Draw control points
        DrawControlPoint(vram, (int)p0.X, (int)p0.Y, 0xFFFF0000);
        DrawControlPoint(vram, (int)p1.X, (int)p1.Y, 0xFFFF0000);
        DrawControlPoint(vram, (int)p2.X, (int)p2.Y, 0xFFFF0000);

        // Draw control polygon
        Cv05_LineDrawing.DrawLineBresenham(vram,
            (int)p0.X, (int)p0.Y, (int)p1.X, (int)p1.Y, 0xFF888888);
        Cv05_LineDrawing.DrawLineBresenham(vram,
            (int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, 0xFF888888);

        return vram.GetBitmap();
    }

    public static void DrawBezierAdaptive(VRam vram, Point p0, Point p1, Point p2, uint color, double tolerance = 1.0)
    {
        SubdivideBezier(vram, p0, p1, p2, color, tolerance);
    }

    private static void SubdivideBezier(VRam vram, Point p0, Point p1, Point p2, uint color, double tolerance)
    {
        // Calculate midpoint
        Point mid = EvaluateBezierQuadratic(p0, p1, p2, 0.5);

        // Calculate linear interpolation midpoint
        Point linearMid = new Point(
            (p0.X + p2.X) / 2,
            (p0.Y + p2.Y) / 2
        );

        // Calculate distance between curve midpoint and linear midpoint
        double dx = mid.X - linearMid.X;
        double dy = mid.Y - linearMid.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance < tolerance)
        {
            // Curve is flat enough, draw line
            Cv05_LineDrawing.DrawLineBresenham(vram,
                (int)Math.Round(p0.X), (int)Math.Round(p0.Y),
                (int)Math.Round(p2.X), (int)Math.Round(p2.Y),
                color);
        }
        else
        {
            // Subdivide further
            Point q0 = p0;
            Point q1 = new Point((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
            Point q2 = mid;

            Point r0 = mid;
            Point r1 = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            Point r2 = p2;

            SubdivideBezier(vram, q0, q1, q2, color, tolerance);
            SubdivideBezier(vram, r0, r1, r2, color, tolerance);
        }
    }

    /// <summary>
    /// Draw quadratic Bezier curve (3 control points).
    /// P(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
    /// </summary>
    public static void DrawBezierQuadratic(VRam vram, Point p0, Point p1, Point p2, uint color, int segments = 50)
    {
        Point prevPoint = p0;

        for (int i = 1; i <= segments; i++)
        {
            double t = i / (double)segments;
            Point point = EvaluateBezierQuadratic(p0, p1, p2, t);

            Cv05_LineDrawing.DrawLineBresenham(vram,
                (int)Math.Round(prevPoint.X), (int)Math.Round(prevPoint.Y),
                (int)Math.Round(point.X), (int)Math.Round(point.Y),
                color);

            prevPoint = point;
        }
    }

    /// <summary>
    /// Draw cubic Bezier curve (4 control points).
    /// P(t) = (1-t)³P₀ + 3(1-t)²tP₁ + 3(1-t)t²P₂ + t³P₃
    /// </summary>
    public static void DrawBezierCubic(VRam vram, Point p0, Point p1, Point p2, Point p3, uint color, int segments = 100)
    {
        Point prevPoint = p0;

        for (int i = 1; i <= segments; i++)
        {
            double t = i / (double)segments;
            Point point = EvaluateBezierCubic(p0, p1, p2, p3, t);

            Cv05_LineDrawing.DrawLineBresenham(vram,
                (int)Math.Round(prevPoint.X), (int)Math.Round(prevPoint.Y),
                (int)Math.Round(point.X), (int)Math.Round(point.Y),
                color);

            prevPoint = point;
        }
    }

    /// <summary>
    /// Evaluate quadratic Bezier curve at parameter t.
    /// </summary>
    private static Point EvaluateBezierQuadratic(Point p0, Point p1, Point p2, double t)
    {
        double u = 1 - t;
        double tt = t * t;
        double uu = u * u;

        double x = uu * p0.X + 2 * u * t * p1.X + tt * p2.X;
        double y = uu * p0.Y + 2 * u * t * p1.Y + tt * p2.Y;

        return new Point(x, y);
    }

    /// <summary>
    /// Evaluate cubic Bezier curve at parameter t.
    /// </summary>
    private static Point EvaluateBezierCubic(Point p0, Point p1, Point p2, Point p3, double t)
    {
        double u = 1 - t;
        double tt = t * t;
        double uu = u * u;
        double uuu = uu * u;
        double ttt = tt * t;

        double x = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X;
        double y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y;

        return new Point(x, y);
    }

    /// <summary>
    /// De Casteljau algorithm for evaluating Bezier curve (more stable).
    /// Works for any number of control points.
    /// </summary>
    public static Point EvaluateBezierDeCasteljau(Point[] controlPoints, double t)
    {
        int n = controlPoints.Length;
        Point[] points = new Point[n];
        Array.Copy(controlPoints, points, n);

        for (int k = 1; k < n; k++)
        {
            for (int i = 0; i < n - k; i++)
            {
                points[i] = new Point(
                    (1 - t) * points[i].X + t * points[i + 1].X,
                    (1 - t) * points[i].Y + t * points[i + 1].Y
                );
            }
        }

        return points[0];
    }

    /// <summary>
    /// Draw general Bezier curve with n control points using De Casteljau.
    /// </summary>
    public static void DrawBezierGeneral(VRam vram, Point[] controlPoints, uint color, int segments = 100)
    {
        if (controlPoints.Length < 2)
            return;

        Point prevPoint = controlPoints[0];

        for (int i = 1; i <= segments; i++)
        {
            double t = i / (double)segments;
            Point point = EvaluateBezierDeCasteljau(controlPoints, t);

            Cv05_LineDrawing.DrawLineBresenham(vram,
                (int)Math.Round(prevPoint.X), (int)Math.Round(prevPoint.Y),
                (int)Math.Round(point.X), (int)Math.Round(point.Y),
                color);

            prevPoint = point;
        }
    }

    /// <summary>
    /// Draw Bezier surface (tensor product of two Bezier curves).
    /// Control points arranged in m×n grid.
    /// </summary>
    public static void DrawBezierSurface(VRam vram, Point[,] controlPoints, uint color, int uSegments = 20, int vSegments = 20)
    {
        int m = controlPoints.GetLength(0);
        int n = controlPoints.GetLength(1);

        // Draw u-parameter curves
        for (int j = 0; j <= vSegments; j++)
        {
            double v = j / (double)vSegments;

            // Get control points for this v
            Point[] uCurvePoints = new Point[m];
            for (int i = 0; i < m; i++)
            {
                Point[] vCurve = new Point[n];
                for (int k = 0; k < n; k++)
                {
                    vCurve[k] = controlPoints[i, k];
                }

                uCurvePoints[i] = EvaluateBezierDeCasteljau(vCurve, v);
            }

            DrawBezierGeneral(vram, uCurvePoints, color, uSegments);
        }

        // Draw v-parameter curves
        for (int i = 0; i <= uSegments; i++)
        {
            double u = i / (double)uSegments;

            // Get control points for this u
            Point[] vCurvePoints = new Point[n];
            for (int j = 0; j < n; j++)
            {
                Point[] uCurve = new Point[m];
                for (int k = 0; k < m; k++)
                {
                    uCurve[k] = controlPoints[k, j];
                }

                vCurvePoints[j] = EvaluateBezierDeCasteljau(uCurve, u);
            }

            DrawBezierGeneral(vram, vCurvePoints, color, vSegments);
        }
    }

    private static void DrawControlPoint(VRam vram, int x, int y, uint color)
    {
        int radius = 3;
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
}
