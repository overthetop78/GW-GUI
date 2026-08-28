using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static class EmulationSettingsValuePresentationFunctions
{
    internal static string DisplayValue(EmulationSettingsField field)
    {
        var choice = field.Choices?.FirstOrDefault(value => value.Id == field.Value)
            ?? field.Choices?.FirstOrDefault();
        return choice?.InvariantDisplayValue ?? (choice is null ? field.Value ?? string.Empty
            : LocExtension.Get(choice.DisplayResourceKey));
    }
    internal static long DefaultNumericValue(EmulationSettingsField field)
    {
        if (field.Editor == EmulationSettingsEditor.Information
            && field.NumericValue is { } information) return information;
        return field.Choices?.FirstOrDefault(choice => choice.Id == field.Value)?.NumericValue ?? 0;
    }

    internal static (string Value, string Unit) FormatMemorySize(long bytes) => bytes >= 1024 * 1024
        ? ($"{bytes / (1024d * 1024d):0.##}", "MiB")
        : bytes >= 1024 ? ($"{bytes / 1024d:0.##}", "KiB") : (bytes.ToString(), "B");

}
