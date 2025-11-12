using System.Diagnostics;
using System.IO;
using System.Windows;
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
}
