using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Functions;

internal delegate TOption EmulationNativeOptionFactory<TOption, TValue>(string key, string name,
    string? description, string? category, string defaultValue, IReadOnlyList<TValue> values,
    string? categorizedName, string? categorizedDescription);

internal delegate IReadOnlyList<TOption> EmulationNativeOptionTableReader<TOption, TCategory>(nint data,
    out IReadOnlyList<TCategory> categories);

public static partial class EmulationCoreOptionFunctions
{
    internal static IReadOnlyList<TOption> CopyInternationalDefinitions<TOption>(nint data,
        int englishPointerIndex, int localPointerIndex, Func<nint, IReadOnlyList<TOption>> read,
        Func<IReadOnlyList<TOption>, IReadOnlyList<TOption>, IReadOnlyList<TOption>> merge)
    {
        if (data == nint.Zero) return [];
        var englishPointer = Marshal.ReadIntPtr(data, englishPointerIndex * IntPtr.Size);
        var localPointer = Marshal.ReadIntPtr(data, localPointerIndex * IntPtr.Size);
        var english = read(englishPointer);
        var local = read(localPointer);
        return merge(english, local);
    }

    internal static IReadOnlyList<TOption> CopyInternationalDefinitions<TOption, TCategory>(nint data,
        int englishPointerIndex, int localPointerIndex,
        EmulationNativeOptionTableReader<TOption, TCategory> read,
        Func<IReadOnlyList<TOption>, IReadOnlyList<TOption>, IReadOnlyList<TOption>> mergeOptions,
        Func<IReadOnlyList<TCategory>, IReadOnlyList<TCategory>, IReadOnlyList<TCategory>> mergeCategories,
        out IReadOnlyList<TCategory> categories)
    {
        categories = [];
        if (data == nint.Zero) return [];
        var englishPointer = Marshal.ReadIntPtr(data, englishPointerIndex * IntPtr.Size);
        var localPointer = Marshal.ReadIntPtr(data, localPointerIndex * IntPtr.Size);
        var english = read(englishPointer, out var englishCategories);
        var local = read(localPointer, out var localCategories);
        var options = mergeOptions(english, local);
        categories = mergeCategories(englishCategories, localCategories);
        return options;
    }

    internal static nint SelectInternationalDefinitions(nint data)
    {
        if (data == nint.Zero) return nint.Zero;
        var local = Marshal.ReadIntPtr(data, IntPtr.Size);
        return local != nint.Zero ? local : Marshal.ReadIntPtr(data);
    }

    internal static IReadOnlyList<T> MergeLocalized<T>(IReadOnlyList<T> english, IReadOnlyList<T> localized,
        Func<T, string> key, Func<T, T, T> merge)
    {
        if (localized.Count == 0) return english;
        var localByKey = localized.ToDictionary(key, StringComparer.Ordinal);
        return english.Select(item => localByKey.TryGetValue(key(item), out var local) ? merge(item, local) : item).ToArray();
    }

    internal static bool ReturnNativeValue(nint data, IReadOnlyDictionary<string, string> values,
        Func<string, string, nint> nativeValue, bool unavailableResult = true)
    {
        if (data == nint.Zero) return unavailableResult;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = NativeOptionCopyString(variable.Key);
        if (key is not null && values.TryGetValue(key, out var value))
        {
            variable.Value = nativeValue(key, value);
            Marshal.StructureToPtr(variable, data, false);
            return true;
        }
        variable.Value = nint.Zero;
        Marshal.StructureToPtr(variable, data, false);
        return unavailableResult;
    }

    internal static bool SetNativeValue(nint data, Func<string, string, bool> setValue)
    {
        if (data == nint.Zero) return false;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = NativeOptionCopyString(variable.Key);
        var value = NativeOptionCopyString(variable.Value);
        return !string.IsNullOrWhiteSpace(key) && value is not null && setValue(key, value);
    }

    internal static bool GetAndClearUpdated(nint data, ref int updated)
    {
        if (data == nint.Zero) return false;
        Marshal.WriteByte(data, Interlocked.Exchange(ref updated, 0) != 0 ? (byte)1 : (byte)0);
        return true;
    }

    internal static bool ApplyNativeVisibility<T>(nint data, IDictionary<string, bool> visibility, IReadOnlyList<T> catalog,
        Func<T, string> keyFor, Func<T, bool, T> update, out IReadOnlyList<T> result)
    {
        result = catalog;
        if (data == nint.Zero) return false;
        var display = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionDisplay>(data);
        var key = NativeOptionCopyString(display.Key);
        if (string.IsNullOrWhiteSpace(key)) return false;
        visibility[key] = display.Visible;
        result = catalog.Select(option => keyFor(option).Equals(key, StringComparison.Ordinal) ? update(option, display.Visible) : option).ToArray();
        return true;
    }

