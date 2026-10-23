using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Factories;
using NativeSettings = GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions.SettingsDescriptionFunctions;
using GWGUI.Emulation.Sinclair.Emulators.EightyOne.Constants;
using NativeMedia = GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions.MediaRuntimeFunctions;
using NativeOptions = GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions.RuntimeOptionFunctions;

namespace GWGUI.Emulation.Sinclair.Emulators.EightyOne.Factories;

internal sealed class EightyOneMachineFactory() : LibretroMachineFactory(EmulatorConstants.Entry), IEmulatorSettingsAdapter
{
    public override IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model) => [PeripheralCategory.None, PeripheralCategory.CursorJoystick];

    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => NativeSettings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => NativeSettings.GetOptions(configuration);

    public override bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => true;
    internal override LoadedContent? PrepareContent(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions) => NativeMedia.Prepare(configuration, sessionDirectory, supportedExtensions);
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory) { }
    public override IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => NativeOptions.Prepare(configuration);
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => [new(EmulationMediaSlot.Cassette0, EmulationMediaType.Cassette, MediaRuntimeConstants.TapeExtensions, RequiresMachineRecreation: true)];
}
