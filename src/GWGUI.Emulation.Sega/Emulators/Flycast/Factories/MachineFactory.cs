using GWGUI.Emulation.Sega.Emulators.Flycast.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Sega.Emulators.Flycast.Factories;

internal sealed class MachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.Dreamcast or ModelConstants.Naomi or ModelConstants.Naomi2 or ModelConstants.Atomiswave or ModelConstants.SystemSp;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
