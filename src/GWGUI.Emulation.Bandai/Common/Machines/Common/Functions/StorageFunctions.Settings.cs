using System.IO;

namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var devices = new List<EmulationMediaDevice>();
        if (configuration.Family == MachineFamily.WonderSwan)
            devices.Add(new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge,
                SettingsConstants.CartridgeExtensions, RequiresMachineRecreation: true,
                ImageDirectory: configuration.Folders.Cartridges, IsPermanent: true));
        else if (configuration.Family == MachineFamily.Playdia)
            devices.Add(new(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc,
                SettingsConstants.CompactDiscExtensions, RequiresMachineRecreation: true,
                ImageDirectory: configuration.Folders.CompactDiscs, IsPermanent: true));
        return new(devices, devices.Select(device => device.Slot).ToArray(),
            configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices.ToDictionary(device => device.Slot);
        if (!settings.ConfiguredSlots.ToHashSet().SetEquals(devices.Keys)
            || settings.MountedMedia.Any(media => !devices.TryGetValue(media.Slot, out var device)
                || device.MediaType != media.Type
                || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var result = configuration with
        {
            Media = settings.MountedMedia.Select(media =>
                EmulationMediaConversionFunctions.ToBandai(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
