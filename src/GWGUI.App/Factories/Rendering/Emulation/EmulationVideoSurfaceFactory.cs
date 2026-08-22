using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.App.Rendering.Emulation.Surfaces;
using GWGUI.Emulation;
using Veldrid;

namespace GWGUI.App.Factories.Rendering.Emulation;

internal static class EmulationVideoSurfaceFactory
{
    internal static IEmulationVideoSurface Create(EmulationVideoRenderer renderer) => renderer switch
    {
        EmulationVideoRenderer.Direct3D11 => new VeldridVideoSurface(GraphicsBackend.Direct3D11),
        EmulationVideoRenderer.Vulkan => new VeldridVideoSurface(GraphicsBackend.Vulkan),
        EmulationVideoRenderer.OpenGL => new OpenGlVideoSurface(),
        _ => new WpfVideoSurface()
    };
}
