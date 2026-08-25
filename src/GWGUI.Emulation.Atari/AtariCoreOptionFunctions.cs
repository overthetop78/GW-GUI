using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari;

internal static class AtariCoreOptionFunctions
{
    internal static IReadOnlyList<AtariCoreOption> CopyLegacyDefinitions(nint definitions) =>
        CopyDefinitions(definitions, versionTwo: false, []);

    internal static IReadOnlyList<AtariCoreOption> CopyVersionTwoDefinitions(
        nint options, out IReadOnlyList<AtariCoreOptionCategory> categories)
    {
        categories = [];
        if (options == nint.Zero) return [];
        var categoryPointer = PointerAt(options, AtariCoreOptionConstants.CategoriesPointerIndex);
        var definitionPointer = PointerAt(options, AtariCoreOptionConstants.DefinitionsPointerIndex);
        categories = CopyCategories(categoryPointer);
        return CopyDefinitions(definitionPointer, versionTwo: true, categories);
    }

    internal static nint SelectInternationalDefinitions(nint data)
    {
        if (data == nint.Zero) return nint.Zero;
        var local = PointerAt(data, AtariCoreOptionConstants.LocalPointerIndex);
        return local != nint.Zero ? local : PointerAt(data, AtariCoreOptionConstants.EnglishPointerIndex);
    }

    internal static IReadOnlyList<AtariCoreOption> MergeLocalizedDefinitions(
        IReadOnlyList<AtariCoreOption> english, IReadOnlyList<AtariCoreOption> localized)
    {
        if (localized.Count == AtariCoreOptionConstants.NoEntries) return english;
        var localByKey = localized.ToDictionary(option => option.Key, StringComparer.Ordinal);
        return english.Select(option => localByKey.TryGetValue(option.Key, out var local)
            ? local with { DefaultValue = option.DefaultValue }
            : option).ToArray();
    }

    internal static IReadOnlyList<AtariCoreOptionCategory> MergeLocalizedCategories(
        IReadOnlyList<AtariCoreOptionCategory> english, IReadOnlyList<AtariCoreOptionCategory> localized)
    {
        if (localized.Count == AtariCoreOptionConstants.NoEntries) return english;
        var localByKey = localized.ToDictionary(category => category.Key, StringComparer.Ordinal);
        return english.Select(category => localByKey.GetValueOrDefault(category.Key, category)).ToArray();
    }

    internal static IReadOnlyList<AtariCoreOption> CopyLegacyVariables(nint variables)
    {
        if (variables == nint.Zero) return [];
        var result = new List<AtariCoreOption>();
        var size = Marshal.SizeOf<GWGUI.Emulation.Interop.ExternalCoreApi.Variable>();
        for (var index = AtariCoreOptionConstants.FirstEntryIndex;
             index < AtariCoreOptionConstants.MaximumDefinitions; index++)
        {
            var variable = Marshal.PtrToStructure<GWGUI.Emulation.Interop.ExternalCoreApi.Variable>(variables + index * size);
            var key = CopyString(variable.Key);
            if (key is null) break;
            var definition = CopyString(variable.Value) ?? string.Empty;
            var parts = definition.Split(AtariCoreOptionConstants.LegacyDefinitionSeparator,
                AtariCoreOptionConstants.LegacyDefinitionPartLimit, StringSplitOptions.TrimEntries);
            var name = parts.ElementAtOrDefault(AtariCoreOptionConstants.LegacyNamePartIndex) ?? key;
            var values = (parts.ElementAtOrDefault(AtariCoreOptionConstants.LegacyValuesPartIndex) ?? string.Empty)
                .Split(AtariCoreOptionConstants.LegacyValueSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => new AtariCoreOptionValue(value, value)).ToArray();
            var defaultValue = values.FirstOrDefault()?.Value ?? string.Empty;
            result.Add(new(key, name, null, null, defaultValue, defaultValue, values));
        }
        return result;
    }

