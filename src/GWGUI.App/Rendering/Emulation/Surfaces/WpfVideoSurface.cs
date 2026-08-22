using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GWGUI.Emulation;

namespace GWGUI.App.Rendering.Emulation.Surfaces;

internal sealed class WpfVideoSurface : IEmulationVideoSurface
{
    private readonly Image _image = new() { Stretch = Stretch.Fill, Focusable = true };
    private WriteableBitmap? _bitmap;
    public FrameworkElement View => _image;
    public BitmapSource? Snapshot => _bitmap;
    public EmulationVideoRenderer Renderer => EmulationVideoRenderer.Wpf;
    public IntPtr InputHandle => IntPtr.Zero;

    public void Present(VideoFrame frame)
    {
        var pixels = EmulationVideoPixelFunctions.ToBgra32(frame);
        var pitch = checked(frame.Width * EmulationVideoPixelConstants.BytesPerBgraPixel);
        if (_bitmap is null || _bitmap.PixelWidth != frame.Width || _bitmap.PixelHeight != frame.Height)
        {
            _bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgra32, null);
            _image.Source = _bitmap;
        }
        _bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), pixels, pitch, 0);
    }

    public void Dispose() { }
}
