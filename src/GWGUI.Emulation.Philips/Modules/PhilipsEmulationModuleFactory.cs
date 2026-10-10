using System.IO;

namespace GWGUI.Emulation.Philips.Modules;

public sealed class PhilipsEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new PhilipsEmulationModule(
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.ConfigurationsDirectoryName),
            context.DataDirectory, context.HttpClient,
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
    }
}
