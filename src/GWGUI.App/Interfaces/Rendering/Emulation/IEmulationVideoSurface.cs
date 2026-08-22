using System.Windows;
using System.Windows.Media.Imaging;
using GWGUI.Emulation;

namespace GWGUI.App.Interfaces.Rendering.Emulation;

internal interface IEmulationVideoSurface : IDisposable
{
    FrameworkElement View { get; }
    BitmapSource? Snapshot { get; }
    EmulationVideoRenderer Renderer { get; }
    IntPtr InputHandle { get; }
    void Present(VideoFrame frame);
}
