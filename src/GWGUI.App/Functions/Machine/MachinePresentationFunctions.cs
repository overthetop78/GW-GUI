using GWGUI.App.Constants.Machine;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Machine;

internal static class MachinePresentationFunctions
{
    internal static string RendererName(EmulationVideoRenderer renderer) => renderer switch
    {
        EmulationVideoRenderer.Direct3D11 => MachinePresentationConstants.Direct3D11Renderer,
        EmulationVideoRenderer.Wpf => MachinePresentationConstants.WpfRenderer,
        _ => renderer.ToString()
    };

    internal static string Status(VideoFrame frame, double expectedFramesPerSecond,
        double measuredFramesPerSecond)
    {
        var frequency = expectedFramesPerSecond > MachinePresentationConstants.EmptyMeasurement
            ? expectedFramesPerSecond
            : measuredFramesPerSecond;
        return string.Format(CultureInfo.CurrentCulture, MachinePresentationConstants.StatusFormat,
            frame.Width, frame.Height, frequency, measuredFramesPerSecond);
    }
}
