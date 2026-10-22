using GWGUI.Emulation.Atari.Common.Services;
namespace GWGUI.Emulation.Atari.Modules;

public sealed class AtariEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
