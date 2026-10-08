using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Constants;

internal static class CoreConstants
{
    internal const string Id = "mupen64plus-next";
    internal const string DisplayName = "Mupen64Plus-Next";
    internal const string LibraryName = "Mupen64Plus-Next";
    internal const string LibraryFileName = "mupen64plus_next_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mupen64plus_next_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mupen64plus-next.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".n64", ".v64", ".z64", ".bin", ".u1"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nintendo64 }));
}
