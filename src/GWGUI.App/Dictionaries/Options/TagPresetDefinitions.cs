namespace GWGUI.App.Dictionaries.Options;

internal static class TagPresetDefinitions
{
    internal static readonly (string Key, string Pattern)[] All =
    [
        ("Options.TagPresetFamily", "[{FAMILY}] "),
        ("Options.TagPresetFormat", "[{FORMAT}] "),
        ("Options.TagPresetFamilyFormat", "[{FAMILY}-{FORMAT}] "),
        ("Options.TagPresetFamilyExtension", "[{FAMILY}-{EXTENSION}] "),
        ("Options.TagPresetDetailed", "[{FAMILY}-{FORMAT}-{EXTENSION}] ")
    ];
}
