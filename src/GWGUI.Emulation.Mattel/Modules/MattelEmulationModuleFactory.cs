using GWGUI.Emulation.Mattel.Common.Services;

namespace GWGUI.Emulation.Mattel.Modules;

public sealed class MattelEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
