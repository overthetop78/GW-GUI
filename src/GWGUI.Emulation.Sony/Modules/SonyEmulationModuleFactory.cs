using GWGUI.Emulation.Sony.Common.Services;
using GWGUI.Emulation.Sony.Common.Constants;

namespace GWGUI.Emulation.Sony.Modules;

public sealed class SonyEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
