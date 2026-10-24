using GWGUI.Emulation.Nintendo.Emulators.Snes9x.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x.Factories;

internal sealed class Snes9xMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Snes9x;
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.Snes;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
