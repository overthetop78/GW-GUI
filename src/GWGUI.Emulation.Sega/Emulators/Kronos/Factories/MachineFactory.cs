using GWGUI.Emulation.Sega.Emulators.Kronos.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Sega.Emulators.Kronos.Factories;

internal sealed class MachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Kronos;
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.StV;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