    internal static bool TryGetDisplayCallback(nint data, out ExternalCoreApi.UpdateCoreOptionsDisplay? callback)
    {
        callback = null;
        if (data == nint.Zero) return false;
        var pointer = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionsUpdateDisplayCallback>(data).Callback;
        callback = GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.OptionalDelegate<ExternalCoreApi.UpdateCoreOptionsDisplay>(pointer);
        return true;
    }

    internal static void ValidateConfiguredValues<TOption, TValue>(IEnumerable<string> configuredKeys,
        IReadOnlyDictionary<string, string> values, IReadOnlyList<TOption> catalog, Func<TOption, string> keyFor,
        Func<TOption, IReadOnlyList<TValue>> choicesFor, Func<TValue, string> valueFor, Action<string, string> invalid)
        where TOption : class
    {
        foreach (var key in configuredKeys)
        {
            var option = catalog.FirstOrDefault(item => keyFor(item).Equals(key, StringComparison.Ordinal));
            if (option is null || choicesFor(option).Count == 0) continue;
            if (!choicesFor(option).Any(item => valueFor(item).Equals(values[key], StringComparison.Ordinal))) invalid(key, values[key]);
        }
    }

    private static class NativeOptionLayout
    {

        internal const int MaximumDefinitions = 1024;
        internal const int MaximumCategories = 256;
        internal const int MaximumValues = 128;
        internal const int LegacyDefinitionPointerCount = 3;
        internal const int VersionTwoDefinitionPointerCountBeforeValues = 6;
        internal const int CategoryPointerCount = 3;

        internal const int ValuePointerCount = 2;
        internal const int DefaultValuePointerCount = 1;

        internal const int KeyPointerIndex = 0;
        internal const int NamePointerIndex = 1;
        internal const int DescriptionPointerIndex = 2;
        internal const int CategorizedNamePointerIndex = 2;
        internal const int VersionTwoDescriptionPointerIndex = 3;
        internal const int CategorizedDescriptionPointerIndex = 4;
        internal const int CategoryKeyPointerIndex = 5;


        internal const int CategoriesPointerIndex = 0;
        internal const int DefinitionsPointerIndex = 1;
        internal const int ValuePointerIndex = 0;
        internal const int LabelPointerIndex = 1;
        internal const int FirstEntryIndex = 0;

        internal const int LegacyDefinitionPartLimit = 2;
        internal const int LegacyNamePartIndex = 0;
        internal const int LegacyValuesPartIndex = 1;
        internal const char LegacyDefinitionSeparator = ';';
        internal const char LegacyValueSeparator = '|';
    }

    internal static IReadOnlyList<TOption> CopyLegacyDefinitions<TOption, TValue>(nint definitions,
        EmulationNativeOptionFactory<TOption, TValue> createOption, Func<string, string, TValue> createValue,
        Func<TValue, string> valueText) => CopyNativeDefinitions(definitions, false,
            new HashSet<string>(StringComparer.Ordinal), createOption, createValue, valueText);

    internal static IReadOnlyList<TOption> CopyVersionTwoDefinitions<TOption, TValue, TCategory>(nint options,
        out IReadOnlyList<TCategory> categories, EmulationNativeOptionFactory<TOption, TValue> createOption,
        Func<string, string, TValue> createValue, Func<TValue, string> valueText,
        Func<string, string, string?, TCategory> createCategory, Func<TCategory, string> categoryKey)
    {
        categories = [];
        if (options == nint.Zero) return [];
        var categoryPointer = Marshal.ReadIntPtr(options, NativeOptionLayout.CategoriesPointerIndex * IntPtr.Size);
        var definitionPointer = Marshal.ReadIntPtr(options, NativeOptionLayout.DefinitionsPointerIndex * IntPtr.Size);
        categories = CopyNativeCategories(categoryPointer, createCategory);
        return CopyNativeDefinitions(definitionPointer, true, categories.Select(categoryKey).ToHashSet(StringComparer.Ordinal),
            createOption, createValue, valueText);
    }

    internal static IReadOnlyList<TOption> CopyLegacyVariables<TOption, TValue>(nint variables,
        EmulationNativeOptionFactory<TOption, TValue> createOption, Func<string, string, TValue> createValue,
        Func<TValue, string> valueText)
    {
        if (variables == nint.Zero) return [];
        var result = new List<TOption>();
        var size = Marshal.SizeOf<GWGUI.Emulation.Interop.ExternalCoreApi.Variable>();
        for (var index = NativeOptionLayout.FirstEntryIndex;
             index < NativeOptionLayout.MaximumDefinitions; index++)
        {
            var variable = Marshal.PtrToStructure<GWGUI.Emulation.Interop.ExternalCoreApi.Variable>(variables + index * size);
            var key = NativeOptionCopyString(variable.Key);
            if (key is null) break;
            var definition = NativeOptionCopyString(variable.Value) ?? string.Empty;
            var parts = definition.Split(NativeOptionLayout.LegacyDefinitionSeparator,
                NativeOptionLayout.LegacyDefinitionPartLimit, StringSplitOptions.TrimEntries);
            var name = parts.ElementAtOrDefault(NativeOptionLayout.LegacyNamePartIndex) ?? key;
            var values = (parts.ElementAtOrDefault(NativeOptionLayout.LegacyValuesPartIndex) ?? string.Empty)
                .Split(NativeOptionLayout.LegacyValueSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => createValue(value, value)).ToArray();
            var defaultValue = values.FirstOrDefault() is { } firstValue ? valueText(firstValue) : string.Empty;
            result.Add(createOption(key, name, null, null, defaultValue, values, null, null));
        }
        return result;
    }

