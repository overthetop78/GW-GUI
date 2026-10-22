using GWGUI.Emulation.Amstrad.Common.Services;

namespace GWGUI.Emulation.Amstrad.Modules;

public sealed class AmstradEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
