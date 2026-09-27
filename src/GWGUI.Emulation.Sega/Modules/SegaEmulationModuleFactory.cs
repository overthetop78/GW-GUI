using System.IO;
using GWGUI.Emulation.Sega.Common.Constants;

namespace GWGUI.Emulation.Sega.Modules;

public sealed class SegaEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new SegaEmulationModule(
            Path.Combine(context.ModuleDirectory, "Configurations"));
    }
}
