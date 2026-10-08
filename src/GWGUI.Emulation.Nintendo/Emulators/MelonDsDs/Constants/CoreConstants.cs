using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.MelonDsDs.Constants;

internal static class CoreConstants
{
    internal const string Id = "melondsds";
    internal const string DisplayName = "melonDS DS";
    internal const string LibraryName = "melonDS DS";
    internal const string LibraryFileName = "melondsds_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/melondsds_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.melondsds.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".nds", ".ids", ".dsi"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs, ModelConstants.NintendoDsi }));
}
