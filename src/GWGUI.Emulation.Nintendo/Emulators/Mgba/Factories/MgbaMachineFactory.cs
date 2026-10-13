using GWGUI.Emulation.Nintendo.Emulators.Mgba.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Mgba.Factories;

internal sealed class MgbaMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.GameBoyAdvance;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
