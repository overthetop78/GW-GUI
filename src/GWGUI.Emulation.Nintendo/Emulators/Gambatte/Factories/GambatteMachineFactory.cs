using GWGUI.Emulation.Nintendo.Emulators.Gambatte.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Gambatte.Factories;

internal sealed class GambatteMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.GameBoy or ModelConstants.GameBoyColor;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
