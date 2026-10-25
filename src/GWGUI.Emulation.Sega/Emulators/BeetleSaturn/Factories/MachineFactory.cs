using GWGUI.Emulation.Sega.Emulators.BeetleSaturn.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Sega.Emulators.BeetleSaturn.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.BeetleSaturn;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
