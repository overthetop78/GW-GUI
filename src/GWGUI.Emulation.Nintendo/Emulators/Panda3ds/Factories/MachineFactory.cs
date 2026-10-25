using GWGUI.Emulation.Nintendo.Emulators.Panda3ds.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Panda3ds.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Panda3ds;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
