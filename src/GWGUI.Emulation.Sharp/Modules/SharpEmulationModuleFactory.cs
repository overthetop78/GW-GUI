using GWGUI.Emulation.Sharp.Common.Services;

namespace GWGUI.Emulation.Sharp.Modules;

public sealed class SharpEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
