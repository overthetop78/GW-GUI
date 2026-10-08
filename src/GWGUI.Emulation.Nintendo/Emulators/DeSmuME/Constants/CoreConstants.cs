using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.DeSmuME.Constants;

internal static class CoreConstants
{
    internal const string Id = "desmume";
    internal const string DisplayName = "DeSmuME";
    internal const string LibraryName = "DeSmuME";
    internal const string LibraryFileName = "desmume_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/desmume_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.desmume.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".nds", ".ids", ".bin"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }));
}
