using GWGUI.Emulation.Apple.Common.Services;

namespace GWGUI.Emulation.Apple.Modules;

public sealed class AppleEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
