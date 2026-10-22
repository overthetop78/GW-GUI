using CommonCoreHostFunctions = GWGUI.Emulation.Sharp.Common.Functions.CoreHostFunctions;
using System.IO;

namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Functions;
internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        if (configuration.Core == Emulator.None)
            return new(Array.Empty<EmulationMediaDevice>(), Array.Empty<EmulationMediaSlot>(), Array.Empty<EmulationMedia>());
        var devices = new Engine().Adapter(configuration) is IEmulatorMediaCapabilities storage ? storage.DescribeMediaDevices(configuration).ToArray() : [];
        var interfaces = devices.Where(device => configuration.ConfiguredSlots.Contains(device.Slot) && device.HardDiskFormats?.Count > 0)
            .Select(device => new EmulationStorageDeviceSettings(device.Slot, InterfaceId: device.HardDiskFormats![0].InterfaceName.ToLowerInvariant())).ToArray();
        return new(devices, configuration.ConfiguredSlots, configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray(), interfaces);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        if (CommonCoreHostFunctions.MediaRules.HasInvalidStorageSlots(settings, devices)
            || settings.DeviceSettings?.Any(setting => setting.Floppy is not null || setting.Cartridge is not null || setting.CompactDisc is not null || setting.MemoryCardPath is not null
                || !settings.ConfiguredSlots.Contains(setting.Slot)
                || !devices.Any(device => device.Slot == setting.Slot && device.HardDiskFormats?.Count > 0
                    && (string.IsNullOrEmpty(setting.InterfaceId) || device.HardDiskFormats.Any(format => string.Equals(format.InterfaceName, setting.InterfaceId, StringComparison.OrdinalIgnoreCase))))) == true)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        foreach (var media in settings.MountedMedia)
        {
            if (CommonCoreHostFunctions.MediaRules.IsInvalidMountedMedia(media, devices, settings.ConfiguredSlots))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        }

        var result = configuration with
        {
            ConfiguredSlots = settings.ConfiguredSlots.ToArray(),
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToSharp(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
