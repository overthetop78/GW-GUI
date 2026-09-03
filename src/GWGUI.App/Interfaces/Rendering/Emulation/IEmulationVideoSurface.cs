using System.Windows;
using System.Windows.Media.Imaging;
using GWGUI.Emulation;

namespace GWGUI.App.Interfaces.Rendering.Emulation;

internal interface IEmulationVideoSurface : IDisposable
{
    FrameworkElement View { get; }
    /// <summary>
    /// Gets the final GW GUI processing output before any external frame or bezel.
    /// </summary>
    BitmapSource? Snapshot { get; }
    Task<BitmapSource?> CaptureSnapshotAsync() => Task.FromResult(Snapshot);
    EmulationVideoRenderer Renderer { get; }
    IntPtr InputHandle { get; }
    EmulationVideoProcessingConfiguration VideoProcessing =>
        EmulationVideoProcessingConfigurationFunctions.Normalize(null);
    void SetVideoProcessing(EmulationVideoProcessingConfiguration configuration);
    void Present(VideoFrame frame);
}
