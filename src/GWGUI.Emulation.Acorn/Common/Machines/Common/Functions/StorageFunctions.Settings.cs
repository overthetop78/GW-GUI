using System.IO;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        IReadOnlyList<EmulationMediaDevice> devices = configuration.Core != Emulator.None && new Engine().Adapter(configuration) is { } storage ? storage.DescribeMediaDevices(configuration) : [];
        var deviceSettings = configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorStorageSettings adapter ? adapter.DescribeDeviceSettings(configuration) : [];
        return new EmulationStorageSettings(devices, devices.Select(device => device.Slot).ToArray(), configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray(), deviceSettings);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        var available = devices.Select(device => device.Slot).ToHashSet();
        if (settings.ConfiguredSlots.Any(slot => !available.Contains(slot))
           || settings.MountedMedia.Any(media => !settings.ConfiguredSlots.Contains(media.Slot)
               || !devices.Any(device => device.Slot == media.Slot && device.MediaType == media.Type
                    && device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase))))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var configured = settings.DeviceSettings ?? [];
        if (configured.Select(item => item.Slot).Distinct().Count() != configured.Count || configured.Any(item => !settings.ConfiguredSlots.Contains(item.Slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var result = configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorStorageSettings adapter
            ? adapter.ApplyDeviceSettings(configuration, configured) : configuration;
        result = result with
        {
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToModule(media, result.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
