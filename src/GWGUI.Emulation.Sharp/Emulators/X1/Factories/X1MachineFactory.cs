using GWGUI.Emulation.Sharp.Emulators.X1.Constants;
using FirmwarePreparation = GWGUI.Emulation.Sharp.Emulators.X1.Functions.FirmwareRuntimeFunctions;
using RuntimeOptions = GWGUI.Emulation.Sharp.Emulators.X1.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Sharp.Emulators.X1.Factories;

internal sealed class X1MachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry)
{
    protected override bool RequiresResetRestart => true;
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => GWGUI.Emulation.Sharp.Emulators.X1.Functions.MediaRuntimeFunctions.Prepare(configuration, sessionDirectory, supportedExtensions);
    protected override bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => true;
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => FirmwarePreparation.Prepare(configuration, systemDirectory);
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => RuntimeOptions.Prepare(configuration);
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => [new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, MediaRuntimeConstants.FloppyExtensions, RequiresMachineRecreation: true)];
}
