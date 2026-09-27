namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var devices = new List<EmulationMediaDevice>();
        for (var index = 0; index < model.MaximumFloppyDriveCount; index++)
            devices.Add(new EmulationMediaDevice(new EmulationMediaSlot(
                    EmulationMediaCategory.FloppyDrive, index), EmulationMediaType.Floppy,
                [StorageSettingsFunctionsConstants.Dsk, StorageSettingsFunctionsConstants.M3u],
                DisplayLabel: index == 0 ? StorageSettingsFunctionsConstants.FloppyDriveLabel
                    : StorageSettingsFunctionsConstants.SecondFloppyDriveLabel,
                IsPermanent: index < model.BuiltInFloppyDriveCount));
        if (model.SupportsCassetteDrive)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cassette0, EmulationMediaType.Cassette,
                [StorageSettingsFunctionsConstants.Cdt, StorageSettingsFunctionsConstants.Tap, StorageSettingsFunctionsConstants.Voc],
                RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CassetteDriveLabel,
                IsPermanent: model.HasBuiltInCassetteDrive));
        if (model.SupportsCartridgeSlot)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge,
                [StorageSettingsFunctionsConstants.Cpr], RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CartridgeSlotLabel,
                IsPermanent: model.HasBuiltInCartridgeSlot));
        var configuredFloppies = Math.Clamp(OptionInt(options,
            StorageSettingsFunctionsConstants.FloppyDriveCountOption,
            model.BuiltInFloppyDriveCount), model.BuiltInFloppyDriveCount,
            model.MaximumFloppyDriveCount);
        var configured = devices.Where(device => device.Slot.Category switch
        {
            EmulationMediaCategory.FloppyDrive => device.Slot.Index < configuredFloppies,
            EmulationMediaCategory.CassetteDrive => model.HasBuiltInCassetteDrive
                || OptionBool(options, StorageSettingsFunctionsConstants.CassetteDriveEnabledOption),
            EmulationMediaCategory.CartridgeSlot => model.HasBuiltInCartridgeSlot
                || OptionBool(options, StorageSettingsFunctionsConstants.CartridgeSlotEnabledOption),
            _ => false
        }).Select(device => device.Slot).ToArray();
        var mounted = EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
        return new EmulationStorageSettings(devices, configured, mounted);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        EmulationStorageSettings settings)
    {
        var media = settings.MountedMedia.Select((item, index) => new MediaConfiguration(
            item.Path, item.Type switch
            {
                EmulationMediaType.Floppy => MediaCategory.Floppy,
                EmulationMediaType.Cassette => MediaCategory.Cassette,
                EmulationMediaType.Cartridge => MediaCategory.Cartridge,
                _ => throw new ArgumentOutOfRangeException(nameof(settings), item.Type, null)
            }, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted, MountOrder: index)).ToArray();
        var model = ModelCatalog.Get(configuration.Model);
        foreach (var item in media)
            if (!ConfigurationValidationFunctions.Supports(model, item.Category))
                throw new ArgumentOutOfRangeException(nameof(settings), item.Category, null);
        var options = new Dictionary<string, string>(configuration.Options
            ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [StorageSettingsFunctionsConstants.FloppyDriveCountOption] = settings.ConfiguredSlots
                .Count(slot => slot.Category == EmulationMediaCategory.FloppyDrive).ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
            [StorageSettingsFunctionsConstants.CassetteDriveEnabledOption] = settings.ConfiguredSlots
                .Contains(EmulationMediaSlot.Cassette0).ToString(),
            [StorageSettingsFunctionsConstants.CartridgeSlotEnabledOption] = settings.ConfiguredSlots
                .Contains(EmulationMediaSlot.Cartridge0).ToString()
        };
        return configuration with { Media = media, Options = options };
    }

    private static int OptionInt(IReadOnlyDictionary<string, string> options,
        string key, int defaultValue) => options.TryGetValue(key, out var value)
        && int.TryParse(value, out var parsed) ? parsed : defaultValue;

    private static bool OptionBool(IReadOnlyDictionary<string, string> options,
        string key) => options.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
        && parsed;
}
