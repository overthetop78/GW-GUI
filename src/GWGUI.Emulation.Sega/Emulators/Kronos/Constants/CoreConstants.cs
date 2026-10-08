using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.Kronos.Constants;

internal static class CoreConstants
{
    internal const string Id = "kronos";
    internal const string DisplayName = "Kronos";
    internal const string LibraryName = "Kronos";
    internal const string LibraryFileName = "kronos_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/kronos_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.kronos.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".cue", ".iso", ".mds", ".ccd", ".zip", ".chd", ".m3u"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Saturn, ModelConstants.StV }), SelectFirstInsertedMedia: true);
}

