namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration) =>
        new Engine().Adapter(configuration).DescribeStorage(configuration);
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings) =>
        new Engine().Adapter(configuration).ApplyStorage(configuration, settings);
}
