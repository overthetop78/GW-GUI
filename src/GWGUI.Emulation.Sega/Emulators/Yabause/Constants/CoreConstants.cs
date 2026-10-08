using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.Yabause.Constants;

internal static class CoreConstants
{
    internal const string Id = "yabause";
    internal const string DisplayName = "Yabause";
    internal const string LibraryName = "Yabause";
    internal const string LibraryFileName = "yabause_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/yabause_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.yabause.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".cue", ".iso", ".mds", ".ccd", ".zip", ".chd", ".m3u"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }), SelectFirstInsertedMedia: true);
}

