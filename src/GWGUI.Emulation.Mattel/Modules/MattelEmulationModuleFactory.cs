using System.IO;

namespace GWGUI.Emulation.Mattel.Modules;

public sealed class MattelEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new MattelEmulationModule(
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.ConfigurationsDirectoryName),
            context.DataDirectory, context.HttpClient,
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
    }
}
