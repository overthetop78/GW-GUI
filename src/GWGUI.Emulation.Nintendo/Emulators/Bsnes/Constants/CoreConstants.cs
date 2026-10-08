using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Bsnes.Constants;

internal static class CoreConstants
{
    internal const string Id = "bsnes";
    internal const string DisplayName = "bsnes";
    internal const string LibraryName = "bsnes";
    internal const string LibraryFileName = "bsnes_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/bsnes_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.bsnes.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".smc", ".sfc", ".gb", ".gbc", ".bs"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
