namespace GWGUI.Emulation.Atari.Modules;

public sealed class AtariEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => AtariEmulationModuleConstants.Atari;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new AtariEmulationModule(
            Path.Combine(context.ModuleDirectory, "Configurations"),
            context.DataDirectory,
            context.HttpClient,
            Path.Combine(context.ModuleDirectory, "Core"));
    }
}
