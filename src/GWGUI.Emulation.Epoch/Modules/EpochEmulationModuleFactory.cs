using GWGUI.Emulation.Epoch.Common.Services;

namespace GWGUI.Emulation.Epoch.Modules;

public sealed class EpochEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}

