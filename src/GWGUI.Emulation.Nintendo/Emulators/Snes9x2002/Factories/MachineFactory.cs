using GWGUI.Emulation.Nintendo.Emulators.Snes9x2002.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x2002.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Snes9x2002;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
