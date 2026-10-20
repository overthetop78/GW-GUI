namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Functions;

internal static class CoreOptionFunctions
{
    private static CoreOption CreateOption(string key, string name, string? description, string? category, string defaultValue,
        IReadOnlyList<CoreOptionValue> values, string? categorizedName, string? categorizedDescription) =>
        new(key, name, description, category, defaultValue, defaultValue, values, true, categorizedName, categorizedDescription);

    internal static IReadOnlyList<CoreOption> CopyLegacyDefinitions(nint data) =>
        EmulationCoreOptionFunctions.CopyLegacyDefinitions<CoreOption, CoreOptionValue>(data, CreateOption,
            static (value, label) => new(value, label), static value => value.Value);

    internal static IReadOnlyList<CoreOption> CopyLegacyVariables(nint data) =>
        EmulationCoreOptionFunctions.CopyLegacyVariables<CoreOption, CoreOptionValue>(data, CreateOption,
            static (value, label) => new(value, label), static value => value.Value);

    internal static IReadOnlyList<CoreOption> CopyVersionTwoDefinitions(nint data, out IReadOnlyList<CoreOptionCategory> categories) =>
        EmulationCoreOptionFunctions.CopyVersionTwoDefinitions<CoreOption, CoreOptionValue, CoreOptionCategory>(data, out categories,
            CreateOption, static (value, label) => new(value, label), static value => value.Value,
            static (key, name, description) => new(key, name, description), static category => category.Key);

    internal static IReadOnlyList<CoreOption> MergeLocalizedDefinitions(IReadOnlyList<CoreOption> english, IReadOnlyList<CoreOption> localized) =>
        EmulationCoreOptionFunctions.MergeLocalized(english, localized, static option => option.Key,
            static (option, local) => local with { DefaultValue = option.DefaultValue });

    internal static IReadOnlyList<CoreOptionCategory> MergeLocalizedCategories(IReadOnlyList<CoreOptionCategory> english, IReadOnlyList<CoreOptionCategory> localized) =>
        EmulationCoreOptionFunctions.MergeLocalized(english, localized, static category => category.Key, static (_, local) => local);
}
