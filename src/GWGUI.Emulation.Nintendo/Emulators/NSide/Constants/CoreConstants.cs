using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.NSide.Constants;

internal static class CoreConstants
{
    internal const string Id = "nside_sfc_balanced";
    internal const string DisplayName = "nSide";
    internal const string LibraryName = "higan (Super Famicom Balanced)";
    internal const string LibraryFileName = "nside_sfc_balanced_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/nside_sfc_balanced_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.nside_sfc_balanced.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".sfc", ".smc", ".gb", ".gbc", ".bml", ".rom"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
