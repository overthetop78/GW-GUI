using System.Text.RegularExpressions;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Functions;

internal static partial class AmigaConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(AmigaMachineConfiguration configuration)
    {
        var model = AmigaModelCatalog.Get(configuration.Model);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var details = new List<string>();

        if (TryOption(options, AmigaConfigurationSummaryFunctionsConstants.OptionCpuModel, out var cpu)) details.Add($"CPU {cpu}");
        var chipset = model.Chipset;
        if (TryOption(options, AmigaConfigurationSummaryFunctionsConstants.OptionVideoStandard, out var configuredVideo))
            chipset += configuredVideo.StartsWith(AmigaConfigurationSummaryFunctionsConstants.NTSC, StringComparison.OrdinalIgnoreCase) ? AmigaConfigurationSummaryFunctionsConstants.NTSC2 : AmigaConfigurationSummaryFunctionsConstants.PAL;
        details.Add(chipset);

        if (HasAnyOption(options, AmigaConfigurationSummaryFunctionsConstants.OptionChipmemSize, AmigaConfigurationSummaryFunctionsConstants.OptionBogomemSize, AmigaConfigurationSummaryFunctionsConstants.OptionFastmemSize, AmigaConfigurationSummaryFunctionsConstants.OptionZ3memSize))
        {
            var chip = ChipMemoryKib(Option(options, AmigaConfigurationSummaryFunctionsConstants.OptionChipmemSize, AmigaConfigurationSummaryFunctionsConstants.Value0));
            var slow = SlowMemoryKib(Option(options, AmigaConfigurationSummaryFunctionsConstants.OptionBogomemSize, AmigaConfigurationSummaryFunctionsConstants.Value0));
            var fast = MemoryMib(Option(options, AmigaConfigurationSummaryFunctionsConstants.OptionFastmemSize, AmigaConfigurationSummaryFunctionsConstants.Value0));
            var z3 = MemoryMib(Option(options, AmigaConfigurationSummaryFunctionsConstants.OptionZ3memSize, AmigaConfigurationSummaryFunctionsConstants.Value0));
            details.Add($"RAM {FormatMemory((chip + slow + (fast + z3) * 1024) * 1024L)}");
        }

        if (!string.IsNullOrWhiteSpace(configuration.KickstartPath))
            details.Add(Firmware(configuration.KickstartPath));

        var devices = new List<string>();
        var floppyCount = NumberOption(options, AmigaConfigurationSummaryFunctionsConstants.GwguiFloppyDriveCount);
        var hardDriveCount = NumberOption(options, AmigaConfigurationSummaryFunctionsConstants.GwguiHardDriveCount);
        if (floppyCount > 0) devices.Add($"DF {floppyCount}");
        if (hardDriveCount > 0) devices.Add($"HD {hardDriveCount}");
        if (Option(options, AmigaConfigurationSummaryFunctionsConstants.GwguiCdDriveEnabled, model.HasCdDrive ? AmigaConfigurationSummaryFunctionsConstants.Enabled : AmigaConfigurationSummaryFunctionsConstants.Disabled) == AmigaConfigurationSummaryFunctionsConstants.Enabled)
            devices.Add(AmigaConfigurationSummaryFunctionsConstants.CD);
        if (devices.Count > 0) details.Add(string.Join(AmigaConfigurationSummaryFunctionsConstants.Value, devices));

        details.Add(configuration.AudioEnabled ? AmigaConfigurationSummaryFunctionsConstants.AudioOn : AmigaConfigurationSummaryFunctionsConstants.AudioOff);
        var displayResourceKey = AmigaMachineCatalog.All.First(item => item.Id == configuration.Model)
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
        return name.Length <= 15 ? name : name[..14] + AmigaConfigurationSummaryFunctionsConstants.Value2;
    }

    private static int ChipMemoryKib(string value) => int.TryParse(value, out var parsed) ? parsed * 512 : 0;
    private static int SlowMemoryKib(string value) => value switch
    {
        AmigaConfigurationSummaryFunctionsConstants.Value22 => 512, AmigaConfigurationSummaryFunctionsConstants.Value4 => 1024, AmigaConfigurationSummaryFunctionsConstants.Value6 => 1536, AmigaConfigurationSummaryFunctionsConstants.Value7 => 1792, _ => 0
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

    [GeneratedRegex(AmigaConfigurationSummaryFunctionsConstants.D11232045301DD, RegexOptions.IgnoreCase)]
    private static partial Regex FirmwareVersionPattern();
}
