using GWGUI.Emulation.Acorn.Common.Services;

namespace GWGUI.Emulation.Acorn.Modules;

public sealed class AcornEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
