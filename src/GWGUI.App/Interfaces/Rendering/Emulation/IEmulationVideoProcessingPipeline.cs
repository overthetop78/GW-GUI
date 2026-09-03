namespace GWGUI.App.Interfaces.Rendering.Emulation;

internal readonly record struct EmulationVideoProcessingSize(int Width, int Height);

internal interface IEmulationVideoProcessingPipeline : IDisposable
{
    EmulationVideoRenderer Renderer { get; }

    VideoFrame Process(
        EmulationVideoProcessingConfiguration configuration,
        VideoFrame frame,
        EmulationVideoProcessingSize sourceSize,
        EmulationVideoProcessingSize outputSize);
}
