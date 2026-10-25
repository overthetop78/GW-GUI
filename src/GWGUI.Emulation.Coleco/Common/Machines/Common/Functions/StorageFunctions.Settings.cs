using System.IO;

namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var cartridgeFields = CartridgeFields(configuration);
        var devices = new List<EmulationMediaDevice>
        {
            new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, SettingsConstants.CartridgeExtensions,
                RequiresMachineRecreation: true, ImageDirectory: configuration.Folders.Cartridges, IsPermanent: true,
                ConfigurationKind: cartridgeFields.Count > 0 ? EmulationStorageConfigurationKind.CartridgeSlot : EmulationStorageConfigurationKind.None,
                ConfigurationFields: cartridgeFields)
        };
        if (configuration.Model == MachineModel.Adam)
        {
            for (var index = 0; index < StorageConstants.DriveCount; index++)
            {
                devices.Add(new(new(EmulationMediaCategory.FloppyDrive, index), EmulationMediaType.Floppy, SettingsConstants.FloppyExtensions, ImageDirectory: configuration.Folders.Floppies, ModelResourceKey: "Emulation.Coleco.Storage.Disk"));
                devices.Add(new(new(EmulationMediaCategory.CassetteDrive, index), EmulationMediaType.Cassette, SettingsConstants.DataPackExtensions,
                    ImageDirectory: configuration.Folders.DataPacks, IsPermanent: index == StorageConstants.FirstDriveIndex,
                    ModelResourceKey: "Emulation.Coleco.Storage.DataPack"));
            }
        }
        return new(devices, configuration.ConfiguredSlots,
            configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray(),
            cartridgeFields.Count > 0 ? [new(EmulationMediaSlot.Cartridge0, Cartridge: new CartridgeSlotSettings(Options: cartridgeFields.ToDictionary(field => field.Id, field => field.Value!)))] : []);
    }

    private static IReadOnlyList<EmulationSettingsField> CartridgeFields(MachineConfiguration configuration) =>
        new Engine().Adapter(configuration) is { } settings ? settings.GetStorageDeviceConfigurationFields(configuration, EmulationMediaCategory.CartridgeSlot) : [];

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices.ToDictionary(device => device.Slot);
        if (settings.ConfiguredSlots.Any(slot => !devices.ContainsKey(slot))
           || settings.MountedMedia.Any(media => !devices.TryGetValue(media.Slot, out var device) || device.MediaType != media.Type || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var cartridgeFields = CartridgeFields(configuration).Select(field => field.Id).ToHashSet(StringComparer.Ordinal);
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        foreach (var deviceSettings in settings.DeviceSettings ?? [])
        {
            if (!devices.ContainsKey(deviceSettings.Slot)) throw new ArgumentException(nameof(settings));
            foreach (var option in deviceSettings.Cartridge?.Options ?? new Dictionary<string, string>())
            {
                if (deviceSettings.Slot != EmulationMediaSlot.Cartridge0 || !cartridgeFields.Contains(option.Key))
                    throw new ArgumentException(nameof(settings));
                options[option.Key] = option.Value;
            }
        }
        var result = configuration with
        {
            Options = options,
            ConfiguredSlots = settings.ConfiguredSlots.ToArray(),
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToModule(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
