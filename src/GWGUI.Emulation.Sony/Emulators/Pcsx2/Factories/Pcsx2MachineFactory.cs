using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;

internal sealed class Pcsx2MachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => Pcsx2Constants.Definition;
    public override IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration) => FileSystemFunctions.Describe(configuration,
        CoreSettingsFunctions.Blocks(Dictionaries.OptionCatalog.All, Pcsx2OptionFunctions.ToNative(configuration)));
    public override void ValidateConfiguration(MachineConfiguration configuration) => FileSystemFunctions.Validate(configuration);
    public override MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) =>
        Pcsx2OptionFunctions.ApplyVideoSettings(configuration, graphicsApi);
    public override IReadOnlyList<ControllerType> GetUsbControllerTypes(MachineConfiguration configuration, int port) =>
        port is 0 or 1 ? [ControllerType.None, ControllerType.Keyboard, ControllerType.Mouse] : [ControllerType.None];
    public override IReadOnlyList<EmulationMediaDevice> GetStorageDevices(MachineConfiguration configuration) => Functions.StorageFunctions.Describe(configuration);
    public override bool IsCpuClock(string fieldId) => false;
    public override IReadOnlyList<EmulationSettingsBlock> GetMachineSettingsBlocks(MachineConfiguration configuration) =>
        CoreSettingsFunctions.MachineBlocks(this, configuration);
    public override IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) =>
        CoreSettingsFunctions.StorageFields(this, configuration, category);
    protected override MachineConfiguration Configure(MachineConfiguration configuration) => Pcsx2OptionFunctions.ToNative(configuration);
}