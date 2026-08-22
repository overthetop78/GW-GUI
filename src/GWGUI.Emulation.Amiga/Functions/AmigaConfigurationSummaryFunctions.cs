using System.Text.RegularExpressions;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal static partial class AmigaConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(AmigaMachineConfiguration configuration)
    {
        var model = AmigaModelCatalog.Get(configuration.Model);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var details = new List<string>();

        if (TryOption(options, "puae_cpu_model", out var cpu)) details.Add($"CPU {cpu}");
        var chipset = model.Chipset;
        if (TryOption(options, "puae_video_standard", out var configuredVideo))
            chipset += configuredVideo.StartsWith("NTSC", StringComparison.OrdinalIgnoreCase) ? "/NTSC" : "/PAL";
        details.Add(chipset);

        if (HasAnyOption(options, "puae_chipmem_size", "puae_bogomem_size", "puae_fastmem_size", "puae_z3mem_size"))
        {
            var chip = ChipMemoryKib(Option(options, "puae_chipmem_size", "0"));
            var slow = SlowMemoryKib(Option(options, "puae_bogomem_size", "0"));
            var fast = MemoryMib(Option(options, "puae_fastmem_size", "0"));
            var z3 = MemoryMib(Option(options, "puae_z3mem_size", "0"));
            details.Add($"RAM {FormatMemory((chip + slow + (fast + z3) * 1024) * 1024L)}");
        }

        if (!string.IsNullOrWhiteSpace(configuration.KickstartPath))
            details.Add(Firmware(configuration.KickstartPath));

        var devices = new List<string>();
        var floppyCount = NumberOption(options, "gwgui_floppy_drive_count");
        var hardDriveCount = NumberOption(options, "gwgui_hard_drive_count");
        if (floppyCount > 0) devices.Add($"DF {floppyCount}");
        if (hardDriveCount > 0) devices.Add($"HD {hardDriveCount}");
        if (Option(options, "gwgui_cd_drive_enabled", "disabled") == "enabled") devices.Add("CD");
        if (devices.Count > 0) details.Add(string.Join(" / ", devices));

        details.Add($"Video {Renderer(configuration.VideoRenderer)}");
        details.Add(configuration.AudioEnabled ? "Audio On" : "Audio Off");
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
        return name.Length <= 15 ? name : name[..14] + "…";
    }

    private static int ChipMemoryKib(string value) => int.TryParse(value, out var parsed) ? parsed * 512 : 0;
    private static int SlowMemoryKib(string value) => value switch
    {
        "2" => 512, "4" => 1024, "6" => 1536, "7" => 1792, _ => 0
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
    private static string Renderer(EmulationVideoRenderer renderer) =>
        renderer == EmulationVideoRenderer.Direct3D11 ? "D3D11" : renderer.ToString();

    [GeneratedRegex(@"(?<!\d)(?:1\.[123]|2\.0[45]|3\.[01])(?:\.\d+)?(?!\d)", RegexOptions.IgnoreCase)]
    private static partial Regex FirmwareVersionPattern();
}
