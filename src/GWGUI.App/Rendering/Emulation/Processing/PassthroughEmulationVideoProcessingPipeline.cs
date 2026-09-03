using GWGUI.App.Interfaces.Rendering.Emulation;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed class PassthroughEmulationVideoProcessingPipeline(
    EmulationVideoRenderer renderer) : IEmulationVideoProcessingPipeline
{
    public EmulationVideoRenderer Renderer { get; } = renderer;

    public VideoFrame Process(
        EmulationVideoProcessingConfiguration configuration,
        VideoFrame frame,
        EmulationVideoProcessingSize sourceSize,
        EmulationVideoProcessingSize outputSize)
    {
        _ = EmulationVideoProcessingConfigurationFunctions.Normalize(configuration);
        Validate(sourceSize, nameof(sourceSize));
        Validate(outputSize, nameof(outputSize));
        if (sourceSize.Width != frame.Width || sourceSize.Height != frame.Height)
            throw new ArgumentException(nameof(sourceSize));
        return frame;
    }

    public void Dispose() { }

    private static void Validate(EmulationVideoProcessingSize size, string parameterName)
    {
        if (size.Width <= 0 || size.Height <= 0)
            throw new ArgumentOutOfRangeException(parameterName);
    }
}
