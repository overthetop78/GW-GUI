using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;

using GWGUI.Emulation.Interfaces;

namespace GWGUI.Emulation.Services;

/// <summary>Expose les traitements d’options existants par leur interface, avec les mêmes arguments et leur ordre d’évaluation.</summary>
internal sealed class EmulationCoreOptionService : IEmulationCoreOptionProcessing
{
    /// <inheritdoc/>
    public bool ApplyNativeVisibility<T>(nint data, IDictionary<string, bool> visibility, IReadOnlyList<T> catalog, Func<T, string> keyFor, Func<T, bool, T> update, out IReadOnlyList<T> result)
        => EmulationCoreOptionFunctions.ApplyNativeVisibility<T>(data, visibility, catalog, keyFor, update, out result);

    /// <inheritdoc/>
    public void ApplyValues(IDictionary<string, string> options, IReadOnlyDictionary<string, string?> values, Func<string, bool>? excluded = null)
        => EmulationCoreOptionFunctions.ApplyValues(options, values, excluded);

    /// <inheritdoc/>
    public IReadOnlyList<TOption> CopyInternationalDefinitions<TOption, TCategory>(nint data,
        int englishPointerIndex, int localPointerIndex,
        EmulationNativeOptionTableReader<TOption, TCategory> read,
        Func<IReadOnlyList<TOption>, IReadOnlyList<TOption>, IReadOnlyList<TOption>> mergeOptions,
        Func<IReadOnlyList<TCategory>, IReadOnlyList<TCategory>, IReadOnlyList<TCategory>> mergeCategories,
        out IReadOnlyList<TCategory> categories)
        => EmulationCoreOptionFunctions.CopyInternationalDefinitions<TOption, TCategory>(data, englishPointerIndex, localPointerIndex, read, mergeOptions, mergeCategories, out categories);

    /// <inheritdoc/>
    public IReadOnlyList<TOption> CopyInternationalDefinitions<TOption>(nint data,
        int englishPointerIndex, int localPointerIndex, Func<nint, IReadOnlyList<TOption>> read,
        Func<IReadOnlyList<TOption>, IReadOnlyList<TOption>, IReadOnlyList<TOption>> merge)
        => EmulationCoreOptionFunctions.CopyInternationalDefinitions<TOption>(data, englishPointerIndex, localPointerIndex, read, merge);

    /// <inheritdoc/>
    public IReadOnlyList<TOption> CopyLegacyDefinitions<TOption, TValue>(nint definitions, EmulationNativeOptionFactory<TOption, TValue> createOption, Func<string, string, TValue> createValue, Func<TValue, string> valueText)
        => EmulationCoreOptionFunctions.CopyLegacyDefinitions<TOption, TValue>(definitions, createOption, createValue, valueText);

    /// <inheritdoc/>
    public IReadOnlyList<TOption> CopyLegacyVariables<TOption, TValue>(nint variables, EmulationNativeOptionFactory<TOption, TValue> createOption, Func<string, string, TValue> createValue, Func<TValue, string> valueText)
        => EmulationCoreOptionFunctions.CopyLegacyVariables<TOption, TValue>(variables, createOption, createValue, valueText);

    /// <inheritdoc/>
    public IReadOnlyList<TOption> CopyVersionTwoDefinitions<TOption, TValue, TCategory>(nint options,
        out IReadOnlyList<TCategory> categories, EmulationNativeOptionFactory<TOption, TValue> createOption,
        Func<string, string, TValue> createValue, Func<TValue, string> valueText,
        Func<string, string, string?, TCategory> createCategory, Func<TCategory, string> categoryKey)
        => EmulationCoreOptionFunctions.CopyVersionTwoDefinitions<TOption, TValue, TCategory>(options, out categories, createOption, createValue, valueText, createCategory, categoryKey);

    /// <inheritdoc/>
    public EmulationSettingsField CreatePreparedSettingsField(string key, EmulationMachineTab tab,
        string block, string resourceKey, EmulationSettingsEditor editor, IReadOnlyList<EmulationSettingsChoice> choices,
        Func<string> currentValue, Func<string?> explanation, Func<string> enabledValue,
        Func<string> disabledValue, Func<bool>? isEnabled = null)
        => EmulationCoreOptionFunctions.CreatePreparedSettingsField(key, tab, block, resourceKey, editor, choices, currentValue, explanation, enabledValue, disabledValue, isEnabled);

    /// <inheritdoc/>
    public IReadOnlyList<EmulationSettingsBlock> CreateSettingsBlocks<TOption>(IReadOnlyList<TOption> options,
        Func<TOption, bool> visible, Func<TOption, string?> category,
        Func<EmulationMachineTab, string> resourceFor,
        Func<TOption, EmulationMachineTab, string, EmulationSettingsField> field)
        => EmulationCoreOptionFunctions.CreateSettingsBlocks<TOption>(options, visible, category, resourceFor, field);

