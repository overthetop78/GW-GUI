using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Emulators.Libretro.Functions;

internal static class CoreOptionFunctions
{
    internal static IReadOnlyList<CoreOption> CopyLegacyDefinitions(nint definitions) =>
        CopyDefinitions(definitions, versionTwo: false, []);

    internal static IReadOnlyList<CoreOption> CopyVersionTwoDefinitions(
        nint options, out IReadOnlyList<CoreOptionCategory> categories)
    {
        categories = [];
        if (options == nint.Zero) return [];
        var categoryPointer = PointerAt(options, CoreOptionConstants.CategoriesPointerIndex);
        var definitionPointer = PointerAt(options, CoreOptionConstants.DefinitionsPointerIndex);
        categories = CopyCategories(categoryPointer);
        return CopyDefinitions(definitionPointer, versionTwo: true, categories);
    }

    internal static nint SelectInternationalDefinitions(nint data)
    {
        if (data == nint.Zero) return nint.Zero;
        var local = PointerAt(data, CoreOptionConstants.LocalPointerIndex);
        return local != nint.Zero ? local : PointerAt(data, CoreOptionConstants.EnglishPointerIndex);
    }

    internal static IReadOnlyList<CoreOption> MergeLocalizedDefinitions(
        IReadOnlyList<CoreOption> english, IReadOnlyList<CoreOption> localized)
    {
        if (localized.Count == CoreOptionConstants.NoEntries) return english;
        var localByKey = localized.ToDictionary(option => option.Key, StringComparer.Ordinal);
        return english.Select(option => localByKey.TryGetValue(option.Key, out var local)
            ? local with { DefaultValue = option.DefaultValue }
            : option).ToArray();
    }

    internal static IReadOnlyList<CoreOptionCategory> MergeLocalizedCategories(
        IReadOnlyList<CoreOptionCategory> english, IReadOnlyList<CoreOptionCategory> localized)
    {
        if (localized.Count == CoreOptionConstants.NoEntries) return english;
        var localByKey = localized.ToDictionary(category => category.Key, StringComparer.Ordinal);
        return english.Select(category => localByKey.GetValueOrDefault(category.Key, category)).ToArray();
    }

    internal static IReadOnlyList<CoreOption> CopyLegacyVariables(nint variables)
    {
        if (variables == nint.Zero) return [];
        var result = new List<CoreOption>();
        var size = Marshal.SizeOf<GWGUI.Emulation.Interop.ExternalCoreApi.Variable>();
        for (var index = CoreOptionConstants.FirstEntryIndex;
             index < CoreOptionConstants.MaximumDefinitions; index++)
        {
            var variable = Marshal.PtrToStructure<GWGUI.Emulation.Interop.ExternalCoreApi.Variable>(variables + index * size);
            var key = CopyString(variable.Key);
            if (key is null) break;
            var definition = CopyString(variable.Value) ?? string.Empty;
            var parts = definition.Split(CoreOptionConstants.LegacyDefinitionSeparator,
                CoreOptionConstants.LegacyDefinitionPartLimit, StringSplitOptions.TrimEntries);
            var name = parts.ElementAtOrDefault(CoreOptionConstants.LegacyNamePartIndex) ?? key;
            var values = (parts.ElementAtOrDefault(CoreOptionConstants.LegacyValuesPartIndex) ?? string.Empty)
                .Split(CoreOptionConstants.LegacyValueSeparator,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => new CoreOptionValue(value, value)).ToArray();
            var defaultValue = values.FirstOrDefault()?.Value ?? string.Empty;
            result.Add(new(key, name, null, null, defaultValue, defaultValue, values));
        }
        return result;
    }

    private static IReadOnlyList<CoreOptionCategory> CopyCategories(nint categories)
    {
        if (categories == nint.Zero) return [];
        var result = new List<CoreOptionCategory>();
        var size = CoreOptionConstants.CategoryPointerCount * IntPtr.Size;
        for (var index = CoreOptionConstants.FirstEntryIndex;
             index < CoreOptionConstants.MaximumCategories; index++)
        {
            var current = categories + index * size;
            var key = StringAt(current, CoreOptionConstants.KeyPointerIndex);
            if (key is null) break;
            result.Add(new(key, StringAt(current, CoreOptionConstants.NamePointerIndex) ?? key,
                StringAt(current, CoreOptionConstants.DescriptionPointerIndex)));
        }
        return result;
    }

    private static IReadOnlyList<CoreOption> CopyDefinitions(nint definitions, bool versionTwo,
        IReadOnlyList<CoreOptionCategory> categories)
    {
        if (definitions == nint.Zero) return [];
        var prefix = versionTwo
            ? CoreOptionConstants.VersionTwoDefinitionPointerCountBeforeValues
            : CoreOptionConstants.LegacyDefinitionPointerCount;
        var size = (prefix + CoreOptionConstants.MaximumValues * CoreOptionConstants.ValuePointerCount
            + CoreOptionConstants.DefaultValuePointerCount) * IntPtr.Size;
        var valuesOffset = prefix * IntPtr.Size;
        var defaultOffset = valuesOffset + CoreOptionConstants.MaximumValues
            * CoreOptionConstants.ValuePointerCount * IntPtr.Size;
        var categoryKeys = categories.Select(category => category.Key).ToHashSet(StringComparer.Ordinal);
        var result = new List<CoreOption>();
        for (var index = CoreOptionConstants.FirstEntryIndex;
             index < CoreOptionConstants.MaximumDefinitions; index++)
        {
            var current = definitions + index * size;
            var key = StringAt(current, CoreOptionConstants.KeyPointerIndex);
            if (key is null) break;
            var name = StringAt(current, CoreOptionConstants.NamePointerIndex) ?? key;
            var descriptionIndex = versionTwo
                ? CoreOptionConstants.VersionTwoDescriptionPointerIndex
                : CoreOptionConstants.DescriptionPointerIndex;
            var description = StringAt(current, descriptionIndex);
            var category = versionTwo ? StringAt(current, CoreOptionConstants.CategoryKeyPointerIndex) : null;
            if (category is not null && !categoryKeys.Contains(category)) category = null;
            var values = CopyValues(current + valuesOffset);
            var declaredDefault = CopyString(Marshal.ReadIntPtr(current, defaultOffset));
            var defaultValue = declaredDefault is not null && values.Any(value => value.Value == declaredDefault)
                ? declaredDefault : values.FirstOrDefault()?.Value ?? string.Empty;
            result.Add(new(key, name, description, category, defaultValue, defaultValue, values, true,
                versionTwo ? StringAt(current, CoreOptionConstants.CategorizedNamePointerIndex) : null,
                versionTwo ? StringAt(current, CoreOptionConstants.CategorizedDescriptionPointerIndex) : null));
        }
        return result;
    }

    private static IReadOnlyList<CoreOptionValue> CopyValues(nint values)
    {
        var result = new List<CoreOptionValue>();
        var size = CoreOptionConstants.ValuePointerCount * IntPtr.Size;
        for (var index = CoreOptionConstants.FirstEntryIndex;
             index < CoreOptionConstants.MaximumValues; index++)
        {
            var current = values + index * size;
            var value = StringAt(current, CoreOptionConstants.ValuePointerIndex);
            if (value is null) break;
            result.Add(new(value, StringAt(current, CoreOptionConstants.LabelPointerIndex) ?? value));
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
