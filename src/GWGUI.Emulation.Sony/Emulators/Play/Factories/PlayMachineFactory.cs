using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Play.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Play.Factories;

internal sealed class PlayMachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Play;
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    public override bool HasFixedMemoryCardDirectories => true;
    public override string? GetMemoryCardDirectory(int index) => Emulators.Play.Functions.MemoryCardFunctions.DirectoryFor(index);
    public override int GetControllerPortCount(MachineConfiguration configuration) => ControllerPortConstants.OneControllerPort;
}
