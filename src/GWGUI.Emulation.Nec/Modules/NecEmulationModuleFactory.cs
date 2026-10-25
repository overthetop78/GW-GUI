using GWGUI.Emulation.Nec.Common.Services;
using GWGUI.Emulation.Nec.Common.Constants;

namespace GWGUI.Emulation.Nec.Modules;

public sealed class NecEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
