using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.Ymir.Constants;

internal static class CoreConstants
{
    internal const string Id = "ymir";
    internal const string DisplayName = "Ymir / Emir";
    internal const string LibraryName = "Emir";
    internal const string LibraryFileName = "ymir_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/ymir_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.ymir.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".cue", ".chd", ".mds", ".ccd", ".iso", ".m3u"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn }), SelectFirstInsertedMedia: true);
}

