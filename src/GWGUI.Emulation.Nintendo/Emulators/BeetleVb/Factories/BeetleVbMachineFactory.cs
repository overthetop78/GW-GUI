using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Factories;

internal sealed class BeetleVbMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.VirtualBoy;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
