using GWGUI.Emulation.Nintendo.Emulators.Fixgb.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Fixgb.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Fixgb;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
