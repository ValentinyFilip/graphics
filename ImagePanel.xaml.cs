using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterGraphics;

public partial class ImagePanel
{
    public ImagePanel()
    {
        InitializeComponent();
        RenderOptions.SetBitmapScalingMode(Img, BitmapScalingMode.NearestNeighbor);
    }

    public BitmapSource GetImage() => (Img.Source as BitmapSource)!;
    public void SetImage(BitmapSource image) => Img.Source = image;
}
