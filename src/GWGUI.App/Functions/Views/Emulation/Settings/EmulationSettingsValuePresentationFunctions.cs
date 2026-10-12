using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;
using System.Globalization;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static class EmulationSettingsValuePresentationFunctions
{
    internal static string? ScaledFrequency(long? nominalHz, string? scale)
    {
        if (nominalHz is null || !double.TryParse(scale?.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out var percentage) || !double.IsFinite(percentage)) return null;
        return $"{nominalHz.Value / 1_000_000d * percentage / 100d:0.####} MHz";
    }
    internal static string DisplayValue(EmulationSettingsField field, IEmulationModule? module = null)
    {
        var choice = field.Choices?.FirstOrDefault(value => value.Id == field.Value) ?? field.Choices?.FirstOrDefault();
        return choice?.InvariantDisplayValue ?? (choice is null ? field.Value ?? string.Empty : (module is null ? LocExtension.Get(choice.DisplayResourceKey) : LocExtension.GetForModule(module, choice.DisplayResourceKey)));
    }
    internal static long DefaultNumericValue(EmulationSettingsField field)
    {
        if (field.Editor == EmulationSettingsEditor.Toggle)
            return field.Value == field.EnabledValue ? field.NumericValue ?? 0 : 0;
        if (field.Editor == EmulationSettingsEditor.Information
            && field.NumericValue is { } information) return information;
        var selected = field.Choices?.FirstOrDefault(choice => choice.Id == field.Value);
        return MemoryBytes(selected?.InvariantDisplayValue ?? field.Value) ?? selected?.NumericValue ?? 0;
    }

    internal static long? MemoryBytes(string? display)
    {
        if (string.IsNullOrWhiteSpace(display)) return null;
        foreach (var (unit, multiplier) in MemoryUnits)
            if (display.EndsWith(unit, StringComparison.OrdinalIgnoreCase)
                && double.TryParse(display[..^unit.Length].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                && double.IsFinite(number) && number >= 0 && number <= long.MaxValue / multiplier)
                return checked((long)(number * multiplier));
        return null;
    }

    private static readonly (string Unit, long Multiplier)[] MemoryUnits =
    [
        ("GiB", 1024L * 1024 * 1024), ("GB", 1024L * 1024 * 1024),
        ("MiB", 1024L * 1024), ("MB", 1024L * 1024),
        ("KiB", 1024L), ("KB", 1024L), ("B", 1L)
    ];

    internal static (string Value, string Unit) FormatMemorySize(long bytes) => bytes >= 1024 * 1024
        ? ($"{bytes / (1024d * 1024d):0.##}", "MiB")
        : bytes >= 1024 ? ($"{bytes / 1024d:0.##}", "KiB") : (bytes.ToString(), "B");

}
