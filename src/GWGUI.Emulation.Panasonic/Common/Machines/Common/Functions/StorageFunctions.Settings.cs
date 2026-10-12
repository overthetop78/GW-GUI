using System.IO;

namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var engine = new Engine();
        var adapter = configuration.Core == Emulator.None ? engine.Adapters(configuration.MachineId).FirstOrDefault() : engine.Adapter(configuration);
        var devices = adapter is IEmulatorMediaCapabilities capabilities ? capabilities.DescribeMediaDevices(configuration) : [];
        return new(devices, configuration.ConfiguredSlots, configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        if (!settings.ConfiguredSlots.SequenceEqual(new[] { EmulationMediaSlot.Cd0 }) || settings.ConfiguredSlots.Distinct().Count() != settings.ConfiguredSlots.Count || settings.ConfiguredSlots.Any(slot => !devices.Any(device => device.Slot == slot)) || settings.MountedMedia.Select(media => media.Slot).Distinct().Count() != settings.MountedMedia.Count || settings.DeviceSettings?.Count > 0)
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
