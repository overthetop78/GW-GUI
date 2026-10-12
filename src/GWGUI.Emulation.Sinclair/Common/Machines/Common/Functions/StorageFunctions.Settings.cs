using System.IO;

namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Functions;
internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        if (configuration.Core == Emulator.None)
            return new(Array.Empty<EmulationMediaDevice>(), Array.Empty<EmulationMediaSlot>(), Array.Empty<EmulationMedia>());
        var devices = new Engine().Adapter(configuration).DescribeMediaDevices(configuration).ToArray();
        return new(devices, configuration.ConfiguredSlots, configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        if (settings.ConfiguredSlots.Distinct().Count() != settings.ConfiguredSlots.Count || settings.ConfiguredSlots.Any(slot => !devices.Any(device => device.Slot == slot)) || settings.MountedMedia.Select(media => media.Slot).Distinct().Count() != settings.MountedMedia.Count || settings.DeviceSettings?.Count > 0)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        foreach (var media in settings.MountedMedia)
        {
            var device = devices.SingleOrDefault(device => device.Slot == media.Slot);
            if (device is null || !settings.ConfiguredSlots.Contains(media.Slot) || media.Type != device.MediaType || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        }

        var result = configuration with
        {
            ConfiguredSlots = settings.ConfiguredSlots.ToArray(),
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToSinclair(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}