namespace GWGUI.Emulation.Acorn.Common.Interfaces;

internal interface IEmulatorStorageSettings : IEmulatorMediaCapabilities
{
    IReadOnlyList<EmulationStorageDeviceSettings> DescribeDeviceSettings(MachineConfiguration configuration) => [];
    MachineConfiguration ApplyDeviceSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings) => configuration;
}
