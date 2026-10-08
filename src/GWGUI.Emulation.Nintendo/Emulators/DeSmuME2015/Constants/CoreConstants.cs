using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.DeSmuME2015.Constants;

internal static class CoreConstants
{
    internal const string Id = "desmume2015";
    internal const string DisplayName = "DeSmuME 2015";
    internal const string LibraryName = "DeSmuME 2015";
    internal const string LibraryFileName = "desmume2015_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/desmume2015_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.desmume2015.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".nds", ".bin"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }));
}
