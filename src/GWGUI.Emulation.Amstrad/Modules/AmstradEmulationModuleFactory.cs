using System.IO;

namespace GWGUI.Emulation.Amstrad.Modules;

public sealed class AmstradEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Directory.CreateDirectory(Path.Combine(context.ModuleDirectory,
            EmulationPathConstants.FirmwareDirectoryName));
        return new AmstradEmulationModule(
            Path.Combine(context.ModuleDirectory, "Configurations"),
            context.DataDirectory, context.HttpClient,
            Path.Combine(context.ModuleDirectory, "Core"));
    }
}
