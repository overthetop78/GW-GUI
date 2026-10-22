using GWGUI.Emulation.Commodore.Common.Services;

namespace GWGUI.Emulation.Commodore.Modules;

public sealed class CommodoreEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
