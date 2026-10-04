using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;

internal static class BeetlePcfxStorageFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices(Model model) =>
    [
        new(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc,
            [StorageSettingsFunctionsConstants.Cue, StorageSettingsFunctionsConstants.Ccd,
                BeetlePcfxStorageConstants.TocExtension, StorageSettingsFunctionsConstants.Chd],
            RequiresMachineRecreation: true,
            DisplayLabel: StorageSettingsFunctionsConstants.CdDriveLabel,
            IsPermanent: model.HasBuiltInCdDrive,
            ConfigurationKind: EmulationStorageConfigurationKind.CompactDiscDrive,
            CompactDiscCacheResourceKey: StorageSettingsFunctionsConstants.ResourceCdImageCache,
            ModelResourceKey: BeetlePcfxStorageConstants.CdRomResourceKey),
        new(EmulationMediaSlot.MemoryCard0, EmulationMediaType.MemoryCard,
            [PcFxBackupMemoryConstants.ExternalExtension],
            RequiresMachineRecreation: true,
            DisplayLabel: PcFxBackupMemoryConstants.ModelName,
            ConfigurationKind: EmulationStorageConfigurationKind.MemoryCard,
            ModelResourceKey: PcFxBackupMemoryConstants.ModelResourceKey,
            FixedImageSizeBytes: PcFxBackupMemoryConstants.BankSize,
            DefaultImageFileName: PcFxBackupMemoryConstants.ExternalFileName,
            DialogPresentation: new EmulationStorageDialogPresentation(
                PcFxBackupMemoryConstants.DeviceResourceKey,
                PcFxBackupMemoryConstants.ImageResourceKey,
                PcFxBackupMemoryConstants.UseExistingResourceKey,
                PcFxBackupMemoryConstants.CreateResourceKey,
                PcFxBackupMemoryConstants.AssociatedResourceKey,
                PcFxBackupMemoryConstants.FileNameResourceKey,
                PcFxBackupMemoryConstants.DestinationFolderResourceKey,
                PcFxBackupMemoryConstants.DeviceNameResourceKey,
                PcFxBackupMemoryConstants.DeviceIdentifierResourceKey,
                PcFxBackupMemoryConstants.ModelFieldResourceKey,
                PcFxBackupMemoryConstants.UseActionResourceKey,
                PcFxBackupMemoryConstants.ExistingHintResourceKey,
                PcFxBackupMemoryConstants.ImageRequiredResourceKey,
                PcFxBackupMemoryConstants.InvalidSizeResourceKey,
                PcFxBackupMemoryConstants.InvalidFormatResourceKey),
            HardDiskFormats: [new HardDiskImageFormat(
                PcFxBackupMemoryConstants.ModelName,
                PcFxBackupMemoryConstants.ExternalExtension,
                PcFxBackupMemoryConstants.ModelName,
                PcFxBackupMemoryConstants.BankSize,
                PcFxBackupMemoryConstants.BankSize)
            { MinimumBytes = PcFxBackupMemoryConstants.BankSize }])
    ];

    internal static string? ExternalCardPath(MachineConfiguration configuration)
    {
        var options = configuration.Options;
        return options is not null
            && options.TryGetValue(PcFxBackupMemoryConstants.EnabledOption,
                out var value) && bool.TryParse(value, out var enabled) && enabled
            ? options.GetValueOrDefault(PcFxBackupMemoryConstants.PathOption)
            : null;
    }

    internal static bool IsExternalCardEnabled(IReadOnlyDictionary<string, string> options) =>
        options.TryGetValue(PcFxBackupMemoryConstants.EnabledOption, out var value)
        && bool.TryParse(value, out var enabled) && enabled;

    internal static EmulationStorageDeviceSettings ExternalCardSettings(
        IReadOnlyDictionary<string, string> options) =>
        new(EmulationMediaSlot.MemoryCard0,
            MemoryCardPath: options.GetValueOrDefault(PcFxBackupMemoryConstants.PathOption));

    internal static EmulationStorageDeviceSettings CompactDiscSettings(
        IReadOnlyDictionary<string, string> options) =>
        new(EmulationMediaSlot.Cd0, CompactDisc: new EmulationCompactDiscDriveSettings(
            CacheImage: options.GetValueOrDefault(SettingsConstants.PcfxCdImageCache,
                StorageSettingsFunctionsConstants.CacheDisabled)
                == StorageSettingsFunctionsConstants.CacheEnabled));

    internal static void ApplyOptions(Dictionary<string, string> options,
        EmulationStorageSettings settings, IReadOnlyCollection<EmulationMediaSlot> configured)
    {
        options[PcFxBackupMemoryConstants.EnabledOption] =
            configured.Contains(EmulationMediaSlot.MemoryCard0).ToString();
        var cardPath = settings.DeviceSettings?.FirstOrDefault(item =>
            item.Slot == EmulationMediaSlot.MemoryCard0)?.MemoryCardPath;
        if (string.IsNullOrWhiteSpace(cardPath))
            options.Remove(PcFxBackupMemoryConstants.PathOption);
        else
        {
            PcFxBackupMemoryFunctions.ValidateExternalCard(cardPath);
            options[PcFxBackupMemoryConstants.PathOption] = cardPath;
        }
        var cd = settings.DeviceSettings?.FirstOrDefault(item =>
            item.Slot == EmulationMediaSlot.Cd0)?.CompactDisc;
        if (cd is not null)
            options[SettingsConstants.PcfxCdImageCache] = cd.CacheImage
                ? StorageSettingsFunctionsConstants.CacheEnabled
                : StorageSettingsFunctionsConstants.CacheDisabled;
    }
}
