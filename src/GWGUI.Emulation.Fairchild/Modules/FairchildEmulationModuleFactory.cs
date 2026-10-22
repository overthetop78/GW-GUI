using GWGUI.Emulation.Fairchild.Common.Services;

namespace GWGUI.Emulation.Fairchild.Modules;

public sealed class FairchildEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}

