using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BeetleGba.Constants;

internal static class CoreConstants
{
    internal const string Id = "mednafen_gba";
    internal const string DisplayName = "Beetle GBA";
    internal const string LibraryName = "Beetle GBA";
    internal const string LibraryFileName = "mednafen_gba_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_gba_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mednafen_gba.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gba", ".agb", ".bin"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance }));
}
