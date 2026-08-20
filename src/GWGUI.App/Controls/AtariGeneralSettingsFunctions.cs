using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariGeneralSettingsFunctions
{
    internal static AtariMachineConfiguration ReplaceGeneral(
        AtariMachineConfiguration source,
        AtariMachineModel model,
        AtariFolderConfiguration folders,
        IReadOnlyList<AtariFirmwareConfiguration> firmwares,
        IReadOnlyDictionary<string, string> options) => new(
            model, firmwares, source.Media, options, source.Input, source.Id, source.SchemaVersion,
            source.AudioEnabled, source.VideoRenderer, folders);

    internal static AtariFolderConfiguration DefaultFolders() => new(
        StoragePaths.AtariSharedStorageDirectory,
        StoragePaths.AtariFloppyImagesDirectory,
        StoragePaths.AtariCassetteImagesDirectory,
        StoragePaths.AtariCartridgeImagesDirectory,
        StoragePaths.AtariCompactDiscsDirectory,
        StoragePaths.AtariHardDisksDirectory,
        StoragePaths.AtariStatesDirectory,
        StoragePaths.AtariCapturesDirectory);

    internal static AtariFolderConfiguration CompleteFolders(AtariFolderConfiguration folders)
    {
        var defaults = DefaultFolders();
        return new AtariFolderConfiguration(
            folders.Shared ?? defaults.Shared, folders.Floppies ?? defaults.Floppies,
            folders.Cassettes ?? defaults.Cassettes, folders.Cartridges ?? defaults.Cartridges,
            folders.CompactDiscs ?? defaults.CompactDiscs, folders.HardDisks ?? defaults.HardDisks,
            folders.States ?? defaults.States, folders.Captures ?? defaults.Captures);
    }

    internal static bool SupportsHardDiskFolder(AtariMachineModel model) =>
        AtariEightBitSettingsCatalog.SupportsComputerOptions(model)
        || AtariCompatibilityCatalog.Get(model).Media.Any(rule =>
            rule.Availability == AtariMediaAvailability.Available
            && rule.Kind is AtariMediaKind.HardDisk or AtariMediaKind.Directory);

    internal static string OptionHeading(AtariCoreOption option) => option.CategorizedName
        ?? (option.Category is null
            ? option.Name
            : option.Category + AtariGeneralSettingsConstants.OptionCategorySeparator + option.Name);

    internal static IReadOnlyDictionary<string, string> MergeOptions(
        IReadOnlyDictionary<string, string> existing,
        IEnumerable<KeyValuePair<string, string>> displayed)
    {
        var result = new Dictionary<string, string>(existing, StringComparer.Ordinal);
        foreach (var entry in displayed) result[entry.Key] = entry.Value;
        return result;
    }

    internal static AtariFirmwareConfiguration FirmwareConfiguration(AtariScannedFirmware firmware) =>
        new(firmware.Definition!.Kind!.Value, firmware.Path,
            firmware.Definition.Provision == AtariFirmwareProvision.RequiredExternal,
            firmware.Definition.Distribution == AtariFirmwareDistribution.UserSuppliedCopyrighted);
}
