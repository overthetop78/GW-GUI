using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static partial class EmulationConfigurationDisplayFunctions
{
    internal const int MaximumFallbackNameLength = 15;

    internal static string Amiga(AmigaMachineConfiguration configuration)
    {
        var model = AmigaModelCatalog.Get(configuration.Model);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var details = new List<string> { $"Amiga {configuration.Model}" };
        if (TryOption(options, "puae_cpu_model", out var cpu)) details.Add($"CPU {cpu}");
        var chipset = model.Chipset;
        if (TryOption(options, "puae_video_standard", out var configuredVideo))
        {
            var video = configuredVideo.StartsWith("NTSC", StringComparison.OrdinalIgnoreCase) ? "NTSC" : "PAL";
            chipset += $"/{video}";
        }
        details.Add(chipset);
        if (HasAnyOption(options, "puae_chipmem_size", "puae_bogomem_size", "puae_fastmem_size", "puae_z3mem_size"))
        {
            var chip = EmulationOptionCatalog.ChipMemoryKib(Option(options, "puae_chipmem_size", "0"));
            var slow = EmulationOptionCatalog.SlowMemoryKib(Option(options, "puae_bogomem_size", "0"));
            var fast = EmulationOptionCatalog.MemoryMib(Option(options, "puae_fastmem_size", "0"));
            var z3 = EmulationOptionCatalog.MemoryMib(Option(options, "puae_z3mem_size", "0"));
            details.Add($"RAM {FormatMemory((chip + slow + (fast + z3) * 1024) * 1024L)}");
        }
        if (!string.IsNullOrWhiteSpace(configuration.KickstartPath))
            details.Add(AmigaFirmware(configuration.KickstartPath));
        var floppyCount = NumberOption(options, "gwgui_floppy_drive_count", 0);
        var hardDriveCount = NumberOption(options, "gwgui_hard_drive_count", 0);
        var cdDrive = Option(options, "gwgui_cd_drive_enabled", "disabled")
            .Equals("enabled", StringComparison.OrdinalIgnoreCase);
        var devices = new List<string>();
        if (floppyCount > 0) devices.Add($"DF {floppyCount}");
        if (hardDriveCount > 0) devices.Add($"HD {hardDriveCount}");
        if (cdDrive) devices.Add("CD");
        if (devices.Count > 0) details.Add(string.Join(" / ", devices));
        details.Add($"Video {Renderer(configuration.VideoRenderer)}");
        details.Add(configuration.AudioEnabled ? "Audio On" : "Audio Off");
        details.Add(ShortId(configuration.Id));
        return string.Join(" · ", details);
    }

    internal static string Atari(AtariMachineConfiguration configuration, string modelName)
    {
        var brandedModel = modelName.StartsWith("Atari ", StringComparison.OrdinalIgnoreCase)
            ? modelName
            : $"Atari {modelName}";
        var details = new List<string> { brandedModel };
        if (TryOption(configuration.Options, AtariHardwareSettingsConstants.CpuOptionKey, out var cpu))
            details.Add($"CPU {cpu}");
        if (HasAnyOption(configuration.Options, AtariHardwareSettingsConstants.MainMemoryOptionKey,
                AtariHardwareSettingsConstants.AlternateMemoryOptionKey))
        {
            var main = LongOption(configuration.Options, AtariHardwareSettingsConstants.MainMemoryOptionKey);
            var alternate = LongOption(configuration.Options, AtariHardwareSettingsConstants.AlternateMemoryOptionKey);
            details.Add($"RAM {FormatMemory(main + alternate)}");
        }
        if (configuration.Firmwares.Count > 0) details.Add(AtariFirmwareSummary(configuration.Firmwares));
        details.Add($"Core {configuration.Core}");
        details.Add($"Video {Renderer(configuration.VideoRenderer)}");
        details.Add(configuration.AudioEnabled ? "Audio On" : "Audio Off");
        details.Add(ShortId(configuration.Id));
        return string.Join(" · ", details);
    }

    internal static string ShortFallbackName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "—";
        var name = Path.GetFileNameWithoutExtension(path);
        return name.Length <= MaximumFallbackNameLength
            ? name
            : name[..(MaximumFallbackNameLength - 1)] + "…";
    }

    private static string AmigaFirmware(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "Kickstart —";
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
        return version.Success ? $"Kickstart {version.Value}" : $"Kickstart {ShortFallbackName(path)}";
    }

    private static string AtariFirmwareSummary(IReadOnlyList<AtariFirmwareConfiguration> firmwares)
    {
        if (firmwares.Count == 0) return "Firmware —";
        return string.Join(" + ", firmwares.Select(AtariFirmware));
    }

    private static string AtariFirmware(AtariFirmwareConfiguration firmware)
    {
        var role = firmware.Kind == AtariFirmwareKind.Tos
            ? "TOS"
            : AtariHardwareSettingsFunctions.FirmwareKindName(firmware.Kind);
        var version = AtariFirmwareVersion(firmware);
        return version is null ? $"{role} {ShortFallbackName(firmware.Path)}" : $"{role} {version}";
    }

    private static string? AtariFirmwareVersion(AtariFirmwareConfiguration firmware)
    {
        if (!File.Exists(firmware.Path)) return null;
        try
        {
            if (firmware.Kind == AtariFirmwareKind.Tos)
            {
                using var stream = File.OpenRead(firmware.Path);
                Span<byte> header = stackalloc byte[4];
                if (stream.Read(header) == header.Length)
                {
                    var encoded = (header[2] << 8) | header[3];
                    var major = encoded >> 8;
                    var minor = encoded & 0xff;
                    if (major is >= 1 and <= 4 && minor <= 99) return $"{major}.{minor:D2}";
                }
            }
            using var file = File.OpenRead(firmware.Path);
            var md5 = Convert.ToHexStringLower(MD5.HashData(file));
            return AtariFirmwareScanFunctions.Identify(md5)?.Version;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { return null; }
    }

    private static string FormatMemory(long bytes) => StorageSizeFormatter.FormatBytes(bytes);

    private static string Option(IReadOnlyDictionary<string, string> options, string key, string fallback) =>
        options.TryGetValue(key, out var value) ? value : fallback;

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

    private static bool HasAnyOption(IReadOnlyDictionary<string, string> options, params string[] keys) =>
        keys.Any(options.ContainsKey);

    private static long LongOption(IReadOnlyDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var value) && long.TryParse(value, NumberStyles.Integer,
            CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

    private static int NumberOption(IReadOnlyDictionary<string, string> options, string key, int fallback) =>
        options.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : fallback;

    private static string ShortId(Guid id) => id.ToString("N")[..8];

    private static string Renderer(GWGUI.Emulation.EmulationVideoRenderer renderer) => renderer switch
    {
        GWGUI.Emulation.EmulationVideoRenderer.Direct3D11 => "D3D11",
        _ => renderer.ToString()
    };

    [GeneratedRegex(@"(?<!\d)(?:1\.[123]|2\.0[45]|3\.[01])(?:\.\d+)?(?!\d)", RegexOptions.IgnoreCase)]
    private static partial Regex FirmwareVersionPattern();
}
