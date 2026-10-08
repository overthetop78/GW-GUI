using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BsnesHd.Constants;

internal static class CoreConstants
{
    internal const string Id = "bsnes_hd_beta";
    internal const string DisplayName = "bsnes HD";
    internal const string LibraryName = "bsnes-hd beta";
    internal const string LibraryFileName = "bsnes_hd_beta_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/bsnes_hd_beta_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.bsnes_hd_beta.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".smc", ".sfc", ".gb", ".gbc", ".bs"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
