using GWGUI.Emulation.Sega.Emulators.Ymir.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Sega.Emulators.Ymir.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Ymir;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