    /// <inheritdoc/>
    public EmulationSettingsField CreateSettingsField<TValue>(string key, EmulationMachineTab tab,
        string block, Func<string> resource, IReadOnlyList<TValue> values,
        Func<TValue, EmulationSettingsChoice> choice, Func<TValue, bool> enabledChoice,
        Func<TValue, bool> disabledChoice, Func<TValue, string> nativeValue,
        Func<string> currentValue, Func<string?> explanation, Func<bool>? isEnabled = null)
        where TValue : class
        => EmulationCoreOptionFunctions.CreateSettingsField<TValue>(key, tab, block, resource, values, choice, enabledChoice, disabledChoice, nativeValue, currentValue, explanation, isEnabled);

    /// <inheritdoc/>
    public EmulationSettingsField CreateSettingsFieldWithToggle<TValue>(string key, EmulationMachineTab tab,
        string block, Func<string> resource, IReadOnlyList<TValue> values, Func<TValue, EmulationSettingsChoice> choice,
        Func<bool> isToggle, Func<string> currentValue, Func<string?> explanation,
        Func<bool, string>? enabledValue = null, Func<bool, string>? disabledValue = null)
        where TValue : class
        => EmulationCoreOptionFunctions.CreateSettingsFieldWithToggle<TValue>(key, tab, block, resource, values, choice, isToggle, currentValue, explanation, enabledValue, disabledValue);

    /// <inheritdoc/>
    public HashSet<string> EditableFieldIds(IEnumerable<EmulationSettingsBlock> blocks)
        => EmulationCoreOptionFunctions.EditableFieldIds(blocks);

    /// <inheritdoc/>
    public Dictionary<string, string> FilterForEmulator(IEnumerable<string> allowedKeys, IReadOnlyDictionary<string, string> options, string audioOutputKey, string audioLatencyKey, string audioVolumeKey)
        => EmulationCoreOptionFunctions.FilterForEmulator(allowedKeys, options, audioOutputKey, audioLatencyKey, audioVolumeKey);

    /// <inheritdoc/>
    public bool GetAndClearUpdated(nint data, ref int updated)
        => EmulationCoreOptionFunctions.GetAndClearUpdated(data, ref updated);

    /// <inheritdoc/>
    public IReadOnlyList<T> MergeLocalized<T>(IReadOnlyList<T> english, IReadOnlyList<T> localized, Func<T, string> key, Func<T, T, T> merge)
        => EmulationCoreOptionFunctions.MergeLocalized<T>(english, localized, key, merge);

    /// <inheritdoc/>
    public IReadOnlyList<EmulationOption> ReadLegacy(nint data, IDictionary<string, string> currentValues, IReadOnlyDictionary<string, bool> visibility)
        => EmulationCoreOptionFunctions.ReadLegacy(data, currentValues, visibility);

    /// <inheritdoc/>
    public IReadOnlyList<EmulationOption>? ReadVersionOne(nint definitions, IDictionary<string, string> currentValues, IReadOnlyDictionary<string, bool> visibility)
        => EmulationCoreOptionFunctions.ReadVersionOne(definitions, currentValues, visibility);

    /// <inheritdoc/>
    public IReadOnlyList<EmulationOption>? ReadVersionTwo(nint options, IDictionary<string, string> currentValues, IReadOnlyDictionary<string, bool> visibility)
        => EmulationCoreOptionFunctions.ReadVersionTwo(options, currentValues, visibility);

    /// <inheritdoc/>
    public bool ReturnNativeValue(nint data, IReadOnlyDictionary<string, string> values, Func<string, string, nint> nativeValue, bool unavailableResult = true)
        => EmulationCoreOptionFunctions.ReturnNativeValue(data, values, nativeValue, unavailableResult);

    /// <inheritdoc/>
    public nint SelectInternationalDefinitions(nint data)
        => EmulationCoreOptionFunctions.SelectInternationalDefinitions(data);

    /// <inheritdoc/>
    public bool SetNativeValue(nint data, Func<string, string, bool> setValue)
        => EmulationCoreOptionFunctions.SetNativeValue(data, setValue);

    /// <inheritdoc/>
    public bool TryGetDisplayCallback(nint data, out ExternalCoreApi.UpdateCoreOptionsDisplay? callback)
        => EmulationCoreOptionFunctions.TryGetDisplayCallback(data, out callback);

    /// <inheritdoc/>
    public void ValidateConfiguredValues<TOption, TValue>(IEnumerable<string> configuredKeys,
        IReadOnlyDictionary<string, string> values, IReadOnlyList<TOption> catalog, Func<TOption, string> keyFor,
        Func<TOption, IReadOnlyList<TValue>> choicesFor, Func<TValue, string> valueFor, Action<string, string> invalid)
        where TOption : class
        => EmulationCoreOptionFunctions.ValidateConfiguredValues<TOption, TValue>(configuredKeys, values, catalog, keyFor, choicesFor, valueFor, invalid);

    /// <inheritdoc/>
    public IReadOnlyList<TOption> WithDefaults<TOption>(IReadOnlyList<TOption> options,
        IReadOnlyDictionary<string, string> defaults, IReadOnlyDictionary<string, string> currentValues,
        Func<TOption, string> key, Func<TOption, string> originalDefault,
        Func<TOption, string, string, TOption> update)
        => EmulationCoreOptionFunctions.WithDefaults<TOption>(options, defaults, currentValues, key, originalDefault, update);
}
