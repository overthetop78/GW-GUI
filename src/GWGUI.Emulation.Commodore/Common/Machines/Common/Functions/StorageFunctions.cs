using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration) =>
        EmulatorCatalog.CreateAdapter(configuration.Core).DescribeStorage(configuration);
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings) =>
        EmulatorCatalog.CreateAdapter(configuration.Core).ApplyStorage(configuration, settings);

    internal static EmulationStorageSettings DescribeBasic(MachineConfiguration configuration)
    {
        var devices = EmulatorCatalog.CreateAdapter(configuration.Core).GetMediaDevices(configuration);
        var mounted = EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
        var configured = configuration.ConfiguredMediaSlots ?? devices.Where(device => device.IsPermanent)
            .Select(device => device.Slot).Concat(mounted.Select(media => media.Slot)).Distinct().ToArray();
        return new EmulationStorageSettings(devices, configured, mounted);
    }

    internal static MachineConfiguration ApplyBasic(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        return configuration with
        {
            Media = EmulationMediaConversionFunctions.FromCommon(settings.MountedMedia),
            ConfiguredMediaSlots = settings.ConfiguredSlots
        };
    }
}
