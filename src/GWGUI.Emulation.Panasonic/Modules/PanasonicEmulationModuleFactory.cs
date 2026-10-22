using GWGUI.Emulation.Panasonic.Common.Services;

namespace GWGUI.Emulation.Panasonic.Modules;

public sealed class PanasonicEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
