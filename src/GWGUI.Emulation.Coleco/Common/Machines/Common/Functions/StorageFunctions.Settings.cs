using System.IO;
using CartridgeOptions = GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants.OptionConstants;

namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var devices = new List<EmulationMediaDevice>
        {
            new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, SettingsConstants.CartridgeExtensions,
                RequiresMachineRecreation: true, ImageDirectory: configuration.Folders.Cartridges, IsPermanent: true,
                ConfigurationKind: configuration.Core == Emulator.GearColeco ? EmulationStorageConfigurationKind.CartridgeSlot : EmulationStorageConfigurationKind.None,
                ConfigurationFields: CartridgeFields(configuration))
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
        return new(devices, configuration.ConfiguredSlots,
            configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray(),
            configuration.Core == Emulator.GearColeco ? [new(EmulationMediaSlot.Cartridge0, Cartridge: new CartridgeSlotSettings(Options: CartridgeFields(configuration).ToDictionary(field => field.Id, field => field.Value!)))] : []);
    }

    private static IReadOnlyList<EmulationSettingsField> CartridgeFields(MachineConfiguration configuration) =>
        configuration.Core == Emulator.GearColeco ? [SettingsDescriptionFunctions.Native(configuration, CoreOptionValidationFunctions.ForCore(configuration.Core).Single(option => option.Key == CartridgeOptions.OptionMapper), EmulationMachineTab.Storage, "Emulation.Storage.Cartridges")] : [];

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices.ToDictionary(device => device.Slot);
        if (settings.ConfiguredSlots.Any(slot => !devices.ContainsKey(slot))
            || settings.MountedMedia.Any(media => !devices.TryGetValue(media.Slot, out var device) || device.MediaType != media.Type || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        foreach (var deviceSettings in settings.DeviceSettings ?? [])
        {
            if (!devices.ContainsKey(deviceSettings.Slot)) throw new ArgumentException(nameof(settings));
            foreach (var option in deviceSettings.Cartridge?.Options ?? new Dictionary<string, string>())
            {
                if (deviceSettings.Slot != EmulationMediaSlot.Cartridge0 || configuration.Core != Emulator.GearColeco
                    || option.Key != CartridgeOptions.OptionMapper)
                    throw new ArgumentException(nameof(settings));
                options[option.Key] = option.Value;
            }
        }
        var result = configuration with
        {
            Options = options,
            ConfiguredSlots = settings.ConfiguredSlots.ToArray(),
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToColeco(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
