using GWGUI.Emulation.Sega.Common.Services;
using GWGUI.Emulation.Sega.Common.Constants;

namespace GWGUI.Emulation.Sega.Modules;

public sealed class SegaEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
