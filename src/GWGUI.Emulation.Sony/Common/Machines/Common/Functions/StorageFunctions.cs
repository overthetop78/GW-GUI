namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var adapter = EmulatorCatalog.CreateAdapters().FirstOrDefault(item => item.EmulatorId == configuration.EmulatorId);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var devices = new List<EmulationMediaDevice>();
        if (model.HasBuiltInMemoryCard)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.MemoryCard0, EmulationMediaType.MemoryCard,
                Machines.PocketStation.Constants.MediaConstants.FlashImageExtensions,
                RequiresMachineRecreation: true, IsPermanent: true));
        if (model.SupportsCompactDiscDrive && adapter is not null)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc,
                adapter.SupportedContentExtensions,
                RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CompactDiscDriveLabel,
                IsPermanent: model.HasBuiltInCompactDiscDrive));
        var configured = devices.Where(device => device.Slot.Category switch
        {
            EmulationMediaCategory.MemoryCard => model.HasBuiltInMemoryCard,
            EmulationMediaCategory.CompactDiscDrive => model.HasBuiltInCompactDiscDrive
                || OptionBool(options, StorageSettingsFunctionsConstants.CompactDiscDriveEnabledOption),
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
                EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
                EmulationMediaType.MemoryCard => MediaCategory.MemoryCard,
                _ => throw new ArgumentOutOfRangeException(nameof(settings), item.Type, null)
            }, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted, MountOrder: index)).ToArray();
        var model = ModelCatalog.Get(configuration.Model);
        foreach (var item in media)
            if (!ConfigurationValidationFunctions.Supports(model, item.Category))
                throw new ArgumentOutOfRangeException(nameof(settings), item.Category, null);
        var options = new Dictionary<string, string>(configuration.Options
            ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [StorageSettingsFunctionsConstants.CompactDiscDriveEnabledOption] = settings.ConfiguredSlots
                .Contains(EmulationMediaSlot.Cd0).ToString()
        };
        return configuration with { Media = media, Options = options };
    }

    private static bool OptionBool(IReadOnlyDictionary<string, string> options,
        string key) => options.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
        && parsed;
}
