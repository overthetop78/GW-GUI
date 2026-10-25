namespace GWGUI.App.Functions.Emulation.Machine;

internal static class EmulationVideoConfigurationFunctions
{
    internal static IEmulationConfiguration Apply(IEmulationModule module, IEmulationConfiguration configuration, EmulationVideoRenderer renderer) =>
        module is IEmulationVideoSettingsManager manager ? manager.ApplyVideoSettings(configuration, GraphicsApi(renderer)) : configuration;

    internal static EmulationGraphicsApi GraphicsApi(EmulationVideoRenderer renderer) => renderer switch
    {
        EmulationVideoRenderer.OpenGL => EmulationGraphicsApi.OpenGL,
        EmulationVideoRenderer.Vulkan => EmulationGraphicsApi.Vulkan,
        EmulationVideoRenderer.Direct3D11 => EmulationGraphicsApi.Direct3D11,
        _ => EmulationGraphicsApi.Automatic
    };
}
