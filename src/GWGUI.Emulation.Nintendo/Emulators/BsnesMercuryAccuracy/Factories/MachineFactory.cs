using GWGUI.Emulation.Nintendo.Emulators.BsnesMercuryAccuracy.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Nintendo.Emulators.BsnesMercuryAccuracy.Factories;

internal sealed class MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.BsnesMercuryAccuracy;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
}
