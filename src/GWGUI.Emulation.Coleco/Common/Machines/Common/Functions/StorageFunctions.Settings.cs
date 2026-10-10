using System.IO;

namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var devices = new List<EmulationMediaDevice>
        {
            new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, SettingsConstants.CartridgeExtensions,
                RequiresMachineRecreation: true, ImageDirectory: configuration.Folders.Cartridges, IsPermanent: true)
        };
        if (configuration.Model == MachineModel.Adam)
        {
            for (var index = 0; index < StorageConstants.DriveCount; index++)
            {
                devices.Add(new(new(EmulationMediaCategory.FloppyDrive, index), EmulationMediaType.Floppy,
                    SettingsConstants.FloppyExtensions, ImageDirectory: configuration.Folders.Floppies,
                    ModelResourceKey: "Emulation.Coleco.Storage.Disk"));
                devices.Add(new(new(EmulationMediaCategory.CassetteDrive, index), EmulationMediaType.Cassette,
                    SettingsConstants.DataPackExtensions, ImageDirectory: configuration.Folders.DataPacks,
                    ModelResourceKey: "Emulation.Coleco.Storage.DataPack"));
            }
        }
        return new(devices, configuration.ConfiguredSlots, configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray());
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices.ToDictionary(device => device.Slot);
        if (settings.ConfiguredSlots.Any(slot => !devices.ContainsKey(slot))
            || settings.MountedMedia.Any(media => !devices.TryGetValue(media.Slot, out var device)
                || device.MediaType != media.Type
                || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var result = configuration with
        {
            ConfiguredSlots = settings.ConfiguredSlots.ToArray(),
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToColeco(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
