using System.IO;

namespace GWGUI.Emulation.Commodore.Modules;

public sealed class CommodoreEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var legacyDirectory = Path.Combine(context.DataDirectory,
            EmulationPathConstants.RootDirectoryName, EmulationPathConstants.MachinesDirectoryName, "amiga");
        var moduleDirectory = !Directory.Exists(context.ModuleDirectory) && Directory.Exists(legacyDirectory)
            ? legacyDirectory : context.ModuleDirectory;
        Directory.CreateDirectory(Path.Combine(moduleDirectory,
            EmulationPathConstants.FirmwareDirectoryName));
        return new CommodoreEmulationModule(
            Path.Combine(moduleDirectory, "Configurations"),
            context.DataDirectory,
            context.HttpClient,
            Path.Combine(moduleDirectory, "Core"));
    }
}
