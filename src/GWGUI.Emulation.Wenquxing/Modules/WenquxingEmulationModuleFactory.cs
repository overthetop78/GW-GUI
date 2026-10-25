using GWGUI.Emulation.Wenquxing.Common.Services;

namespace GWGUI.Emulation.Wenquxing.Modules;

public sealed class WenquxingEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
