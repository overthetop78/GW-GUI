using GWGUI.Emulation.Nintendo.Emulators.TgbDual.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.TgbDual.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.TgbDual;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
