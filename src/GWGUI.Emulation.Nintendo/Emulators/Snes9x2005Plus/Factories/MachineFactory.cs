using GWGUI.Emulation.Nintendo.Emulators.Snes9x2005Plus.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x2005Plus.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Snes9x2005Plus;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
