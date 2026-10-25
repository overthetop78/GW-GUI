using GWGUI.Emulation.Nintendo.Emulators.Citra2018.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.Citra2018.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Citra2018;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
