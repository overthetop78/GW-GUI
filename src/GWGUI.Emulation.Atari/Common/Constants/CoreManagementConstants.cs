namespace GWGUI.Emulation.Atari.Common.Constants;

internal static class EmulatorCatalogConstants
{
    internal const string HatariId = "hatari";
    internal const string Atari800Id = "atari800";
    internal const string StellaId = "stella2023";
    internal const string ProSystemId = "prosystem";
    internal const string BeetleLynxId = "beetle-lynx";
    internal const string VirtualJaguarId = "virtual-jaguar";

    internal const string HatariDllName = "hatari_libretro.dll";
    internal const string Atari800DllName = "atari800_libretro.dll";
    internal const string StellaDllName = "stella2023_libretro.dll";
    internal const string ProSystemDllName = "prosystem_libretro.dll";
    internal const string BeetleLynxDllName = "mednafen_lynx_libretro.dll";
    internal const string VirtualJaguarDllName = "virtualjaguar_libretro.dll";

    internal const string ArchiveExtension = ".zip";
    internal const string ManifestFileName = "core.json";
    internal const string ActiveManifestFileName = "active.json";
    internal const string WindowsX64Architecture = "x64";

    internal const string BuildServerRoot =
        "https://buildbot.libretro.com/nightly/windows/x86_64/latest/";
    internal const string HatariSource = "https://github.com/libretro/hatari";
    internal const string Atari800Source = "https://github.com/libretro/libretro-atari800";
    internal const string StellaSource = "https://github.com/libretro/stella";
    internal const string ProSystemSource = "https://github.com/libretro/prosystem-libretro";
    internal const string BeetleLynxSource = "https://github.com/libretro/beetle-lynx-libretro";
    internal const string VirtualJaguarSource = "https://github.com/libretro/virtualjaguar-libretro";

    internal const string HatariRevision = "24e7bd744f24f20b464385f365a3850c269bd140";
    internal const string Atari800Revision = "cd721790a0aa0e0772810949abcf5bd699c15371";
    internal const string StellaRevision = "878a9c8d5f03ef0b7cd190b5713d6bf31c48df38";
    internal const string ProSystemRevision = "363b6dfbd3e240762e022c2b4897b4fe55722be3";
    internal const string BeetleLynxRevision = "fcdefcfb3c11d6d2e71be076a5d3df2e88ab73ed";
    internal const string VirtualJaguarRevision = "385c4d458538fd473c4bc8dc8dab4778897e8ac6";
}

internal static class EmulatorCatalogErrors
{
    internal const string EmptyInstallationRoot = "The Atari core installation root is empty.";
    internal const string EmptyVersion = "The Atari core version is empty.";
    internal const string DuplicateCore = "The Atari core catalog contains a duplicate core identifier.";
    internal const string DuplicateModel = "An Atari machine model is associated with more than one core.";
    internal const string MissingModel = "An Atari machine model has no associated core.";
}

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
    internal const int HatariDefinitionCount = 50;
    internal const int Atari800DefinitionCount = 44;
    internal const int StellaDefinitionCount = 16;
    internal const int ProSystemDefinitionCount = 4;
    internal const int BeetleLynxDefinitionCount = 3;
    internal const int VirtualJaguarDefinitionCount = 74;
}

public static class CoreOptionProbeConstants
{
    public const string CommandLineArgument = "--atari-option-probe";
    public const int SuccessExitCode = 0;
    public const int FailureExitCode = 1;
    public const int ProcessTimeoutMilliseconds = 30000;
}

internal static class CoreOptionProbeValues
{
    internal const string GWGUIAtariOptionProbe = "GWGUI-Atari-OptionProbe";
    internal const string N = "N";
}

internal static class CoreReleaseConstants
{
    internal const string ReleaseIdPrefix = "official-";
    internal const string ReleaseVersionFormat = "yyyyMMdd-HHmmss";
    internal const string TemporaryDownloadExtension = ".download";
    internal const string TemporaryExtractExtension = ".extract";
    internal const string TemporaryManifestExtension = ".temporary";
    internal const string UnknownDiagnosticValue = "unknown";
    internal const string WindowsX86Architecture = "x86";
    internal const string WindowsArm64Architecture = "arm64";
    internal const int DownloadBufferSize = 81_920;
    internal const double CompletedProgress = 1D;
    internal const int PeExportDirectoryMinimumSize = 40;
    internal const int PeExportNumberOfNamesOffset = 24;
    internal const int PeExportAddressOfNamesOffset = 32;
    internal const int ExportNameRvaSize = sizeof(uint);
    internal const int MaximumExportNameLength = 4_096;
    internal const ushort WindowsX86Machine = 0x014c;
    internal const ushort WindowsX64Machine = 0x8664;
    internal const ushort WindowsArm64Machine = 0xaa64;
}

internal static class CoreReleaseErrors
{
    internal const string MissingPublishedDate =
        "The official Atari core source did not provide a build date.";
    internal const string MissingExpectedLibraryFormat =
        "The official Atari core archive does not contain the expected library '{0}'.";
    internal const string InvalidExportDirectory =
        "The downloaded Atari core has an invalid PE export directory.";
    internal const string InstalledLibraryLockedFormat =
        "The installed Atari core library '{0}' is locked and could not be replaced.";
}
