using System.IO;

namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;
internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var devices = configuration.Core != Emulator.None && new Engine().Adapter(configuration)is IEmulatorMediaCapabilities capabilities ? capabilities.DescribeMediaDevices(configuration) : [];
        return new(devices, configuration.ConfiguredSlots, configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        if (settings.ConfiguredSlots.Distinct().Count() 
            != settings.ConfiguredSlots.Count 
            || settings.ConfiguredSlots.Any(slot => !devices.Any(device => device.Slot 
            == slot)) 
            || settings.MountedMedia.Select(media => media.Slot).Distinct().Count() 
            != settings.MountedMedia.Count 
            || settings.DeviceSettings?.Count 
            > 0)
            throw new ArgumentException(nameof(settings));
        foreach (var media in settings.MountedMedia)
        {
            var device = devices.SingleOrDefault(device => device.Slot == media.Slot);
            if (device is null || !settings.ConfiguredSlots.Contains(media.Slot) || media.Type != device.MediaType || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException(nameof(settings));
        }

        var result = configuration with
        {
            ConfiguredSlots = settings.ConfiguredSlots.ToArray(),
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToModule(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}