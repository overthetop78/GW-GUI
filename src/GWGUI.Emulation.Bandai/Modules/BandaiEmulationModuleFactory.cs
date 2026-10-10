using System.IO;

namespace GWGUI.Emulation.Bandai.Modules;

public sealed class BandaiEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new BandaiEmulationModule(
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.ConfigurationsDirectoryName),
            context.DataDirectory, context.HttpClient,
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
    }
}
