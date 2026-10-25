using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class CoreOptionMetadataFunctions
{
    internal static IReadOnlyList<CoreOption> ReadLegacy(nint data, Dictionary<string, string> values,
        IReadOnlyDictionary<string, bool> visibility) => WithCurrentValues(Adapt(
            CommonCoreHostFunctions.OptionProcessing.ReadLegacy(data, values, visibility)), values);

    internal static IReadOnlyList<CoreOption>? ReadVersionTwo(nint data, Dictionary<string, string> values,
        IReadOnlyDictionary<string, bool> visibility) =>
        CommonCoreHostFunctions.OptionProcessing.ReadVersionTwo(data, values, visibility) is { } options
            ? WithCurrentValues(Adapt(options), values) : null;

    private static IReadOnlyList<CoreOption> Adapt(IReadOnlyList<EmulationOption> options) => options.Select(option =>
        new CoreOption(option.Key, option.Name, option.Description, option.Category, option.DefaultValue, option.CurrentValue,
            option.Values.Select(value => new CoreOptionValue(value.Value, value.Label)).ToArray(), IsVisible: option.IsVisible)).ToArray();

    internal static IReadOnlyList<CoreOption> WithCurrentValues(IReadOnlyList<CoreOption> options, IReadOnlyDictionary<string, string> currentValues) =>
        options.Select(option => currentValues.TryGetValue(option.Key, out var value) && option.CurrentValue != value ? option with { CurrentValue = value } : option).ToArray();

    internal static IReadOnlyList<CoreOption> Merge(IReadOnlyList<CoreOption> native, IReadOnlyList<CoreOption> declared, EmulationGraphicsApi? graphicsApi = null)
    {
        var descriptions = declared.ToDictionary(option => option.Key, StringComparer.Ordinal);
        return native.Select(option =>
        {
            if (!descriptions.TryGetValue(option.Key, out var description)) return option;
            description = GraphicsSettingsFunctions.Present(description, option.CurrentValue, graphicsApi);
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
                GraphicsApiValues = description.GraphicsApiValues,
                RetainedRendererValues = description.RetainedRendererValues,
                IsGlobalStorageOption = description.IsGlobalStorageOption,
                Values = option.Values.Where(value => description.GraphicsApiValues is null || labels.ContainsKey(value.Value)).Select(value => value with
                {
                    Label = labels.TryGetValue(value.Value, out var label) ? label
                        : value.Value == description.NativeZeroValue ? OptionValueResourceConstants.DisabledLabelResource : value.Label
                }).ToArray()
            };
        }).ToArray();
    }
}
