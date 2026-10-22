using GWGUI.Emulation.Coleco.Common.Services;

namespace GWGUI.Emulation.Coleco.Modules;

public sealed class ColecoEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}

