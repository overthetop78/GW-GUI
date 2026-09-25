using GWGUI.Emulation;
using System.Text.RegularExpressions;
using System.Text;

namespace GWGUI.Emulation.Amiga.Common.Functions;

internal static partial class ConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var details = new List<string>();

        if (TryOption(options, ConfigurationSummaryFunctionsConstants.OptionCpuModel, out var cpu)) details.Add($"CPU {cpu}");
        var chipset = model.Chipset;
        if (TryOption(options, ConfigurationSummaryFunctionsConstants.OptionVideoStandard, out var configuredVideo))
            chipset += configuredVideo.StartsWith(ConfigurationSummaryFunctionsConstants.NTSC, StringComparison.OrdinalIgnoreCase) ? ConfigurationSummaryFunctionsConstants.NTSC2 : ConfigurationSummaryFunctionsConstants.PAL;
        details.Add(chipset);

        if (HasAnyOption(options, ConfigurationSummaryFunctionsConstants.OptionChipmemSize, ConfigurationSummaryFunctionsConstants.OptionBogomemSize, ConfigurationSummaryFunctionsConstants.OptionFastmemSize, ConfigurationSummaryFunctionsConstants.OptionZ3memSize))
        {
            var chip = ChipMemoryKib(Option(options, ConfigurationSummaryFunctionsConstants.OptionChipmemSize, ConfigurationSummaryFunctionsConstants.Value0));
            var slow = SlowMemoryKib(Option(options, ConfigurationSummaryFunctionsConstants.OptionBogomemSize, ConfigurationSummaryFunctionsConstants.Value0));
            var fast = MemoryMib(Option(options, ConfigurationSummaryFunctionsConstants.OptionFastmemSize, ConfigurationSummaryFunctionsConstants.Value0));
            var z3 = MemoryMib(Option(options, ConfigurationSummaryFunctionsConstants.OptionZ3memSize, ConfigurationSummaryFunctionsConstants.Value0));
            details.Add($"RAM {FormatMemory((chip + slow + (fast + z3) * 1024) * 1024L)}");
        }

        if (!string.IsNullOrWhiteSpace(configuration.KickstartPath))
            details.Add(Firmware(configuration.KickstartPath));

        var devices = new List<string>();
        var floppyCount = NumberOption(options, ConfigurationSummaryFunctionsConstants.GwguiFloppyDriveCount);
        var hardDriveCount = NumberOption(options, ConfigurationSummaryFunctionsConstants.GwguiHardDriveCount);
        if (floppyCount > 0) devices.Add($"DF {floppyCount}");
        if (hardDriveCount > 0) devices.Add($"HD {hardDriveCount}");
        if (Option(options, ConfigurationSummaryFunctionsConstants.GwguiCdDriveEnabled, model.HasCdDrive ? ConfigurationSummaryFunctionsConstants.Enabled : ConfigurationSummaryFunctionsConstants.Disabled) == ConfigurationSummaryFunctionsConstants.Enabled)
            devices.Add(ConfigurationSummaryFunctionsConstants.CD);
        if (devices.Count > 0) details.Add(string.Join(ConfigurationSummaryFunctionsConstants.Value, devices));

        details.Add(configuration.AudioEnabled ? ConfigurationSummaryFunctionsConstants.AudioOn : ConfigurationSummaryFunctionsConstants.AudioOff);
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
                var firmware = FirmwareCatalog.Inspect(path);
                if (!string.IsNullOrWhiteSpace(firmware.Version)) return $"Kickstart {firmware.Version}";
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
        }
        var version = FirmwareVersionPattern().Match(Path.GetFileNameWithoutExtension(path));
        return $"Kickstart {(version.Success ? version.Value : ShortName(path))}";
    }

    private static string ShortName(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        return name.Length <= 15 ? name : name[..14] + ConfigurationSummaryFunctionsConstants.Value2;
    }

    private static int ChipMemoryKib(string value) => int.TryParse(value, out var parsed) ? parsed * 512 : 0;
    private static int SlowMemoryKib(string value) => value switch
    {
        ConfigurationSummaryFunctionsConstants.Value22 => 512, ConfigurationSummaryFunctionsConstants.Value4 => 1024, ConfigurationSummaryFunctionsConstants.Value6 => 1536, ConfigurationSummaryFunctionsConstants.Value7 => 1792, _ => 0
    };
    private static int MemoryMib(string value) => int.TryParse(value, out var parsed) ? parsed : 0;
    private static int NumberOption(IReadOnlyDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : 0;
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
    private static string FormatMemory(long bytes) => bytes < 1024 * 1024
        ? $"{bytes / 1024d:0.#} KiB"
        : $"{bytes / 1024d / 1024d:0.##} MiB";

    [GeneratedRegex(ConfigurationSummaryFunctionsConstants.D11232045301DD, RegexOptions.IgnoreCase)]
    private static partial Regex FirmwareVersionPattern();
}

internal static class ConfigurationValidationFunctions
{
    private const string EncryptedKickstartHeader = ConfigurationValidationFunctionsConstants.AMIROMTYPE1;

    internal static void ValidateForSave(MachineConfiguration configuration)
    {
        ValidateFile(configuration.KickstartPath, true);
        ValidateFile(configuration.ExtendedRomPath, false);

        var requiresRomKey = IsEncryptedKickstart(configuration.KickstartPath);
        ValidateFile(configuration.RomKeyPath, requiresRomKey);

        ValidateFile(configuration.InitialDiskPath, false);
        foreach (var floppy in configuration.Floppies ?? []) ValidateFile(floppy.Path, true);
        foreach (var media in configuration.Media ?? []) ValidateFile(media.Path, true);
    }

    internal static bool IsEncryptedKickstart(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
        try
        {
            Span<byte> header = stackalloc byte[EncryptedKickstartHeader.Length];
            using var stream = File.OpenRead(path);
            return stream.Read(header) == header.Length
                && Encoding.ASCII.GetString(header) == EncryptedKickstartHeader;
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
