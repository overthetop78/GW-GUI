namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var contentExtensions = new Engine().Adapter(configuration).SupportedContentExtensions;
        var cartridgeFormats = model.Id switch
        {
            ModelConstants.MegaDrive or ModelConstants.MegaCd or ModelConstants.ThirtyTwoX or ModelConstants.Pico => StorageSettingsFunctionsConstants.MegaDriveCartridgeExtensions,
            ModelConstants.Naomi or ModelConstants.Naomi2 or ModelConstants.Atomiswave or ModelConstants.SystemSp or ModelConstants.StV or ModelConstants.Model3 => StorageSettingsFunctionsConstants.ArcadeCartridgeExtensions,
            _ => StorageSettingsFunctionsConstants.EightBitCartridgeExtensions
        };
        var options = configuration.Options ?? new Dictionary<string, string>();
        var devices = new List<EmulationMediaDevice>();
        for (var index = 0; index < model.MaximumFloppyDriveCount; index++)
            devices.Add(new EmulationMediaDevice(new EmulationMediaSlot(
                    EmulationMediaCategory.FloppyDrive, index), EmulationMediaType.Floppy,
                contentExtensions.Where(StorageSettingsFunctionsConstants.FloppyExtensions.Contains).ToArray(),
                DisplayLabel: index == 0 ? StorageSettingsFunctionsConstants.FloppyDriveLabel
                    : StorageSettingsFunctionsConstants.SecondFloppyDriveLabel,
                IsPermanent: index < model.BuiltInFloppyDriveCount));
        if (model.SupportsCassetteDrive)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cassette0, EmulationMediaType.Cassette,
                contentExtensions.Where(StorageSettingsFunctionsConstants.CassetteExtensions.Contains).ToArray(),
                RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CassetteDriveLabel,
                IsPermanent: model.HasBuiltInCassetteDrive));
        if (model.SupportsCartridgeSlot)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge,
                contentExtensions.Where(cartridgeFormats.Contains).ToArray(), RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CartridgeSlotLabel,
                IsPermanent: model.HasBuiltInCartridgeSlot));
        var megaCdEnabled = model.Id == ModelConstants.MegaDrive
            && configuration.MegaCdEnabled
            && configuration.MegaCdModel is ModelConstants.MegaCdI or ModelConstants.MegaCdII;
        var threeDGlassesEnabled = model.SupportsThreeDGlasses
            && OptionEnabled(options, SettingsConstants.MasterSystemThreeDGlasses);
        if (model.SupportsSegaCardSlot)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cartridge1, EmulationMediaType.Cartridge,
                [StorageSettingsFunctionsConstants.Mv], RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.SegaCardSlotLabel,
                IsPermanent: model.HasBuiltInSegaCardSlot,
                ConfigurationKind: model.SupportsThreeDGlasses
                    ? EmulationStorageConfigurationKind.CartridgeSlot
                    : EmulationStorageConfigurationKind.None,
                ConfigurationOptionResourceKey: model.SupportsThreeDGlasses
                    ? SettingsDescriptionFunctionsConstants.ResourceMasterSystemThreeDGlasses : null,
                ConfigurationOptionDetailedResourceKey: model.SupportsThreeDGlasses
                    ? SettingsDescriptionFunctionsConstants.ResourceMasterSystemThreeDGlassesHelp : null));
        IReadOnlyList<string> opticalExtensions = contentExtensions.Where(StorageSettingsFunctionsConstants.OpticalExtensions.Contains).ToArray();
        if (model.SupportsCompactDiscDrive || megaCdEnabled)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc,
                opticalExtensions, RequiresMachineRecreation: true,
                DisplayLabel: StorageSettingsFunctionsConstants.CompactDiscDriveLabel,
                IsPermanent: model.HasBuiltInCompactDiscDrive));
        var configuredFloppies = Math.Clamp(OptionInt(options,
            StorageSettingsFunctionsConstants.FloppyDriveCountOption,
            model.BuiltInFloppyDriveCount), model.BuiltInFloppyDriveCount,
            model.MaximumFloppyDriveCount);
        var configured = devices.Where(device => device.Slot.Category switch
        {
            EmulationMediaCategory.FloppyDrive => device.Slot.Index < configuredFloppies,
            EmulationMediaCategory.CassetteDrive => model.HasBuiltInCassetteDrive
                || OptionBool(options, StorageSettingsFunctionsConstants.CassetteDriveEnabledOption),
            EmulationMediaCategory.CartridgeSlot => device.Slot.Index == 0
                ? model.HasBuiltInCartridgeSlot
                    || OptionBool(options, StorageSettingsFunctionsConstants.CartridgeSlotEnabledOption)
                : model.HasBuiltInSegaCardSlot
                    || OptionBool(options, StorageSettingsFunctionsConstants.SegaCardSlotEnabledOption),
            EmulationMediaCategory.CompactDiscDrive => model.HasBuiltInCompactDiscDrive
                || megaCdEnabled
                || OptionBool(options, StorageSettingsFunctionsConstants.CompactDiscDriveEnabledOption),
            _ => false
        }).Select(device => device.Slot).ToArray();
        var mounted = EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
        IReadOnlyList<EmulationStorageDeviceSettings> deviceSettings = model.SupportsSegaCardSlot
            ? [new EmulationStorageDeviceSettings(EmulationMediaSlot.Cartridge1,
                Cartridge: new CartridgeSlotSettings(threeDGlassesEnabled))]
            : [];
        return new EmulationStorageSettings(devices, configured, mounted, deviceSettings);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        EmulationStorageSettings settings)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var threeDGlassesEnabled = model.SupportsThreeDGlasses && (settings.DeviceSettings ?? [])
            .FirstOrDefault(item => item.Slot == EmulationMediaSlot.Cartridge1)?.Cartridge
            ?.OptionEnabled == true;
        var media = settings.MountedMedia
            .Where(item => !threeDGlassesEnabled || item.Slot != EmulationMediaSlot.Cartridge1)
            .Select(item => new MediaConfiguration(
            item.Path, item.Type switch
            {
                EmulationMediaType.Floppy => MediaCategory.Floppy,
                EmulationMediaType.Cassette => MediaCategory.Cassette,
                EmulationMediaType.Cartridge => MediaCategory.Cartridge,
                EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
                _ => throw new ArgumentOutOfRangeException(nameof(settings), item.Type, null)
            }, item.Slot, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted,
            MountOrder: item.Slot.Index)).ToArray();
        var options = new Dictionary<string, string>(configuration.Options
            ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var item in media)
            if (!ConfigurationValidationFunctions.Supports(model, item.Category, item.Slot, options)
                || ConfigurationValidationFunctions.IsIncompatibleMedia(model, item, options)
                || item.Slot == EmulationMediaSlot.Cartridge1 && IsSegaCardLocked(model, options))
                throw new ArgumentOutOfRangeException(nameof(settings), item.Category, null);
        options = new Dictionary<string, string>(options, StringComparer.Ordinal)
        {
            [StorageSettingsFunctionsConstants.FloppyDriveCountOption] = settings.ConfiguredSlots
                .Count(slot => slot.Category == EmulationMediaCategory.FloppyDrive).ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
            [StorageSettingsFunctionsConstants.CassetteDriveEnabledOption] = settings.ConfiguredSlots
                .Contains(EmulationMediaSlot.Cassette0).ToString(),
            [StorageSettingsFunctionsConstants.CartridgeSlotEnabledOption] = settings.ConfiguredSlots
                .Contains(EmulationMediaSlot.Cartridge0).ToString(),
            [StorageSettingsFunctionsConstants.SegaCardSlotEnabledOption] = settings.ConfiguredSlots
                .Contains(EmulationMediaSlot.Cartridge1).ToString(),
            [SettingsConstants.MasterSystemThreeDGlasses] = threeDGlassesEnabled.ToString(),
            [StorageSettingsFunctionsConstants.CompactDiscDriveEnabledOption] = settings.ConfiguredSlots
                .Contains(EmulationMediaSlot.Cd0).ToString()
        };
        return configuration with { Media = media, Options = options };
    }

    private static int OptionInt(IReadOnlyDictionary<string, string> options,
        string key, int defaultValue) => options.TryGetValue(key, out var value)
        && int.TryParse(value, out var parsed) ? parsed : defaultValue;

    private static bool OptionBool(IReadOnlyDictionary<string, string> options,
        string key) => options.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
        && parsed;

    private static bool OptionEnabled(IReadOnlyDictionary<string, string> options, string key) =>
        OptionBool(options, key) || options.TryGetValue(key, out var value)
        && value.Equals(SettingsDescriptionFunctionsConstants.Enabled, StringComparison.OrdinalIgnoreCase);

    private static bool IsSegaCardLocked(Model model, IReadOnlyDictionary<string, string> options) =>
        model.SupportsThreeDGlasses && OptionEnabled(options, SettingsConstants.MasterSystemThreeDGlasses)
        || model.Id == ModelConstants.MasterSystem
        && options.GetValueOrDefault(SettingsConstants.MasterSystemVariant,
            ModelConstants.MasterSystemSmsI).Equals(ModelConstants.MasterSystemSmsIi,
                StringComparison.Ordinal);
}
