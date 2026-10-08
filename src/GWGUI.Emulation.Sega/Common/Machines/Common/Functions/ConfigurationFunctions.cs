using System.IO;

namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var details = new List<string>
        {
            string.Format(System.Globalization.CultureInfo.InvariantCulture,
                model.RamBytes.HasValue ? ConfigurationSummaryFunctionsConstants.MemorySummaryBytesFormat
                    : ConfigurationSummaryFunctionsConstants.MemorySummaryKibFormat,
                model.RamBytes ?? model.RamKib)
        };
        details.AddRange((configuration.Media ?? []).OrderBy(media => media.MountOrder)
            .Select(media => Path.GetFileName(media.Path)));
        return new EmulationConfigurationSummary(
            MachineConfigurationConstants.ResourcePrefix + model.Id, details);
    }
}

internal static class ConfigurationValidationFunctions
{
    internal static void ValidateForSave(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        if (string.IsNullOrWhiteSpace(configuration.EmulatorId))
            throw new InvalidDataException(nameof(configuration.EmulatorId));
        if (configuration.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new InvalidDataException(nameof(configuration.SchemaVersion));
        foreach (var media in configuration.Media ?? [])
        {
            if (!File.Exists(media.Path)) throw new FileNotFoundException(null, media.Path);
            if (!Supports(model, media.Category, media.Slot, configuration.Options)
                || IsIncompatibleMedia(model, media, configuration.Options)
                || media.Slot == EmulationMediaSlot.Cartridge1
                && IsSegaCardLocked(model, configuration.Options))
                throw new InvalidDataException($"{model.Id}:{media.Category}");
        }
    }

    internal static bool Supports(Model model, MediaCategory category) =>
        Supports(model, category, default, null);

    internal static bool IsIncompatibleMedia(Model model, MediaConfiguration media,
        IReadOnlyDictionary<string, string>? options)
    {
        if (model.Id != ModelConstants.MegaDrive || media.Category != MediaCategory.Cartridge)
            return false;
        var isThirtyTwoX = Path.GetExtension(media.Path).Equals(
            StorageSettingsFunctionsConstants.ThirtyTwoX, StringComparison.OrdinalIgnoreCase);
        return isThirtyTwoX && !IsEnabled(options, SettingsConstants.MegaDriveThirtyTwoX);
    }

    private static bool IsMegaCdEnabled(Model model,
        IReadOnlyDictionary<string, string>? options) => model.Id == ModelConstants.MegaDrive
            && IsEnabled(options, SettingsConstants.MegaCdEnabled)
            && options?.GetValueOrDefault(SettingsConstants.MegaCdModel,
                ModelConstants.MegaCdI) is ModelConstants.MegaCdI or ModelConstants.MegaCdII;

    private static bool IsEnabled(IReadOnlyDictionary<string, string>? options, string key) =>
        options is not null && options.TryGetValue(key, out var value)
        && (bool.TryParse(value, out var parsed) && parsed
            || value.Equals(SettingsDescriptionFunctionsConstants.Enabled, StringComparison.OrdinalIgnoreCase));

    private static bool IsSegaCardLocked(Model model, IReadOnlyDictionary<string, string>? options) =>
        IsEnabled(options, SettingsConstants.MasterSystemThreeDGlasses)
        || model.Id == ModelConstants.MasterSystem
        && options?.GetValueOrDefault(SettingsConstants.MasterSystemVariant,
            ModelConstants.MasterSystemSmsI).Equals(ModelConstants.MasterSystemSmsIi,
                StringComparison.Ordinal) == true;

    internal static bool Supports(Model model, MediaCategory category, EmulationMediaSlot slot,
        IReadOnlyDictionary<string, string>? options = null) => category switch
    {
        MediaCategory.Floppy => model.MaximumFloppyDriveCount > 0,
        MediaCategory.Cassette => model.SupportsCassetteDrive,
        MediaCategory.Cartridge => slot.Category != EmulationMediaCategory.CartridgeSlot
            || slot.Index == 0
            ? model.SupportsCartridgeSlot
            : slot.Index == 1 && model.SupportsSegaCardSlot,
        MediaCategory.CompactDisc => model.SupportsCompactDiscDrive || IsMegaCdEnabled(model, options),
        MediaCategory.Snapshot => true,
        _ => false
    };
}
