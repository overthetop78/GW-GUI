using System.IO;
using GWGUI.Emulation.Nintendo.Common.Constants;

namespace GWGUI.Emulation.Nintendo.Modules;

public sealed class NintendoEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Directory.CreateDirectory(Path.Combine(context.ModuleDirectory, "Configurations"));
        return new NintendoEmulationModule(
            Path.Combine(context.ModuleDirectory, "Configurations"),
            context.DataDirectory,
            context.HttpClient,
            Path.Combine(context.ModuleDirectory, "Core"));
    }
}
