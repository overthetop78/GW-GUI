using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;

internal sealed class Pcsx2MachineFactory : CoreMachineFactory
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.Pcsx2;
    protected override CoreDefinition CoreDefinition => Pcsx2Constants.Definition;
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => FileSystemFunctions.Describe(configuration,
        CoreSettingsFunctions.Blocks(Dictionaries.OptionCatalog.All, Pcsx2OptionFunctions.ToNative(configuration)));
    public override void ValidateConfiguration(MachineConfiguration configuration) => FileSystemFunctions.Validate(configuration);
    public override MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) =>
        Pcsx2OptionFunctions.ApplyVideoSettings(configuration, graphicsApi);
    public override IReadOnlyList<PeripheralCategory> GetUsbPeripherals(MachineConfiguration configuration, int port) =>
        port is 0 or 1 ? [PeripheralCategory.None, PeripheralCategory.Keyboard, PeripheralCategory.Mouse] : [PeripheralCategory.None];
    public override IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => Functions.StorageFunctions.Describe(configuration);
    public override bool IsCpuClock(string fieldId) => false;
    public override IReadOnlyList<EmulationSettingsBlock> GetMachineSettingsBlocks(MachineConfiguration configuration, EmulatorManagementContext? context = null) =>
        SettingsDescriptionFunctions.MachineBlocks(this, configuration, context);
    public override IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) =>
        SettingsDescriptionFunctions.StorageFields(this, configuration, category);
    protected override MachineConfiguration Configure(MachineConfiguration configuration) => Pcsx2OptionFunctions.ToNative(configuration);
}