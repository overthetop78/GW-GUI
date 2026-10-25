using GWGUI.Emulation.Nintendo.Emulators.BeetleSnes.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.BeetleSnes.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.BeetleSnes;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
