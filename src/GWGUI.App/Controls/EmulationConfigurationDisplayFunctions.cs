using System.Globalization;
using System.IO;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class EmulationConfigurationDisplayFunctions
{
    internal const int MaximumRomNameLength = 15;

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
        return $"Amiga {configuration.Model} · RAM {FormatMemory(totalKib * 1024L)} · "
            + $"ROM {ShortFileName(configuration.KickstartPath)} · {ShortId(configuration.Id)}";
    }

    internal static string Atari(AtariMachineConfiguration configuration, string modelName)
    {
        var view = AtariHardwareSettingsFunctions.Create(configuration.Model, configuration.Options);
        var memory = AtariHardwareSettingsFunctions.TotalMemoryBytes(configuration.Options, view);
        var firmware = FirmwareSummary(configuration.Firmwares);
        var brandedModel = modelName.StartsWith("Atari ", StringComparison.OrdinalIgnoreCase)
            ? modelName
            : $"Atari {modelName}";
        return $"{brandedModel} · RAM {FormatMemory(memory)} · ROM {firmware} · {ShortId(configuration.Id)}";
    }

    internal static string ShortFileName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "—";
        var name = Path.GetFileName(path);
        if (name.Length <= MaximumRomNameLength) return name;
        var extension = Path.GetExtension(name);
        if (extension.Length is > 0 and < MaximumRomNameLength - 2)
        {
            var stemLength = MaximumRomNameLength - extension.Length - 1;
            return name[..stemLength] + "…" + extension;
        }
        return name[..(MaximumRomNameLength - 1)] + "…";
    }

    private static string FirmwareSummary(IReadOnlyList<AtariFirmwareConfiguration> firmwares)
    {
        if (firmwares.Count == 0) return "—";
        var first = ShortFileName(firmwares[0].Path);
        return firmwares.Count == 1 ? first : $"{first} +{firmwares.Count - 1}";
    }

    private static string FormatMemory(long bytes)
    {
        const long kib = 1024;
        const long mib = kib * 1024;
        if (bytes >= mib)
        {
            var value = bytes / (double)mib;
            return value.ToString(value % 1 == 0 ? "0" : "0.##", CultureInfo.CurrentCulture)
                + " " + StorageSizeFormatter.MebibyteUnit;
        }
        return (bytes / (double)kib).ToString("0.##", CultureInfo.CurrentCulture)
            + " " + StorageSizeFormatter.KibibyteUnit;
    }

    private static string Option(IReadOnlyDictionary<string, string> options, string key, string fallback) =>
        options.TryGetValue(key, out var value) ? value : fallback;

    private static string ShortId(Guid id) => id.ToString("N")[..8];
}
