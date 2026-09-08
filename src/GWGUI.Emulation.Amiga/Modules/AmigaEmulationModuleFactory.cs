namespace GWGUI.Emulation.Amiga.Modules;

public sealed class AmigaEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => AmigaEmulationModuleConstants.Amiga;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new AmigaEmulationModule(
            Path.Combine(context.ModuleDirectory, "Configurations"),
            context.DataDirectory,
            context.HttpClient,
            Path.Combine(context.ModuleDirectory, "Core"));
    }
}
