using GWGUI.Emulation.Watara.Common.Services;

namespace GWGUI.Emulation.Watara.Modules;

public sealed class WataraEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
