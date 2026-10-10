using System.IO;

namespace GWGUI.Emulation.Fairchild.Modules;

public sealed class FairchildEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new FairchildEmulationModule(
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.ConfigurationsDirectoryName),
            context.DataDirectory, context.HttpClient,
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
    }
}

