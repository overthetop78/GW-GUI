using System.IO;

namespace GWGUI.Emulation.Epoch.Modules;

public sealed class EpochEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new EpochEmulationModule( Path.Combine(context.ModuleDirectory, EmulationModuleConstants.ConfigurationsDirectoryName), context.DataDirectory, context.HttpClient, Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
    }
}

