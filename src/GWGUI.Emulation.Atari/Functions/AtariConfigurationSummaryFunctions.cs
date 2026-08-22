using System.Globalization;
using System.Security.Cryptography;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariConfigurationSummaryFunctions
{
    internal static EmulationConfigurationSummary Create(AtariMachineConfiguration configuration)
    {
        var model = AtariModelCatalog.All.First(item => item.Id == configuration.MachineId);
        var details = new List<string>();
        if (configuration.Options.TryGetValue(AtariSettingsConstants.Cpu, out var cpu)
            && !string.IsNullOrWhiteSpace(cpu)) details.Add($"CPU {cpu}");

        if (configuration.Options.ContainsKey(AtariConfigurationOptionConstants.MainMemory)
            || configuration.Options.ContainsKey(AtariSettingsConstants.AlternateMemory))
        {
            var main = LongOption(configuration.Options, AtariConfigurationOptionConstants.MainMemory);
            var alternate = LongOption(configuration.Options, AtariSettingsConstants.AlternateMemory);
            details.Add($"RAM {FormatMemory(main + alternate)}");
        }

        if (configuration.Firmwares.Count > 0)
            details.Add(string.Join(" + ", configuration.Firmwares.Select(Firmware)));
        details.Add($"Core {configuration.Core}");
        details.Add($"Video {(configuration.VideoRenderer == EmulationVideoRenderer.Direct3D11 ? "D3D11" : configuration.VideoRenderer)}");
        details.Add(configuration.AudioEnabled ? "Audio On" : "Audio Off");
        return new EmulationConfigurationSummary(model.DisplayResourceKey, details);
    }

    private static string Firmware(AtariFirmwareConfiguration firmware)
    {
        var role = firmware.Category == AtariFirmwareCategory.Tos ? "TOS" : firmware.Category.ToString();
        var version = FirmwareVersion(firmware);
        return $"{role} {(version ?? ShortName(firmware.Path))}";
    }

    private static string? FirmwareVersion(AtariFirmwareConfiguration firmware)
    {
        if (!File.Exists(firmware.Path)) return null;
        try
        {
            if (firmware.Category == AtariFirmwareCategory.Tos)
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

    private static string ShortName(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        return name.Length <= 15 ? name : name[..14] + "…";
    }
    private static long LongOption(IReadOnlyDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var value) && long.TryParse(value, NumberStyles.Integer,
            CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
    private static string FormatMemory(long bytes) => bytes < 1024 * 1024
        ? $"{bytes / 1024d:0.#} KiB"
        : $"{bytes / 1024d / 1024d:0.##} MiB";
}
