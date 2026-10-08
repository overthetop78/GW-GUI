using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.BeetleSaturn.Constants;

internal static class CoreConstants
{
    internal const string Id = "mednafen_saturn";
    internal const string DisplayName = "Beetle Saturn";
    internal const string LibraryName = "Beetle Saturn";
    internal const string LibraryFileName = "mednafen_saturn_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_saturn_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mednafen_saturn.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".cue", ".ccd", ".chd", ".toc", ".m3u", ".zip"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }), SelectFirstInsertedMedia: true);
}

