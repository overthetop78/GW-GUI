using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Factories;

internal sealed class DolphinMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Dolphin;
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.GameCube or ModelConstants.Wii;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
