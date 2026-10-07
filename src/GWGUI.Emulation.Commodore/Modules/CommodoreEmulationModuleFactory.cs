using System.IO;

namespace GWGUI.Emulation.Commodore.Modules;

public sealed class CommodoreEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var moduleDirectory = context.ModuleDirectory;
        Directory.CreateDirectory(Path.Combine(moduleDirectory,
            EmulationPathConstants.FirmwareDirectoryName));
        return new CommodoreEmulationModule(
            Path.Combine(moduleDirectory, CoreDirectoryConstants.ConfigurationsDirectoryName),
            context.DataDirectory,
            context.HttpClient,
            Path.Combine(moduleDirectory, CoreDirectoryConstants.CoreDirectoryName));
    }
}
