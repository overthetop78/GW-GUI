using GWGUI.Emulation.Nintendo.Emulators.Bsnes2014Performance.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Bsnes2014Performance.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Bsnes2014Performance;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
