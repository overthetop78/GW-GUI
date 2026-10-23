using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sharp.Emulators.X1.Functions;
using GWGUI.Emulation.Sharp.Emulators.X1.Constants;
using Settings = GWGUI.Emulation.Sharp.Emulators.X1.Functions.SettingsDescriptionFunctions;
using FirmwarePreparation = GWGUI.Emulation.Sharp.Emulators.X1.Functions.FirmwareRuntimeFunctions;
using RuntimeOptions = GWGUI.Emulation.Sharp.Emulators.X1.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Sharp.Emulators.X1.Factories;

internal sealed class X1MachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter
{
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => Settings.GetOptions(configuration);

    protected override bool RequiresResetRestart => true;
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => GWGUI.Emulation.Sharp.Emulators.X1.Functions.MediaRuntimeFunctions.Prepare(configuration, sessionDirectory, supportedExtensions);
    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => slot != EmulationMediaSlot.Floppy0;
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => FirmwarePreparation.Prepare(configuration, systemDirectory);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => RuntimeOptions.Prepare(configuration);
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => [new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, MediaRuntimeConstants.FloppyExtensions)];
}
