using GWGUI.Emulation.Sega.Emulators.VeMUlator.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Sega.Emulators.VeMUlator.Factories;

internal sealed class MachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.VeMUlator;
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.DreamcastVmu;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
