using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.MelonDs.Factories;

internal sealed class MelonDsMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.MelonDs;
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.NintendoDs;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
