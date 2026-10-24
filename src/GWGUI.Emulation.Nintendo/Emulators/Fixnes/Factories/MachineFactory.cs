using GWGUI.Emulation.Nintendo.Emulators.Fixnes.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Fixnes.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Fixnes;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
