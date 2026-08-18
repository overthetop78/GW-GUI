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
        var chip = EmulationOptionCatalog.ChipMemoryKib(Option(options, "puae_chipmem_size",
            EmulationOptionCatalog.ChipMemoryValue(model.ChipMemoryKib)));
        var slow = EmulationOptionCatalog.SlowMemoryKib(Option(options, "puae_bogomem_size",
            EmulationOptionCatalog.SlowMemoryValue(model.SlowMemoryKib)));
        var fast = EmulationOptionCatalog.MemoryMib(Option(options, "puae_fastmem_size",
            model.FastMemoryMib.ToString(CultureInfo.InvariantCulture)));
        var z3 = EmulationOptionCatalog.MemoryMib(Option(options, "puae_z3mem_size", "0"));
        var totalKib = chip + slow + (fast + z3) * 1024;
        var cpu = Option(options, "puae_cpu_model", model.DefaultCpu);
        var video = Option(options, "puae_video_standard", "PAL").StartsWith("NTSC",
            StringComparison.OrdinalIgnoreCase) ? "NTSC" : "PAL";
        var floppyCount = NumberOption(options, "gwgui_floppy_drive_count",
            configuration.Floppies?.Count ?? 1);
        var hardDriveCount = NumberOption(options, "gwgui_hard_drive_count",
            configuration.Media?.Count(item => item.Kind == AmigaMediaKind.HardDrive) ?? 0);
        var devices = new List<string>();
        if (floppyCount > 0) devices.Add($"DF {floppyCount}");
        if (hardDriveCount > 0) devices.Add($"HD {hardDriveCount}");
        if (model.HasCdDrive) devices.Add("CD");
        var details = new List<string>
        {
            $"Amiga {configuration.Model}", $"CPU {cpu}", $"{model.Chipset}/{video}",
            $"RAM {FormatMemory(totalKib * 1024L)}", AmigaFirmware(configuration.KickstartPath)
        };
        if (devices.Count > 0) details.Add(string.Join(" / ", devices));
        details.Add(ShortId(configuration.Id));
        return string.Join(" · ", details);
    }

    internal static string Atari(AtariMachineConfiguration configuration, string modelName)
    {
        var view = AtariHardwareSettingsFunctions.Create(configuration.Model, configuration.Options);
        var memory = AtariHardwareSettingsFunctions.TotalMemoryBytes(configuration.Options, view);
        var cpu = view.Cpu.First(field => field.Option == AtariSettingOption.CpuModel).SelectedValue;
        var firmware = AtariFirmwareSummary(configuration.Firmwares);
        var brandedModel = modelName.StartsWith("Atari ", StringComparison.OrdinalIgnoreCase)
            ? modelName
            : $"Atari {modelName}";
        return $"{brandedModel} · CPU {cpu} · RAM {FormatMemory(memory)} · {firmware} · "
            + $"Core {configuration.Core} · {ShortId(configuration.Id)}";
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

    private static int NumberOption(IReadOnlyDictionary<string, string> options, string key, int fallback) =>
        options.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : fallback;

    private static string ShortId(Guid id) => id.ToString("N")[..8];

    [GeneratedRegex(@"(?<!\d)(?:1\.[123]|2\.0[45]|3\.[01])(?:\.\d+)?(?!\d)", RegexOptions.IgnoreCase)]
    private static partial Regex FirmwareVersionPattern();
}
