using GWGUI.Emulation.Sega.Emulators.GenesisPlusGXWide.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGXWide.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.GenesisPlusGXWide;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
