using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static partial class StorageSettingsFunctions
{
private static IReadOnlyList<string> Extensions(MediaCategory category) => category switch
    {
        MediaCategory.Floppy => [StorageSettingsFunctionsConstants.St, StorageSettingsFunctionsConstants.Msa, StorageSettingsFunctionsConstants.Stx, StorageSettingsFunctionsConstants.Dim, StorageSettingsFunctionsConstants.Ipf, StorageSettingsFunctionsConstants.Scp, StorageSettingsFunctionsConstants.Atr, StorageSettingsFunctionsConstants.Xfd, StorageSettingsFunctionsConstants.Dcm, StorageSettingsFunctionsConstants.Atx],
        MediaCategory.HardDisk => [StorageSettingsFunctionsConstants.Img, StorageSettingsFunctionsConstants.Hdf, StorageSettingsFunctionsConstants.Vhd],
        MediaCategory.Cassette => [StorageSettingsFunctionsConstants.Cas],
        MediaCategory.CompactDisc => [StorageSettingsFunctionsConstants.Cue, StorageSettingsFunctionsConstants.Chd, StorageSettingsFunctionsConstants.Iso],
        _ => []
    };

    private static IReadOnlyList<string> CartridgeExtensions(MachineConfiguration configuration)
    {
        IReadOnlySet<string> extensions = configuration.Core switch
        {
            Emulator.Atari800 when configuration.Model == MachineModel.Atari5200 =>
                new HashSet<string>(["a52", "bin", "rom"], StringComparer.OrdinalIgnoreCase),
            Emulator.Atari800 =>
                new HashSet<string>(["car", "bin", "rom"], StringComparer.OrdinalIgnoreCase),
            _ when CartridgeConstants.Extensions.TryGetValue(configuration.Core, out var supported) => supported,
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };
        return extensions.Order(StringComparer.OrdinalIgnoreCase)
            .Select(extension => $"{CommonConstants.ExtensionPrefix}{extension}").ToArray();
    }

    private static FloppyDriveDialogOptions FloppyOptions(MachineModel model, string? imageDirectory)
    {
        var models = StorageConfigurationFunctions.Family(model) == MachineFamily.St
            ? StFloppyModels(model)
            : EightBitFloppyModels();
        return new FloppyDriveDialogOptions(models, imageDirectory ?? string.Empty,
            string.Join(';', Extensions(MediaCategory.Floppy).Select(extension => $"*{extension}")),
            StorageConfigurationFunctions.Family(model) == MachineFamily.St ? StorageSettingsFunctionsConstants.St : StorageSettingsFunctionsConstants.Atr);
    }

    private static IReadOnlyList<FloppyDriveModelChoice> StFloppyModels(MachineModel model)
    {
        var models = new List<FloppyDriveModelChoice>
        {
            new(StorageSettingsFunctionsConstants.Atarist720, StorageSettingsFunctionsConstants.FormatAtarist720, BlankImageSize: 737_280)
        };
        if (StModelCatalog.Get(model).Storage.Contains(StStorageCapability.FloppyHighDensity))
            models.Add(new FloppyDriveModelChoice(StorageSettingsFunctionsConstants.Atarist1440, StorageSettingsFunctionsConstants.FormatAtarist1440,
                BlankImageSize: 1_474_560));
        return models;
    }

    private static IReadOnlyList<FloppyDriveModelChoice> EightBitFloppyModels() =>
    [
        new(StorageSettingsFunctionsConstants.Atari90, StorageSettingsFunctionsConstants.FormatAtari90, BlankImageSize: 92_160),
        new(StorageSettingsFunctionsConstants.Atari130, StorageSettingsFunctionsConstants.FormatAtari130, BlankImageSize: 133_120),
        new(StorageSettingsFunctionsConstants.Atari180, StorageSettingsFunctionsConstants.FormatAtari180, BlankImageSize: 184_320)
    ];

    private static IReadOnlyList<EmulationSettingsChoice> Interfaces(MachineModel model,
        MediaCategory category)
    {
        if (category != MediaCategory.HardDisk
            || StorageConfigurationFunctions.Family(model) != MachineFamily.St) return [];
        var storage = StModelCatalog.Get(model).Storage;
        var choices = new List<EmulationSettingsChoice>();
        if (storage.Contains(StStorageCapability.Acsi)) choices.Add(InvariantChoice(StorageSettingsFunctionsConstants.Acsi, StorageSettingsFunctionsConstants.ACSI));
        if (storage.Contains(StStorageCapability.Ide)) choices.Add(InvariantChoice(StorageSettingsFunctionsConstants.Ide, StorageSettingsFunctionsConstants.IDE));
        return choices;
    }

    private static EmulationSettingsChoice InvariantChoice(string id, string value) => new(id, string.Empty, value);

    private static EmulationStorageDeviceSettings DeviceSettings(MachineConfiguration configuration,
        EmulationMediaSlot slot)
    {
        var device = CompatibilityCatalog.Get(configuration.Model).Media
            .First(rule => rule.Availability == MediaAvailability.Available && rule.Slots.Contains(slot));
        var floppy = device.Category == MediaCategory.Floppy
            ? new FloppyDriveSettings(
                Option(configuration, ModelOptionPrefix, slot) ?? FloppyOptions(configuration.Model,
                    configuration.Folders.Floppies).Models[0].Value,
                Option(configuration, SpeedOptionPrefix, slot) ?? StorageSettingsFunctionsConstants.Value100,
                bool.TryParse(Option(configuration, WriteProtectedOptionPrefix, slot), out var protectedValue)
                    && protectedValue,
                bool.TryParse(Option(configuration, RedirectWritesOptionPrefix, slot), out var redirectValue)
                    && redirectValue)
            : null;
        return new EmulationStorageDeviceSettings(slot, floppy,
            Option(configuration, InterfaceOptionPrefix, slot));
    }

    private static string? Option(MachineConfiguration configuration, string prefix,
        EmulationMediaSlot slot) => configuration.Options.GetValueOrDefault(prefix + slot);

    private static bool IsDeviceOption(string key) => key.StartsWith(DeviceOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(ModelOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(SpeedOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(WriteProtectedOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(RedirectWritesOptionPrefix, StringComparison.Ordinal)
        || key.StartsWith(InterfaceOptionPrefix, StringComparison.Ordinal);

    private static string DisplayLabel(MachineModel model, EmulationMediaSlot slot) => slot.Category switch
    {
        EmulationMediaCategory.FloppyDrive when model is MachineModel.Atari400
            or MachineModel.Atari800 or MachineModel.Atari800Xl
            or MachineModel.Atari130Xe or MachineModel.Xegs or MachineModel.XlXe => $"D{slot.Index + 1}:",
        EmulationMediaCategory.FloppyDrive => $"{(char)('A' + slot.Index)}:",
        EmulationMediaCategory.HardDisk => $"HD{slot.Index}:",
        EmulationMediaCategory.CompactDiscDrive => $"CD{slot.Index}:",
        EmulationMediaCategory.CartridgeSlot => $"CART{slot.Index}:",
        EmulationMediaCategory.CassetteDrive => $"TAPE{slot.Index}:",
        _ => slot.ToString()
    };
}
