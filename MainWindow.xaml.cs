using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using RasterGraphics.Common;
using RasterGraphics.Exercises;
using RasterGraphics.Tasks;

namespace RasterGraphics;

public partial class MainWindow
{
    private VRam? _vram;
    private VRam? _originalVram;

    public MainWindow()
    {
        InitializeComponent();

        // var exercise = new Cv01Rgb(256, 256);
        //
        // ImagePanel.SetImage(exercise.Execute());
    }

    private void LoadImage_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Image files|*.png;*.bmp;*.jpg;*.jpeg;*.webp", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dlg.ShowDialog() == true)
        {
            try
            {
                BitmapImage bitmap = new(new Uri(dlg.FileName));

                _vram = new VRam(bitmap.PixelWidth, bitmap.PixelHeight);
                _vram.LoadFromBitmap(bitmap);

                _originalVram = new VRam(bitmap.PixelWidth, bitmap.PixelHeight);
                _originalVram.LoadFromBitmap(bitmap);

                ImagePanel.SetImage(_vram.GetBitmap());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load image: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SaveImage_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dlg = new() { Filter = "PNG Image|*.png", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) };
        if (dlg.ShowDialog() != true)
        {
            return;
        }

        BitmapSource bitmap = ImagePanel.GetImage();
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream stream = new(dlg.FileName, FileMode.Create);
        encoder.Save(stream);
    }

