using System.IO;
using GWGUI.Emulation;
using System.Text.RegularExpressions;
using System.Text;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static partial class AmigaConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var model = (AmigaModel)ModelCatalog.Get(configuration.Model);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var details = new List<string>();

        if (TryOption(options, SettingsConstants.OptionCpuModel, out var cpu)) details.Add(string.Concat(ConfigurationSummaryFunctionsConstants.CpuLabelPrefix, cpu));
        var chipset = model.Chipset;
        if (TryOption(options, SettingsConstants.OptionVideoStandard, out var configuredVideo))
        {
            var videoStandard = configuredVideo.StartsWith(SettingsValueConstants.NTSC,
                StringComparison.OrdinalIgnoreCase)
                ? SettingsValueConstants.NTSC
                : SettingsValueConstants.PAL;
            chipset = string.Concat(chipset, MachineConfigurationConstants.ChipsetVideoStandardSeparator,
                videoStandard);
        }
        details.Add(chipset);

        if (HasAnyOption(options, SettingsConstants.OptionChipmemSize, SettingsConstants.OptionBogomemSize, SettingsConstants.OptionFastmemSize, SettingsConstants.OptionZ3memSize))
        {
            var chip = ChipMemoryKib(Option(options, SettingsConstants.OptionChipmemSize, SettingsValueConstants.DisabledMemory));
            var slow = SlowMemoryKib(Option(options, SettingsConstants.OptionBogomemSize, SettingsValueConstants.DisabledMemory));
            var fast = MemoryMib(Option(options, SettingsConstants.OptionFastmemSize, SettingsValueConstants.DisabledMemory));
            var z3 = MemoryMib(Option(options, SettingsConstants.OptionZ3memSize, SettingsValueConstants.DisabledMemory));
            details.Add(string.Concat(ConfigurationSummaryFunctionsConstants.RamLabelPrefix, FormatMemory((chip + slow + (fast + z3) * MemoryConstants.KibPerMib) * (long)MemoryConstants.BytesPerKib)));
        }

        if (!string.IsNullOrWhiteSpace(configuration.FirmwarePath(SettingsConstants.KickstartPath)))
            details.Add(Firmware(configuration.FirmwarePath(SettingsConstants.KickstartPath)));

        var devices = new List<string>();
        var floppyCount = NumberOption(options, StorageSettingsFunctionsConstants.GwguiFloppyDriveCount);
        var hardDriveCount = NumberOption(options, StorageSettingsFunctionsConstants.GwguiHardDriveCount);
        if (floppyCount > (int)DriveCapacity.None) devices.Add(string.Concat(ConfigurationSummaryFunctionsConstants.FloppyLabelPrefix, floppyCount));
        if (hardDriveCount > (int)DriveCapacity.None) devices.Add(string.Concat(ConfigurationSummaryFunctionsConstants.HardDriveLabelPrefix, hardDriveCount));
        if (Option(options, StorageSettingsFunctionsConstants.GwguiCdDriveEnabled,
                model.HasCdDrive ? StorageSettingsFunctionsConstants.Enabled : StorageSettingsFunctionsConstants.Disabled)
            == StorageSettingsFunctionsConstants.Enabled)
            devices.Add(StorageSettingsFunctionsConstants.CompactDiscSummaryLabel);
        if (devices.Count > BufferConstants.EmptyCollectionCount)
            details.Add(string.Join(MachineConfigurationConstants.DeviceSeparator, devices));

        var displayResourceKey = MachineCatalog.All.First(item => item.Id == configuration.Model)
            .DisplayResourceKey;
        return new EmulationConfigurationSummary(displayResourceKey, details);
    }

    private static string Firmware(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                var firmware = AmigaFirmwareCatalog.Inspect(path);
                if (!string.IsNullOrWhiteSpace(firmware.Version)) return string.Concat(ConfigurationSummaryFunctionsConstants.KickstartLabelPrefix, firmware.Version);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
        }
        var version = FirmwareVersionPattern().Match(Path.GetFileNameWithoutExtension(path));
        return string.Concat(ConfigurationSummaryFunctionsConstants.KickstartLabelPrefix, version.Success ? version.Value : ShortName(path));
    }

    private static string ShortName(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        return name.Length <= ConfigurationSummaryFunctionsConstants.MaximumShortNameLength
            ? name
            : name[..ConfigurationSummaryFunctionsConstants.TruncatedShortNameLength]
              + ConfigurationSummaryFunctionsConstants.Ellipsis;
    }

    private static int ChipMemoryKib(string value) => int.TryParse(value, out var parsed) ? parsed * (int)RamCapacity._512KB : (int)RamCapacity.None;
    private static int SlowMemoryKib(string value) => value switch
    {
        _ when int.TryParse(value, out var units) => units * (int)RamCapacity._256KB,
        _ => (int)RamCapacity.None
    };
    private static int MemoryMib(string value) => int.TryParse(value, out var parsed) ? parsed : (int)RamCapacity.None;
    private static int NumberOption(IReadOnlyDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : (int)DriveCapacity.None;
    private static string Option(IReadOnlyDictionary<string, string> options, string key, string fallback) =>
        options.GetValueOrDefault(key) ?? fallback;
    private static bool HasAnyOption(IReadOnlyDictionary<string, string> options, params string[] keys) =>
        keys.Any(options.ContainsKey);
    private static bool TryOption(IReadOnlyDictionary<string, string> options, string key, out string value)
    {
        if (options.TryGetValue(key, out var configured) && !string.IsNullOrWhiteSpace(configured))
        {
            value = configured;
            return true;
        }
        value = string.Empty;
        return false;
    }
    private static string FormatMemory(long bytes) => bytes < MemoryConstants.BytesPerMib
        ? string.Format(MemoryConstants.FractionalKibLabelFormat, bytes / (double)MemoryConstants.BytesPerKib)
        : string.Format(MemoryConstants.FractionalMibLabelFormat, bytes / (double)MemoryConstants.BytesPerMib);

    [GeneratedRegex(FirmwareCatalogConstants.SummaryFirmwareVersionPattern, RegexOptions.IgnoreCase)]
    private static partial Regex FirmwareVersionPattern();
}

internal static class AmigaConfigurationValidationFunctions
{
    internal static void ValidateForSave(MachineConfiguration configuration)
    {
        ValidateFile(configuration.FirmwarePath(SettingsConstants.KickstartPath), false);
        ValidateFile(configuration.FirmwarePath(SettingsConstants.ExtendedRomPath), false);

        var requiresRomKey = IsEncryptedKickstart(configuration.FirmwarePath(SettingsConstants.KickstartPath));
        ValidateFile(configuration.FirmwarePath(SettingsConstants.RomKeyPath), requiresRomKey);

        ValidateFile(configuration.InitialDiskPath, false);
        foreach (var floppy in configuration.Floppies ?? []) ValidateFile(floppy.Path, true);
        foreach (var media in configuration.Media ?? []) ValidateFile(media.Path, true);
    }

    internal static bool IsEncryptedKickstart(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
        try
        {
            Span<byte> header = stackalloc byte[FirmwareCatalogConstants.EncryptedKickstartHeader.Length];
            using var stream = File.OpenRead(path);
            return stream.Read(header) == header.Length
                && Encoding.ASCII.GetString(header) == FirmwareCatalogConstants.EncryptedKickstartHeader;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void ValidateFile(string? path, bool required)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            if (required) throw new FileNotFoundException(null, path);
            return;
        }

        if (!File.Exists(path)) throw new FileNotFoundException(null, path);
    }
}
