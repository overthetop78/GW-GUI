using System.Globalization;
using GWGUI.App.Localization;
using GWGUI.Domain.Settings;
using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Controls;

internal static class EmulationOptionCatalog
{
    internal static OptionChoice[] Choices(params (string Value, string TextOrKey)[] choices) => choices
        .Select(choice => new OptionChoice(choice.Value,
            choice.TextOrKey.Contains('.') ? LocExtension.Get(choice.TextOrKey) : choice.TextOrKey))
        .ToArray();

    internal static OptionChoice[] InitialChipMemory()
    {
        var mib = StorageSizeFormatter.MebibyteUnit;
        var kib = StorageSizeFormatter.KibibyteUnit;
        return
        [
            new("auto", LocExtension.Get("Visual.Automatic")), new("1", $"512 {kib}"),
            new("2", $"1 {mib}"), new("3", $"{1.5.ToString("0.0", CultureInfo.CurrentCulture)} {mib}"),
            new("4", $"2 {mib}")
        ];
    }

    internal static OptionChoice[] InitialSlowMemory()
    {
        var mib = StorageSizeFormatter.MebibyteUnit;
        var kib = StorageSizeFormatter.KibibyteUnit;
        return
        [
            new("auto", LocExtension.Get("Visual.Automatic")), new("0", LocExtension.Get("Emulation.MemoryNone")),
            new("2", $"512 {kib}"), new("4", $"1 {mib}"),
            new("6", $"{1.5.ToString("0.0", CultureInfo.CurrentCulture)} {mib}"),
            new("7", $"{1.8.ToString("0.0", CultureInfo.CurrentCulture)} {mib}")
        ];
    }

    internal static OptionChoice[] MemoryChoices(IEnumerable<int> values, bool includeAutomatic = true)
    {
        var choices = values.Select(value => new OptionChoice(value.ToString(),
            value == 0 ? LocExtension.Get("Emulation.MemoryNone") : $"{value} {StorageSizeFormatter.MebibyteUnit}"));
        return includeAutomatic
            ? [new("auto", LocExtension.Get("Visual.Automatic")), .. choices]
            : choices.ToArray();
    }

    internal static OptionChoice[] VideoStandards() =>
    [
        new("PAL auto", $"PAL ({LocExtension.Get("Visual.Automatic")})"),
        new("NTSC auto", $"NTSC ({LocExtension.Get("Visual.Automatic")})"), new("PAL", "PAL"), new("NTSC", "NTSC")
    ];

    internal static OptionChoice[] CpuCompatibility() =>
    [
        new("normal", $"{LocExtension.Get("Emulation.CompatibilityNormal")} (CPU)"),
        new("compatible", $"{LocExtension.Get("Emulation.CompatibilityCompatible")} (CPU)"),
        new("memory", $"{LocExtension.Get("Emulation.CompatibilityMemory")} (DMA / RAM)"),
        new("exact", $"{LocExtension.Get("Emulation.CompatibilityExact")} (CPU / DMA / RAM)")
    ];

    internal static RendererChoice[] VideoRenderers() =>
    [
        new(EmulationVideoRenderer.Direct3D11, "Direct3D 11"), new(EmulationVideoRenderer.Vulkan, "Vulkan"),
        new(EmulationVideoRenderer.OpenGL, "OpenGL"), new(EmulationVideoRenderer.Wpf, "WPF")
    ];

    internal static IReadOnlyList<int> ChipMemoryValues(AmigaModel model) => model.Id switch
    {
        "A1000" => [512], "A500" => [512, 1024, 1536, 2048],
        "A500PLUS" or "A600" => [1024, 2048], "A2000" => [512, 1024, 2048], _ => [2048]
    };

    internal static IReadOnlyList<int> SlowMemoryValues(AmigaModel model) => model.Id switch
    {
        "A1000" or "A500" or "A500PLUS" or "A2000" => [0, 512, 1024, 1536, 1792], _ => [0]
    };

    internal static IReadOnlyList<int> FastMemoryValues() => [0, 1, 2, 4, 8];
    internal static IReadOnlyList<int> Z3MemoryValues(AmigaModel model) => model.Id is "A3000" or "A4000"
        ? [0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512] : [0];

    internal static OptionChoice ChipMemoryChoice(int kib) => new(ChipMemoryValue(kib), StorageSizeFormatter.FormatKibibytes(kib));
    internal static OptionChoice SlowMemoryChoice(int kib) => new(SlowMemoryValue(kib),
        kib == 0 ? LocExtension.Get("Emulation.MemoryNone") : StorageSizeFormatter.FormatKibibytes(kib));
    internal static string ChipMemoryValue(int kib) => Math.Clamp(kib / 512, 1, 4).ToString();
    internal static string SlowMemoryValue(int kib) => kib switch { 0 => "0", 512 => "2", 1024 => "4", 1536 => "6", 1792 => "7", _ => "0" };
    internal static int ChipMemoryKib(string value) => int.TryParse(value, out var units) ? units * 512 : 0;
    internal static int SlowMemoryKib(string value) => value switch { "2" => 512, "4" => 1024, "6" => 1536, "7" => 1792, _ => 0 };
    internal static int MemoryMib(string value) => int.TryParse(value, out var mib) ? mib : 0;

    internal static IReadOnlyList<string> FpuValues(string cpu) => cpu switch
    {
        "68000" or "68010" => ["0"], "68020" or "68030" => ["0", "68881", "68882"],
        _ => ["cpu", "0", "68881", "68882"]
    };

    internal static string CpuDisplayName(string cpu) => cpu == "68020" ? "Motorola 68EC020" : $"Motorola {cpu}";
    internal static string DefaultFpu(string cpu) => cpu is "68040" or "68060" ? "cpu" : "0";
    internal static double NominalCpuFrequencyMhz(AmigaModel model, bool ntsc) => model.Id switch
    {
        "A1200" or "CD32" => ntsc ? 14.31818d : 14.18758d,
        "A3000" or "A4000" => 25d,
        _ => ntsc ? 7.15909d : 7.09379d
    };
}

internal sealed record OptionChoice(string Value, string Text) { public override string ToString() => Text; }
internal sealed record RendererChoice(EmulationVideoRenderer Renderer, string Label);
internal sealed record CpuFrequencyChoice(double Ratio, string ThrottleValue, string MultiplierValue, string Text)
{
    public override string ToString() => Text;
}