    private void Revert_Click(object sender, RoutedEventArgs e)
    {
        if (_originalVram == null)
        {
            MessageBox.Show("No original image to revert to.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Copy original back to working vram
        _vram = new VRam(_originalVram.Width, _originalVram.Height);
        _vram.CopyFrom(_originalVram);

        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void Grayscale_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Cv02Images.GrayScale(_vram);
        stopwatch.Stop();
        Console.WriteLine($"Grayscale took {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void Saturation_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Cv02Images.SaturateImage(_vram, 0.5f);
        stopwatch.Stop();
        Console.WriteLine($"Saturation took {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void Hue_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Cv02Images.HueShift(_vram, 151.6f);
        stopwatch.Stop();
        Console.WriteLine($"Hue shift took {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void BoxBlur_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var boxBlur = Kernel.BoxBlur3X3;
        Cv03Convolution.Convolution(_vram, in boxBlur);
        stopwatch.Stop();
        Console.WriteLine($"BoxBlur 3x3 took {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void EdgeDetect_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var edgeDetect = Kernel.EdgeDetect;
        Cv03Convolution.ConvolutionLaplacianGray(_vram, in edgeDetect);
        stopwatch.Stop();
        Console.WriteLine($"EdgeDetection took {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void GaussianBlurSmall_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var boxBlur = Kernel.GaussianBlur3x3;
        Cv03Convolution.Convolution(_vram, in boxBlur);
        stopwatch.Stop();
        Console.WriteLine($"GaussianBlur 3x3 took {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void GaussianBlurBig_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var boxBlur = Kernel.GaussianBlur5x5;
        Cv03Convolution.Convolution(_vram, in boxBlur);
        stopwatch.Stop();
        Console.WriteLine($"GaussianBlur 5x5 took {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void RemoveRedEyes_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        RemoveRedEyeEffectTask.RemoveRedEyes(_vram);
        stopwatch.Stop();
        Console.WriteLine($"RemoveRedEyes took {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private async void GaussianSmoothingWithThreshold_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int[] originalData = new int[_vram._rawData.Length];
        Array.Copy(_vram._rawData, originalData, originalData.Length);

        var boxBlur = Kernel.GaussianBlur5x5;
        Array.Copy(originalData, _vram._rawData, originalData.Length);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        SmoothingFilterTask.ConvolutionWithThreshold(_vram, in boxBlur, 50);

        stopwatch.Stop();
        Console.WriteLine($"Smoothing with threshold T=50 took {stopwatch.ElapsedMilliseconds} ms");

        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void CompressionRle_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // 1) Convert to grayscale
        byte[] gray = Cv04_Compression.ToGrayscaleBuffer(_vram);
        int originalBytes = gray.Length; // 1 byte per pixel

        // 2) Compress with RLE
        var compressed = Cv04_Compression.CompressRle(gray);
        int compressedBytes = compressed.Count * 2; // count + value = 2 bytes per run

        // 3) Decompress for verification
        byte[] decompressed = Cv04_Compression.DecompressRle(compressed, gray.Length);

        // 4) Display decompressed image
        Cv04_Compression.FromGrayscaleBuffer(_vram, decompressed);

        stopwatch.Stop();

        double ratio = (double)compressedBytes / originalBytes;
        double savedPercent = (1.0 - ratio) * 100;

        MessageBox.Show(
            $"RLE Lossless Compression:\n\n" +
            $"Original size: {originalBytes:N0} bytes\n" +
            $"Compressed size: {compressedBytes:N0} bytes\n" +
            $"Compression ratio: {ratio:P1}\n" +
            $"Space saved: {savedPercent:F1}%\n\n" +
            $"Time: {stopwatch.ElapsedMilliseconds} ms\n\n" +
            $"Note: Image is lossless (identical to original)",
            "Compression Results",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        Console.WriteLine($"RLE Compression: {originalBytes} B → {compressedBytes} B (ratio {ratio:P1}, saved {savedPercent:F1}%)");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void CompressionRleQuantized_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // 1) Convert to grayscale
        byte[] gray = Cv04_Compression.ToGrayscaleBuffer(_vram);
        int originalBytes = gray.Length;

        // 2) Apply lossy quantization (256 levels -> 16 levels)
        Cv04_Compression.QuantizeGrayscale(gray, 16);

        // 3) Compress with RLE (works much better after quantization!)
        var compressed = Cv04_Compression.CompressRle(gray);
        int compressedBytes = compressed.Count * 2;

        // 4) Decompress
        byte[] decompressed = Cv04_Compression.DecompressRle(compressed, gray.Length);

        // 5) Display result
        Cv04_Compression.FromGrayscaleBuffer(_vram, decompressed);

        stopwatch.Stop();

        double ratio = (double)compressedBytes / originalBytes;
        double savedPercent = (1.0 - ratio) * 100;

        MessageBox.Show(
            $"RLE with Quantization (Lossy):\n\n" +
            $"Original size: {originalBytes:N0} bytes\n" +
            $"Compressed size: {compressedBytes:N0} bytes\n" +
            $"Compression ratio: {ratio:P1}\n" +
            $"Space saved: {savedPercent:F1}%\n\n" +
            $"Time: {stopwatch.ElapsedMilliseconds} ms\n\n" +
            $"Note: Image quality reduced\n" +
            $"(256 gray levels → 16 gray levels)",
            "Compression Results",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        Console.WriteLine($"RLE + Quantization: {originalBytes} B → {compressedBytes} B (ratio {ratio:P1}, saved {savedPercent:F1}%)");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void LineNative_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        // Draw star pattern using native WPF rendering
        var bitmap = Cv05_LineDrawing.DrawStarPatternNative(
            _vram.Width,
            _vram.Height,
            Colors.White);

        stopwatch.Stop();

        Console.WriteLine($"Native (WPF): {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(bitmap);
    }

    private void LineDDA_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        // Clear screen
        Array.Clear(_vram._rawData, 0, _vram._rawData.Length);

        // Draw test pattern
        int w = _vram.Width;
        int h = _vram.Height;
        int centerX = w / 2;
        int centerY = h / 2;
        int radius = Math.Min(w, h) / 3;

        for (int i = 0; i < 16; i++)
        {
            double angle = i * Math.PI * 2 / 16;
            int x = centerX + (int)(Math.Cos(angle) * radius);
            int y = centerY + (int)(Math.Sin(angle) * radius);

            Cv05_LineDrawing.DrawLineDDA(_vram, centerX, centerY, x, y, 0xFFFFFFFF);
        }

        stopwatch.Stop();
        Console.WriteLine($"DDA: {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void LineBresenham_Click(object sender, RoutedEventArgs e)
    {
        if (_vram == null)
        {
            MessageBox.Show("Please load an image first.", "No Image",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        Array.Clear(_vram._rawData, 0, _vram._rawData.Length);

        int w = _vram.Width;
        int h = _vram.Height;
        int centerX = w / 2;
        int centerY = h / 2;
        int radius = Math.Min(w, h) / 3;

        for (int i = 0; i < 16; i++)
        {
            double angle = i * Math.PI * 2 / 16;
            int x = centerX + (int)(Math.Cos(angle) * radius);
            int y = centerY + (int)(Math.Sin(angle) * radius);

            Cv05_LineDrawing.DrawLineBresenham(_vram, centerX, centerY, x, y, 0xFFFFFFFF);
        }

        stopwatch.Stop();
        Console.WriteLine($"Bresenham: {stopwatch.ElapsedMilliseconds} ms");
        ImagePanel.SetImage(_vram.GetBitmap());
    }

    private void BezierQuadratic_Click(object sender, RoutedEventArgs e)
    {
        var vram = new VRam(800, 600);
        var exercise = new Cv06_BezierCurves(800, 600);
        ImagePanel.SetImage(exercise.Execute());
    }

    private void BezierCubic_Click(object sender, RoutedEventArgs e)
    {
        var vram = new VRam(800, 600);

        Point p0 = new(50, 300);
        Point p1 = new(200, 50);
        Point p2 = new(600, 50);
        Point p3 = new(750, 300);

        Cv06_BezierCurves.DrawBezierCubic(vram, p0, p1, p2, p3, 0xFF00FF00, 500);

        ImagePanel.SetImage(vram.GetBitmap());
    }

    private void BezierSpline_Click(object sender, RoutedEventArgs e)
    {
        var task = new BezierSplineTask(800, 600);
        ImagePanel.SetImage(task.Execute());
    }
}
