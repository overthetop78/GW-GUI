using GWGUI.Emulation.Sinclair.Common.Services;

namespace GWGUI.Emulation.Sinclair.Modules;

public sealed class SinclairEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
