using GWGUI.Emulation.Nintendo.Emulators.ParallelN64.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.ParallelN64.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.ParallelN64;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
