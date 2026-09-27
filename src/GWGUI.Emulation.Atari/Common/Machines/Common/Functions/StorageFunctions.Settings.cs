using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static partial class StorageSettingsFunctions
{
    private const string DeviceOptionPrefix = StorageSettingsFunctionsConstants.StorageDevice;
    private const string ModelOptionPrefix = StorageSettingsFunctionsConstants.StorageModel;
    private const string SpeedOptionPrefix = StorageSettingsFunctionsConstants.StorageSpeed;
    private const string WriteProtectedOptionPrefix = StorageSettingsFunctionsConstants.StorageWriteProtected;
    private const string RedirectWritesOptionPrefix = StorageSettingsFunctionsConstants.StorageRedirectWrites;
    private const string InterfaceOptionPrefix = StorageSettingsFunctionsConstants.StorageInterface;

    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var compatibility = CompatibilityCatalog.Get(configuration.Model);
        var devices = compatibility.Media
            .Where(rule => rule.Availability == MediaAvailability.Available
                && rule.Category != MediaCategory.Directory)
            .SelectMany(rule => rule.Slots.Select(slot => new EmulationMediaDevice(slot,
                ToMediaType(rule.Category), rule.Category == MediaCategory.HardDisk
                    ? HardDiskFormats.For(configuration.Model).Select(format => format.Extension).ToArray()
                    : rule.Category == MediaCategory.Cartridge
                        ? CartridgeExtensions(configuration)
                        : Extensions(rule.Category),
                StorageConfigurationFunctions.IsRemovable(rule.Category),
                rule.Category == MediaCategory.Cartridge && compatibility.Core == Emulator.Atari800,
                DisplayLabel(configuration.Model, slot),
                rule.Category == MediaCategory.Floppy
                    ? FloppyOptions(configuration.Model, configuration.Folders.Floppies) : null,
                Interfaces(configuration.Model, rule.Category),
                rule.Category == MediaCategory.HardDisk
                    ? configuration.Folders.HardDisks : null,
                IsPermanent: StorageConfigurationFunctions.IsPrimaryDevice(configuration.Model, slot),
                HardDiskFormats: rule.Category == MediaCategory.HardDisk
                    ? HardDiskFormats.For(configuration.Model) : null,
                ConfigurationKind: rule.Category switch
                {
                    MediaCategory.Floppy => EmulationStorageConfigurationKind.FloppyDrive,
                    MediaCategory.HardDisk => EmulationStorageConfigurationKind.HardDiskDrive,
                    _ => EmulationStorageConfigurationKind.None
                })))
            .ToArray();
        var primary = StorageConfigurationFunctions.PrimaryDevice(configuration.Model)?.Slot;
        var configured = configuration.Options
            .Where(option => option.Key.StartsWith(DeviceOptionPrefix, StringComparison.Ordinal)
                && EmulationMediaSlot.TryParse(option.Key[DeviceOptionPrefix.Length..], out _))
            .Select(option =>
            {
                EmulationMediaSlot.TryParse(option.Key[DeviceOptionPrefix.Length..], out var slot);
                return slot;
            })
            .Concat(configuration.Media.Select(media => media.Slot))
            .Concat(primary is { } slot ? [slot] : [])
            .Distinct().ToArray();
        var mounted = configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon)
            .OfType<EmulationMedia>().ToArray();
        var settings = configured.Select(slot => DeviceSettings(configuration, slot)).ToArray();
        return new EmulationStorageSettings(devices, configured, mounted, settings);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        EmulationStorageSettings settings)
    {
        var options = configuration.Options
            .Where(option => !IsDeviceOption(option.Key))
            .ToDictionary(option => option.Key, option => option.Value);
        foreach (var slot in settings.ConfiguredSlots)
        {
            if (StorageConfigurationFunctions.IsPrimaryDevice(configuration.Model, slot)) continue;
            var device = settings.AvailableDevices.First(item => item.Slot == slot);
            options[$"{DeviceOptionPrefix}{slot}"] = ToAtariCategory(device.MediaType).ToString();
        }
        foreach (var deviceSettings in settings.DeviceSettings ?? [])
        {
            if (deviceSettings.Floppy is { } floppy)
            {
                options[$"{ModelOptionPrefix}{deviceSettings.Slot}"] = floppy.Model;
                options[$"{SpeedOptionPrefix}{deviceSettings.Slot}"] = floppy.Speed;
                options[$"{WriteProtectedOptionPrefix}{deviceSettings.Slot}"] = floppy.WriteProtected.ToString();
                options[$"{RedirectWritesOptionPrefix}{deviceSettings.Slot}"] = floppy.RedirectWrites.ToString();
            }
            if (!string.IsNullOrWhiteSpace(deviceSettings.InterfaceId))
            {
                var device = settings.AvailableDevices.First(item => item.Slot == deviceSettings.Slot);
                var selectedInterface = device.InterfaceChoices?.FirstOrDefault(choice =>
                    string.Equals(choice.Id, deviceSettings.InterfaceId, StringComparison.OrdinalIgnoreCase));
                if (selectedInterface is not null)
                    options[$"{InterfaceOptionPrefix}{deviceSettings.Slot}"] = selectedInterface.Id;
            }
        }
        var media = settings.MountedMedia.Select(item =>
        {
            var converted = EmulationMediaConversionFunctions.ToAtari(item, configuration.Media);
            if (item.Type != EmulationMediaType.HardDisk) return converted;
            var format = HardDiskFormats.For(configuration.Model).FirstOrDefault(candidate =>
                string.Equals(candidate.Extension, Path.GetExtension(item.Path), StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException(ErrorMessages.StorageExtensionInvalid);
            var selected = settings.DeviceSettings?.FirstOrDefault(device => device.Slot == item.Slot)?.InterfaceId;
            if (selected is not null && !string.Equals(selected, format.InterfaceName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(ErrorMessages.StorageExtensionInvalid);
            return converted with { StorageBus = format.Id == "atari-acsi" ? StorageBus.Acsi : StorageBus.Ide };
        }).ToArray();
        return configuration with { Media = media, Options = options };
    }

    private static EmulationMediaType ToMediaType(MediaCategory category) => category switch
    {
        MediaCategory.Floppy => EmulationMediaType.Floppy,
        MediaCategory.HardDisk => EmulationMediaType.HardDisk,
        MediaCategory.Cassette => EmulationMediaType.Cassette,
        MediaCategory.Cartridge => EmulationMediaType.Cartridge,
        MediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };

    private static MediaCategory ToAtariCategory(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Floppy => MediaCategory.Floppy,
        EmulationMediaType.HardDisk => MediaCategory.HardDisk,
        EmulationMediaType.Cassette => MediaCategory.Cassette,
        EmulationMediaType.Cartridge => MediaCategory.Cartridge,
        EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
