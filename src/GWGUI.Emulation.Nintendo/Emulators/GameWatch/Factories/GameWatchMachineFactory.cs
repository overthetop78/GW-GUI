using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.GameWatch.Factories;

internal sealed class GameWatchMachineFactory : CoreMachineFactory, IEmulatorMachineDefaults
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.GameWatch;
    public bool IsDefaultFor(string machineId) => machineId is ModelConstants.GameWatch;

    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
