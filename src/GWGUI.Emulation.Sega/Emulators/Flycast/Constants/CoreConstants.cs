using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.Flycast.Constants;

internal static class CoreConstants
{
    internal const string Id = "flycast";
    internal const string DisplayName = "Flycast";
    internal const string LibraryName = "Flycast";
    internal const string LibraryFileName = "flycast_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/flycast_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.flycast.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".chd", ".cdi", ".elf", ".cue", ".gdi", ".lst", ".bin", ".dat", ".zip", ".7z", ".m3u"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Dreamcast, ModelConstants.Naomi, ModelConstants.Naomi2, ModelConstants.Atomiswave, ModelConstants.SystemSp }), SelectFirstInsertedMedia: true);
}

