using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sharp.Emulators.Px68k.Functions;
using GWGUI.Emulation.Sharp.Emulators.Px68k.Constants;
using Settings = GWGUI.Emulation.Sharp.Emulators.Px68k.Functions.SettingsDescriptionFunctions;
using FirmwarePreparation = GWGUI.Emulation.Sharp.Emulators.Px68k.Functions.FirmwareRuntimeFunctions;
using RuntimeOptions = GWGUI.Emulation.Sharp.Emulators.Px68k.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Factories;
internal sealed class Px68kMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter
{
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions(configuration);

    public override bool SupportsPointer(MachineConfiguration configuration) => true;
    protected override bool SupportsNoContent => true;

    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => GWGUI.Emulation.Sharp.Emulators.Px68k.Functions.MediaRuntimeFunctions.Prepare(configuration, sessionDirectory);
    public override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => FirmwarePreparation.Prepare(configuration, systemDirectory);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => RuntimeOptions.Prepare(configuration);
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => [
        new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, MediaRuntimeConstants.FloppyExtensions, RequiresMachineRecreation: true),
        new(EmulationMediaSlot.Floppy1, EmulationMediaType.Floppy, MediaRuntimeConstants.FloppyExtensions, RequiresMachineRecreation: true),
        new(EmulationMediaSlot.HardDisk0, EmulationMediaType.HardDisk, MediaRuntimeConstants.HardDiskExtensions, RequiresMachineRecreation: true,
            ImageDirectory: configuration.Folders.HardDisks, HardDiskFormats: HardDiskConstants.Formats, ConfigurationKind: EmulationStorageConfigurationKind.HardDiskDrive)
    ];
}