    private static IReadOnlyList<TCategory> CopyNativeCategories<TCategory>(nint categories,
        Func<string, string, string?, TCategory> createCategory)
    {
        if (categories == nint.Zero) return [];
        var result = new List<TCategory>();
        var size = NativeOptionLayout.CategoryPointerCount * IntPtr.Size;
        for (var index = NativeOptionLayout.FirstEntryIndex;
             index < NativeOptionLayout.MaximumCategories; index++)
        {
            var current = categories + index * size;
            var key = NativeOptionStringAt(current, NativeOptionLayout.KeyPointerIndex);
            if (key is null) break;
            result.Add(createCategory(key, NativeOptionStringAt(current, NativeOptionLayout.NamePointerIndex) ?? key, NativeOptionStringAt(current, NativeOptionLayout.DescriptionPointerIndex)));
        }
        return result;
    }

    private static IReadOnlyList<TOption> CopyNativeDefinitions<TOption, TValue>(nint definitions, bool versionTwo,
        IReadOnlySet<string> categoryKeys, EmulationNativeOptionFactory<TOption, TValue> createOption,
        Func<string, string, TValue> createValue, Func<TValue, string> valueText)
    {
        if (definitions == nint.Zero) return [];
        var prefix = versionTwo
            ? NativeOptionLayout.VersionTwoDefinitionPointerCountBeforeValues
            : NativeOptionLayout.LegacyDefinitionPointerCount;
        var size = (prefix + NativeOptionLayout.MaximumValues * NativeOptionLayout.ValuePointerCount
            + NativeOptionLayout.DefaultValuePointerCount) * IntPtr.Size;
        var valuesOffset = prefix * IntPtr.Size;
        var defaultOffset = valuesOffset + NativeOptionLayout.MaximumValues
            * NativeOptionLayout.ValuePointerCount * IntPtr.Size;
        var result = new List<TOption>();
        for (var index = NativeOptionLayout.FirstEntryIndex;
             index < NativeOptionLayout.MaximumDefinitions; index++)
        {
            var current = definitions + index * size;
            var key = NativeOptionStringAt(current, NativeOptionLayout.KeyPointerIndex);
            if (key is null) break;
            var name = NativeOptionStringAt(current, NativeOptionLayout.NamePointerIndex) ?? key;
            var descriptionIndex = versionTwo
                ? NativeOptionLayout.VersionTwoDescriptionPointerIndex
                : NativeOptionLayout.DescriptionPointerIndex;
            var description = NativeOptionStringAt(current, descriptionIndex);
            var category = versionTwo ? NativeOptionStringAt(current, NativeOptionLayout.CategoryKeyPointerIndex) : null;
            if (category is not null && !categoryKeys.Contains(category)) category = null;
            var values = CopyNativeValues(current + valuesOffset, createValue);
            var declaredDefault = NativeOptionCopyString(Marshal.ReadIntPtr(current, defaultOffset));
            var defaultValue = declaredDefault is not null && values.Any(value => valueText(value) == declaredDefault)
                ? declaredDefault : values.FirstOrDefault() is { } firstValue ? valueText(firstValue) : string.Empty;
            result.Add(createOption(key, name, description, category, defaultValue, values, versionTwo ? NativeOptionStringAt(current, NativeOptionLayout.CategorizedNamePointerIndex) : null, versionTwo ? NativeOptionStringAt(current, NativeOptionLayout.CategorizedDescriptionPointerIndex) : null));
        }
        return result;
    }

    private static IReadOnlyList<TValue> CopyNativeValues<TValue>(nint values, Func<string, string, TValue> createValue)
    {
        var result = new List<TValue>();
        var size = NativeOptionLayout.ValuePointerCount * IntPtr.Size;
        for (var index = NativeOptionLayout.FirstEntryIndex;
             index < NativeOptionLayout.MaximumValues; index++)
        {
            var current = values + index * size;
            var value = NativeOptionStringAt(current, NativeOptionLayout.ValuePointerIndex);
            if (value is null) break;
            result.Add(createValue(value, NativeOptionStringAt(current, NativeOptionLayout.LabelPointerIndex) ?? value));
        }
        return result;
    }

    private static string? NativeOptionStringAt(nint structure, int pointerIndex) => StringAt(structure, pointerIndex * IntPtr.Size);
    private static string? NativeOptionCopyString(nint pointer) => pointer == nint.Zero ? null : Marshal.PtrToStringUTF8(pointer);
}
