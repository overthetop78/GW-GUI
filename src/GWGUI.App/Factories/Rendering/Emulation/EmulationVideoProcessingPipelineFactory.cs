using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.App.Rendering.Emulation.Processing;

namespace GWGUI.App.Factories.Rendering.Emulation;

internal static class EmulationVideoProcessingPipelineFactory
{
    internal static IEmulationVideoProcessingPipeline Create(EmulationVideoRenderer renderer) =>
        renderer switch
        {
            EmulationVideoRenderer.Wpf => new SoftwareEmulationVideoProcessingPipeline(),
            EmulationVideoRenderer.OpenGL or
            EmulationVideoRenderer.Direct3D11 or
            EmulationVideoRenderer.Vulkan =>
                new PassthroughEmulationVideoProcessingPipeline(renderer),
            _ => throw new ArgumentOutOfRangeException(nameof(renderer), renderer, null)
        };
}
