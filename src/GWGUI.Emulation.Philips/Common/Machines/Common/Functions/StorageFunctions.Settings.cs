using System.IO;

namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var slot = ConfigurationFunctions.DefaultSlots(configuration.Model).Single();
        var device = configuration.Family switch
        {
            MachineFamily.Videopac => new EmulationMediaDevice(slot, EmulationMediaType.Cartridge,
                StorageSettingsConstants.CartridgeExtensions, RequiresMachineRecreation: true,
                ImageDirectory: configuration.Folders.Cartridges, IsPermanent: true),
            MachineFamily.P2000 => new EmulationMediaDevice(slot, EmulationMediaType.Cassette, StorageSettingsConstants.CassetteExtensions, RequiresMachineRecreation: true, ImageDirectory: configuration.Folders.Cassettes, IsPermanent: true),
            MachineFamily.Cdi => new EmulationMediaDevice(slot, EmulationMediaType.CompactDisc,
                StorageSettingsConstants.SameCdiExtensions,
                RequiresMachineRecreation: true,
                ImageDirectory: configuration.Folders.CompactDiscs, IsPermanent: true),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration))
        };
        return new([device], configuration.ConfiguredSlots, configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var device = Describe(configuration).AvailableDevices.Single();
        if (settings.ConfiguredSlots.Count != 1 || settings.ConfiguredSlots[0] != device.Slot
            || settings.MountedMedia.Count > 1 || settings.DeviceSettings?.Count > 0
            || settings.MountedMedia.Any(media => media.Slot != device.Slot || media.Type != device.MediaType
                || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)
                || (configuration.Family == MachineFamily.P2000 && !media.IsReadOnly)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var result = configuration with
        {
            ConfiguredSlots = settings.ConfiguredSlots.ToArray(),
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToPhilips(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
