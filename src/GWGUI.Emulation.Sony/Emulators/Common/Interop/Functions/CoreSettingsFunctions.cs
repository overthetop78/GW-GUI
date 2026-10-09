using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using System.Globalization;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class CoreSettingsFunctions
{
    internal static MachineConfiguration Configure(CoreDefinition definition, MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options
            ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var option in definition.Options)
        {
            var selected = configuration.Options?.GetValueOrDefault(option.Key) ?? option.DefaultValue;
            options[option.Key] = option.NumericRange is null
                ? option.Values.Any(value => value.Value == selected) ? selected : option.DefaultValue
                : NativeNumber(option, selected);
        }
        return configuration with { Options = options };
    }

    internal static IReadOnlyList<EmulationSettingsBlock> Blocks(CoreDefinition definition,
        MachineConfiguration configuration)
    {
        var fields = definition.Options.Where(option => option.IsVisible).Select(option =>
            new EmulationSettingsField(option.Key, option.Tab, CoreSettingsConstants.OptionsBlock + option.Tab,
                option.Name, option.NumericRange is null ? EmulationSettingsEditor.Selection : EmulationSettingsEditor.Slider,
                option.NumericRange is null
                    ? configuration.Options?.GetValueOrDefault(option.Key) ?? option.DefaultValue
                    : ReadNumber(option, configuration.Options?.GetValueOrDefault(option.Key) ?? option.DefaultValue)
                        .ToString(CoreSettingsConstants.NumericFormat, CultureInfo.InvariantCulture),
                option.Values.Select(value => value.Label.StartsWith(CoreSettingsConstants.OptionResourcePrefix,
                    StringComparison.Ordinal)
                    ? new EmulationSettingsChoice(value.Value, value.Label)
                    : new EmulationSettingsChoice(value.Value, string.Empty, value.Label)).ToArray(),
                ExplanationResourceKey: option.Description, RequiresRestart: option.RequiresRestart,
                NumericRange: option.NumericRange)).ToArray();
        return fields.GroupBy(field => field.Tab).Select(group => new EmulationSettingsBlock(
            CoreSettingsConstants.OptionsBlock + group.Key, group.Key,
            CoreSettingsConstants.Sections[group.Key].Title, group.ToArray(),
            CoreSettingsConstants.Sections[group.Key].Icon,
            SettingsDescriptionFunctionsConstants.SingleColumnLayout)).ToArray();
    }

    private static double ReadNumber(CoreOption option, string selected)
    {
        var range = option.NumericRange!;
        if (selected == option.NativeZeroValue) return 0;
        if (range.UnitSuffix.Length != 0 && selected.EndsWith(range.UnitSuffix, StringComparison.Ordinal))
            selected = selected[..^range.UnitSuffix.Length];
        if (!double.TryParse(selected, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            || !double.IsFinite(value))
            return selected == option.DefaultValue ? range.Minimum : ReadNumber(option, option.DefaultValue);
        var clamped = Math.Clamp(value, range.Minimum, range.Maximum);
        return Math.Clamp(range.Minimum + Math.Round((clamped - range.Minimum) / range.Step,
            MidpointRounding.AwayFromZero) * range.Step, range.Minimum, range.Maximum);
    }

    private static string NativeNumber(CoreOption option, string selected)
    {
        var number = ReadNumber(option, selected);
        return number == 0 && option.NativeZeroValue is not null ? option.NativeZeroValue
            : number.ToString(option.NativeNumericFormat ?? CoreSettingsConstants.NumericFormat,
                CultureInfo.InvariantCulture) + option.NumericRange!.UnitSuffix;
    }
}
