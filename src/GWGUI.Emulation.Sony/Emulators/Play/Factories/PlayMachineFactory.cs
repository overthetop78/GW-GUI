using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Play.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Play.Factories;

internal sealed class PlayMachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    public override int GetControllerPortCount(MachineConfiguration configuration) =>
        ControllerPortConstants.OneControllerPort;
}
