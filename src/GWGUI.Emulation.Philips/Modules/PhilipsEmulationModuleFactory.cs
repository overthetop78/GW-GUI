using GWGUI.Emulation.Philips.Common.Services;

namespace GWGUI.Emulation.Philips.Modules;

public sealed class PhilipsEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
