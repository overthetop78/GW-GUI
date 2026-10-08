using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.YabaSanshiro.Constants;

internal static class CoreConstants
{
    internal const string Id = "yabasanshiro";
    internal const string DisplayName = "YabaSanshiro";
    internal const string LibraryName = "YabaSanshiro";
    internal const string LibraryFileName = "yabasanshiro_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/yabasanshiro_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.yabasanshiro.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".cue", ".iso", ".mds", ".ccd", ".chd"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }), SelectFirstInsertedMedia: true);
}

