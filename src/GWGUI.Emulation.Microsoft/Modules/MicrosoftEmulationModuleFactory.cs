using System.IO;
using GWGUI.Emulation.Microsoft.Common.Constants;

namespace GWGUI.Emulation.Microsoft.Modules;

public sealed class MicrosoftEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Directory.CreateDirectory(Path.Combine(context.ModuleDirectory, "Configurations"));
        return new MicrosoftEmulationModule(
            Path.Combine(context.ModuleDirectory, "Configurations"),
            context.DataDirectory,
            context.HttpClient,
            Path.Combine(context.ModuleDirectory, "Core"));
    }
}
