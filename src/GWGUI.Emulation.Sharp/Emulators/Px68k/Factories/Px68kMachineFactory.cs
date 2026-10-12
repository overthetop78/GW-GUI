using GWGUI.Emulation.Sharp.Emulators.Px68k.Constants;
using FirmwarePreparation = GWGUI.Emulation.Sharp.Emulators.Px68k.Functions.FirmwareRuntimeFunctions;
using RuntimeOptions = GWGUI.Emulation.Sharp.Emulators.Px68k.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Factories;

internal sealed class Px68kMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    protected override bool SupportsPointerCapture => true;
    protected override bool SupportsNoContent => true;
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => GWGUI.Emulation.Sharp.Emulators.Px68k.Functions.MediaRuntimeFunctions.Prepare(configuration, sessionDirectory);
    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => true;
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => FirmwarePreparation.Prepare(configuration, systemDirectory);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => RuntimeOptions.Prepare(configuration);
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => [
        new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, MediaRuntimeConstants.FloppyExtensions, RequiresMachineRecreation: true),
        new(EmulationMediaSlot.Floppy1, EmulationMediaType.Floppy, MediaRuntimeConstants.FloppyExtensions, RequiresMachineRecreation: true),
        new(EmulationMediaSlot.HardDisk0, EmulationMediaType.HardDisk, MediaRuntimeConstants.HardDiskExtensions, RequiresMachineRecreation: true)
    ];
}
