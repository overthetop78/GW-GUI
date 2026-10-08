using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BsnesCpp98.Constants;

internal static class CoreConstants
{
    internal const string Id = "bsnes_cplusplus98";
    internal const string DisplayName = "bsnes C++98";
    internal const string LibraryName = "bsnes";
    internal const string LibraryFileName = "bsnes_cplusplus98_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/bsnes_cplusplus98_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.bsnes_cplusplus98.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".sfc", ".smc", ".gb", ".gbc", ".st", ".bs"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        [], [], new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
