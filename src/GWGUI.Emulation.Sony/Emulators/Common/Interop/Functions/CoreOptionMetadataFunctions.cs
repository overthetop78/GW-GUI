using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class CoreOptionMetadataFunctions
{
    internal static IReadOnlyList<CoreOption> WithCurrentValues(IReadOnlyList<CoreOption> options, IReadOnlyDictionary<string, string> currentValues) =>
        options.Select(option => currentValues.TryGetValue(option.Key, out var value) && option.CurrentValue != value ? option with { CurrentValue = value } : option).ToArray();

    internal static IReadOnlyList<CoreOption> Merge(IReadOnlyList<CoreOption> native, IReadOnlyList<CoreOption> declared)
    {
        var descriptions = declared.ToDictionary(option => option.Key, StringComparer.Ordinal);
        return native.Select(option =>
        {
            if (!descriptions.TryGetValue(option.Key, out var description)) return option;
            var labels = description.Values.ToDictionary(value => value.Value, value => value.Label, StringComparer.Ordinal);
            return option with
            {
                Name = description.Name,
                Description = description.Description ?? option.Description,
                Category = description.Category ?? CoreSettingsConstants.Sections[description.Tab].Title,
                IsVisible = option.IsVisible && CoreSettingsFunctions.IsVisible(description),
                RequiresRestart = option.RequiresRestart || description.RequiresRestart,
                NumericRange = description.NumericRange,
                NativeNumericFormat = description.NativeNumericFormat,
                NativeZeroValue = description.NativeZeroValue,
                Values = option.Values.Select(value => value with
                {
                    Label = labels.TryGetValue(value.Value, out var label) ? label
                        : value.Value == description.NativeZeroValue ? OptionValueResourceConstants.DisabledLabelResource : value.Label
                }).ToArray()
            };
        }).ToArray();
    }
}
