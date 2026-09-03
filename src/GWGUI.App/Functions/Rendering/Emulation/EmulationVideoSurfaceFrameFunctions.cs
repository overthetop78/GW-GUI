using GWGUI.App.Interfaces.Rendering.Emulation;

namespace GWGUI.App.Functions.Rendering.Emulation;

internal sealed record EmulationVideoSurfaceFrame(VideoFrame Frame, byte[] Bgra32Pixels);

internal static class EmulationVideoSurfaceFrameFunctions
{
    internal static EmulationVideoSurfaceFrame Process(
        IEmulationVideoProcessingPipeline pipeline,
        EmulationVideoProcessingConfiguration configuration,
        VideoFrame frame,
        EmulationVideoProcessingSize outputSize)
    {
        var processed = pipeline.Process(configuration, frame,
            new EmulationVideoProcessingSize(frame.Width, frame.Height), outputSize);
        return new EmulationVideoSurfaceFrame(
            processed, EmulationVideoPixelFunctions.ToBgra32(processed));
    }
}