    private static IReadOnlyList<AtariCoreOptionCategory> CopyCategories(nint categories)
    {
        if (categories == nint.Zero) return [];
        var result = new List<AtariCoreOptionCategory>();
        var size = AtariCoreOptionConstants.CategoryPointerCount * IntPtr.Size;
        for (var index = AtariCoreOptionConstants.FirstEntryIndex;
             index < AtariCoreOptionConstants.MaximumCategories; index++)
        {
            var current = categories + index * size;
            var key = StringAt(current, AtariCoreOptionConstants.KeyPointerIndex);
            if (key is null) break;
            result.Add(new(key, StringAt(current, AtariCoreOptionConstants.NamePointerIndex) ?? key,
                StringAt(current, AtariCoreOptionConstants.DescriptionPointerIndex)));
        }
        return result;
    }

    private static IReadOnlyList<AtariCoreOption> CopyDefinitions(nint definitions, bool versionTwo,
        IReadOnlyList<AtariCoreOptionCategory> categories)
    {
        if (definitions == nint.Zero) return [];
        var prefix = versionTwo
            ? AtariCoreOptionConstants.VersionTwoDefinitionPointerCountBeforeValues
            : AtariCoreOptionConstants.LegacyDefinitionPointerCount;
        var size = (prefix + AtariCoreOptionConstants.MaximumValues * AtariCoreOptionConstants.ValuePointerCount
            + AtariCoreOptionConstants.DefaultValuePointerCount) * IntPtr.Size;
        var valuesOffset = prefix * IntPtr.Size;
        var defaultOffset = valuesOffset + AtariCoreOptionConstants.MaximumValues
            * AtariCoreOptionConstants.ValuePointerCount * IntPtr.Size;
        var categoryKeys = categories.Select(category => category.Key).ToHashSet(StringComparer.Ordinal);
        var result = new List<AtariCoreOption>();
        for (var index = AtariCoreOptionConstants.FirstEntryIndex;
             index < AtariCoreOptionConstants.MaximumDefinitions; index++)
        {
            var current = definitions + index * size;
            var key = StringAt(current, AtariCoreOptionConstants.KeyPointerIndex);
            if (key is null) break;
            var name = StringAt(current, AtariCoreOptionConstants.NamePointerIndex) ?? key;
            var descriptionIndex = versionTwo
                ? AtariCoreOptionConstants.VersionTwoDescriptionPointerIndex
                : AtariCoreOptionConstants.DescriptionPointerIndex;
            var description = StringAt(current, descriptionIndex);
            var category = versionTwo ? StringAt(current, AtariCoreOptionConstants.CategoryKeyPointerIndex) : null;
            if (category is not null && !categoryKeys.Contains(category)) category = null;
            var values = CopyValues(current + valuesOffset);
            var declaredDefault = CopyString(Marshal.ReadIntPtr(current, defaultOffset));
            var defaultValue = declaredDefault is not null && values.Any(value => value.Value == declaredDefault)
                ? declaredDefault : values.FirstOrDefault()?.Value ?? string.Empty;
            result.Add(new(key, name, description, category, defaultValue, defaultValue, values, true,
                versionTwo ? StringAt(current, AtariCoreOptionConstants.CategorizedNamePointerIndex) : null,
                versionTwo ? StringAt(current, AtariCoreOptionConstants.CategorizedDescriptionPointerIndex) : null));
        }
        return result;
    }

    private static IReadOnlyList<AtariCoreOptionValue> CopyValues(nint values)
    {
        var result = new List<AtariCoreOptionValue>();
        var size = AtariCoreOptionConstants.ValuePointerCount * IntPtr.Size;
        for (var index = AtariCoreOptionConstants.FirstEntryIndex;
             index < AtariCoreOptionConstants.MaximumValues; index++)
        {
            var current = values + index * size;
            var value = StringAt(current, AtariCoreOptionConstants.ValuePointerIndex);
            if (value is null) break;
            result.Add(new(value, StringAt(current, AtariCoreOptionConstants.LabelPointerIndex) ?? value));
        }
        return result;
    }

    private static nint PointerAt(nint structure, int pointerIndex) =>
        Marshal.ReadIntPtr(structure, pointerIndex * IntPtr.Size);

    private static string? StringAt(nint structure, int pointerIndex) =>
        CopyString(PointerAt(structure, pointerIndex));

    private static string? CopyString(nint pointer) =>
        pointer == nint.Zero ? null : Marshal.PtrToStringUTF8(pointer);
}
