using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariStorageSettingsFunctions
{
    private const string DeviceOptionPrefix = AtariStorageSettingsFunctionsConstants.StorageDevice;
    private const string ModelOptionPrefix = AtariStorageSettingsFunctionsConstants.StorageModel;
    private const string SpeedOptionPrefix = AtariStorageSettingsFunctionsConstants.StorageSpeed;
    private const string WriteProtectedOptionPrefix = AtariStorageSettingsFunctionsConstants.StorageWriteProtected;
    private const string RedirectWritesOptionPrefix = AtariStorageSettingsFunctionsConstants.StorageRedirectWrites;
    private const string InterfaceOptionPrefix = AtariStorageSettingsFunctionsConstants.StorageInterface;

    internal static EmulationStorageSettings Describe(AtariMachineConfiguration configuration)
    {
        var compatibility = AtariCompatibilityCatalog.Get(configuration.Model);
        var devices = compatibility.Media
            .Where(rule => rule.Availability == AtariMediaAvailability.Available
                && rule.Category != AtariMediaCategory.Directory)
            .SelectMany(rule => rule.Slots.Select(slot => new EmulationMediaDevice(slot,
                ToMediaType(rule.Category), Extensions(rule.Category),
                AtariStorageConfigurationFunctions.IsRemovable(rule.Category),
                rule.Category == AtariMediaCategory.Cartridge && compatibility.Core == AtariEmulator.Atari800,
                DisplayLabel(configuration.Model, slot),
                rule.Category == AtariMediaCategory.Floppy
                    ? FloppyOptions(configuration.Model, configuration.Folders.Floppies) : null,
                Interfaces(configuration.Model, rule.Category),
                rule.Category == AtariMediaCategory.HardDisk
                    ? configuration.Folders.HardDisks : null,
                IsPermanent: AtariStorageConfigurationFunctions.IsPrimaryDevice(configuration.Model, slot))))
            .ToArray();
        var primary = AtariStorageConfigurationFunctions.PrimaryDevice(configuration.Model)?.Slot;
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

    internal static AtariMachineConfiguration Apply(AtariMachineConfiguration configuration,
        EmulationStorageSettings settings)
    {
        var options = configuration.Options
            .Where(option => !IsDeviceOption(option.Key))
            .ToDictionary(option => option.Key, option => option.Value);
        foreach (var slot in settings.ConfiguredSlots)
        {
            if (AtariStorageConfigurationFunctions.IsPrimaryDevice(configuration.Model, slot)) continue;
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
                if (device.InterfaceChoices?.Any(choice => choice.Id == deviceSettings.InterfaceId) == true)
                    options[$"{InterfaceOptionPrefix}{deviceSettings.Slot}"] = deviceSettings.InterfaceId;
            }
        }
        var media = settings.MountedMedia.Select(item =>
            EmulationMediaConversionFunctions.ToAtari(item, configuration.Media)).ToArray();
        return new AtariMachineConfiguration(configuration.Model, configuration.Firmwares, media, options,
            configuration.Input, configuration.Id, configuration.SchemaVersion, configuration.AudioEnabled,
            configuration.VideoRenderer, configuration.Folders, configuration.VideoProcessing);
    }

    private static EmulationMediaType ToMediaType(AtariMediaCategory category) => category switch
    {
        AtariMediaCategory.Floppy => EmulationMediaType.Floppy,
        AtariMediaCategory.HardDisk => EmulationMediaType.HardDisk,
        AtariMediaCategory.Cassette => EmulationMediaType.Cassette,
        AtariMediaCategory.Cartridge => EmulationMediaType.Cartridge,
        AtariMediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };

    private static AtariMediaCategory ToAtariCategory(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Floppy => AtariMediaCategory.Floppy,
        EmulationMediaType.HardDisk => AtariMediaCategory.HardDisk,
        EmulationMediaType.Cassette => AtariMediaCategory.Cassette,
        EmulationMediaType.Cartridge => AtariMediaCategory.Cartridge,
        EmulationMediaType.CompactDisc => AtariMediaCategory.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private static IReadOnlyList<string> Extensions(AtariMediaCategory category) => category switch
    {
        AtariMediaCategory.Floppy => [AtariStorageSettingsFunctionsConstants.St, AtariStorageSettingsFunctionsConstants.Msa, AtariStorageSettingsFunctionsConstants.Stx, AtariStorageSettingsFunctionsConstants.Dim, AtariStorageSettingsFunctionsConstants.Ipf, AtariStorageSettingsFunctionsConstants.Scp, AtariStorageSettingsFunctionsConstants.Atr, AtariStorageSettingsFunctionsConstants.Xfd, AtariStorageSettingsFunctionsConstants.Dcm, AtariStorageSettingsFunctionsConstants.Atx],
        AtariMediaCategory.HardDisk => [AtariStorageSettingsFunctionsConstants.Img, AtariStorageSettingsFunctionsConstants.Hdf, AtariStorageSettingsFunctionsConstants.Vhd],
        AtariMediaCategory.Cassette => [AtariStorageSettingsFunctionsConstants.Cas],
        AtariMediaCategory.Cartridge => [AtariStorageSettingsFunctionsConstants.Car, AtariStorageSettingsFunctionsConstants.Rom, AtariStorageSettingsFunctionsConstants.A26, AtariStorageSettingsFunctionsConstants.A52, AtariStorageSettingsFunctionsConstants.A78, AtariStorageSettingsFunctionsConstants.Lnx, AtariStorageSettingsFunctionsConstants.J64, AtariStorageSettingsFunctionsConstants.Jag],
        AtariMediaCategory.CompactDisc => [AtariStorageSettingsFunctionsConstants.Cue, AtariStorageSettingsFunctionsConstants.Chd, AtariStorageSettingsFunctionsConstants.Iso],
        _ => []
    };

    private static FloppyDriveDialogOptions FloppyOptions(AtariMachineModel model, string? imageDirectory)
    {
        var models = AtariStorageConfigurationFunctions.Family(model) == AtariMachineFamily.St
            ? StFloppyModels(model)
            : EightBitFloppyModels();
        return new FloppyDriveDialogOptions(models, imageDirectory ?? string.Empty,
            string.Join(';', Extensions(AtariMediaCategory.Floppy).Select(extension => $"*{extension}")),
            AtariStorageConfigurationFunctions.Family(model) == AtariMachineFamily.St ? AtariStorageSettingsFunctionsConstants.St : AtariStorageSettingsFunctionsConstants.Atr);
    }

    private static IReadOnlyList<FloppyDriveModelChoice> StFloppyModels(AtariMachineModel model)
    {
        var models = new List<FloppyDriveModelChoice>
        {
            new(AtariStorageSettingsFunctionsConstants.Atarist720, AtariStorageSettingsFunctionsConstants.FormatAtarist720, BlankImageSize: 737_280)
        };
        if (AtariStModelCatalog.Get(model).Storage.Contains(AtariStStorageCapability.FloppyHighDensity))
            models.Add(new FloppyDriveModelChoice(AtariStorageSettingsFunctionsConstants.Atarist1440, AtariStorageSettingsFunctionsConstants.FormatAtarist1440,
                BlankImageSize: 1_474_560));
        return models;
    }

    private static IReadOnlyList<FloppyDriveModelChoice> EightBitFloppyModels() =>
    [
        new(AtariStorageSettingsFunctionsConstants.Atari90, AtariStorageSettingsFunctionsConstants.FormatAtari90, BlankImageSize: 92_160),
        new(AtariStorageSettingsFunctionsConstants.Atari130, AtariStorageSettingsFunctionsConstants.FormatAtari130, BlankImageSize: 133_120),
        new(AtariStorageSettingsFunctionsConstants.Atari180, AtariStorageSettingsFunctionsConstants.FormatAtari180, BlankImageSize: 184_320)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> Interfaces(AtariMachineModel model,
        AtariMediaCategory category)
    {
        if (category != AtariMediaCategory.HardDisk
            || AtariStorageConfigurationFunctions.Family(model) != AtariMachineFamily.St) return [];
        var storage = AtariStModelCatalog.Get(model).Storage;
        var choices = new List<EmulationSettingsChoice>();
        if (storage.Contains(AtariStStorageCapability.Acsi)) choices.Add(InvariantChoice(AtariStorageSettingsFunctionsConstants.Acsi, AtariStorageSettingsFunctionsConstants.ACSI));
        if (storage.Contains(AtariStStorageCapability.Ide)) choices.Add(InvariantChoice(AtariStorageSettingsFunctionsConstants.Ide, AtariStorageSettingsFunctionsConstants.IDE));
        return choices;
    }

    private static EmulationSettingsChoice InvariantChoice(string id, string value) => new(id, string.Empty, value);

    private static EmulationStorageDeviceSettings DeviceSettings(AtariMachineConfiguration configuration,
        EmulationMediaSlot slot)
    {
        var device = AtariCompatibilityCatalog.Get(configuration.Model).Media
            .First(rule => rule.Availability == AtariMediaAvailability.Available && rule.Slots.Contains(slot));
        var floppy = device.Category == AtariMediaCategory.Floppy
            ? new FloppyDriveSettings(
                Option(configuration, ModelOptionPrefix, slot) ?? FloppyOptions(configuration.Model,
                    configuration.Folders.Floppies).Models[0].Value,
                Option(configuration, SpeedOptionPrefix, slot) ?? AtariStorageSettingsFunctionsConstants.Value100,
                bool.TryParse(Option(configuration, WriteProtectedOptionPrefix, slot), out var protectedValue)
                    && protectedValue,
                bool.TryParse(Option(configuration, RedirectWritesOptionPrefix, slot), out var redirectValue)
                    && redirectValue)
            : null;
        return new EmulationStorageDeviceSettings(slot, floppy,
            Option(configuration, InterfaceOptionPrefix, slot));
    }

    private static string? Option(AtariMachineConfiguration configuration, string prefix,
        EmulationMediaSlot slot) => configuration.Options.GetValueOrDefault(prefix + slot);

    private static bool IsDeviceOption(string key) => key.StartsWith(DeviceOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(ModelOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(SpeedOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(WriteProtectedOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(RedirectWritesOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(InterfaceOptionPrefix, StringComparison.Ordinal);

    private static string DisplayLabel(AtariMachineModel model, EmulationMediaSlot slot) => slot.Category switch
    {
        EmulationMediaCategory.FloppyDrive when model is AtariMachineModel.Atari400
            or AtariMachineModel.Atari800 or AtariMachineModel.Atari800Xl
            or AtariMachineModel.Atari130Xe or AtariMachineModel.Xegs or AtariMachineModel.XlXe => $"D{slot.Index + 1}:",
        EmulationMediaCategory.FloppyDrive => $"{(char)('A' + slot.Index)}:",
        EmulationMediaCategory.HardDisk => $"HD{slot.Index}:",
        EmulationMediaCategory.CompactDiscDrive => $"CD{slot.Index}:",
        EmulationMediaCategory.CartridgeSlot => $"CART{slot.Index}:",
        EmulationMediaCategory.CassetteDrive => $"TAPE{slot.Index}:",
        _ => slot.ToString()
    };
}
