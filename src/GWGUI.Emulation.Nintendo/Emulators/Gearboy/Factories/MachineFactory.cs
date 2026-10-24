using GWGUI.Emulation.Nintendo.Emulators.Gearboy.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Gearboy.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Gearboy;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
