using GWGUI.Emulation.Nintendo.Emulators.BsnesJg.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.BsnesJg.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.BsnesJg;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
