namespace GWGUI.Emulation.Atari.Emulators.Libretro.Constants;



// Unique to GWGUI.Emulation.Atari.
internal static class CoreOptionConstants
{
    internal const uint SupportedInterfaceVersion = 2;
    internal const int MaximumDefinitions = 1024;
    internal const int MaximumCategories = 256;
    internal const int MaximumValues = 128;
    internal const int LegacyDefinitionPointerCount = 3;
    internal const int VersionTwoDefinitionPointerCountBeforeValues = 6;
    internal const int CategoryPointerCount = 3;
    internal const int InternationalPointerCount = 2;
    internal const int ValuePointerCount = 2;
    internal const int DefaultValuePointerCount = 1;
    internal const int TerminatedArrayEntryCount = 2;
    internal const int KeyPointerIndex = 0;
    internal const int NamePointerIndex = 1;
    internal const int DescriptionPointerIndex = 2;
    internal const int CategorizedNamePointerIndex = 2;
    internal const int VersionTwoDescriptionPointerIndex = 3;
    internal const int CategorizedDescriptionPointerIndex = 4;
    internal const int CategoryKeyPointerIndex = 5;
    internal const int EnglishPointerIndex = 0;
    internal const int LocalPointerIndex = 1;
    internal const int CategoriesPointerIndex = 0;
    internal const int DefinitionsPointerIndex = 1;
    internal const int ValuePointerIndex = 0;
    internal const int LabelPointerIndex = 1;
    internal const int FirstEntryIndex = 0;
    internal const int NoEntries = 0;
    internal const int LegacyDefinitionPartLimit = 2;
    internal const int LegacyNamePartIndex = 0;
    internal const int LegacyValuesPartIndex = 1;
    internal const char LegacyDefinitionSeparator = ';';
    internal const char LegacyValueSeparator = '|';
}
