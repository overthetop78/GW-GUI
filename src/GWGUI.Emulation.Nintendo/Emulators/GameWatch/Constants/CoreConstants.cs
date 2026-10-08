using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.GameWatch.Constants;

internal static class CoreConstants
{
    internal const string Id = "gw";
    internal const string DisplayName = "GW";
    internal const string LibraryName = "Game & Watch";
    internal const string LibraryFileName = "gw_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/gw_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.gw.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".mgw"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameWatch }));
}
