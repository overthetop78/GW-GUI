using GWGUI.Emulation.Nintendo.Common.Services;
using GWGUI.Emulation.Nintendo.Common.Constants;

namespace GWGUI.Emulation.Nintendo.Modules;

public sealed class NintendoEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
