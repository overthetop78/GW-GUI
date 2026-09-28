using System.IO;
using GWGUI.Emulation.Sega.Common.Constants;

namespace GWGUI.Emulation.Sega.Modules;

public sealed class SegaEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Directory.CreateDirectory(Path.Combine(context.ModuleDirectory, "Configurations"));
        return new SegaEmulationModule(
            Path.Combine(context.ModuleDirectory, "Configurations"),
            context.DataDirectory,
            context.HttpClient,
            Path.Combine(context.ModuleDirectory, "Core"));
    }
}
