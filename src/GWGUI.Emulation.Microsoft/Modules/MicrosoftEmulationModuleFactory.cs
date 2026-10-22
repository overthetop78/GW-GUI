using GWGUI.Emulation.Microsoft.Common.Services;
using GWGUI.Emulation.Microsoft.Common.Constants;

namespace GWGUI.Emulation.Microsoft.Modules;

public sealed class MicrosoftEmulationModuleFactory : IEmulationModuleFactory
{
    public string Id => EmulationModuleConstants.ModuleId;

    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
