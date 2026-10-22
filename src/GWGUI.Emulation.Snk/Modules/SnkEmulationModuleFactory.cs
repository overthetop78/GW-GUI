using GWGUI.Emulation.Snk.Common.Services;

namespace GWGUI.Emulation.Snk.Modules;

public sealed class SnkEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
