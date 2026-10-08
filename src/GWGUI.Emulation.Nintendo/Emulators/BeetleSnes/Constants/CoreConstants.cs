using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BeetleSnes.Constants;

internal static class CoreConstants
{
    internal const string Id = "mednafen_snes";
    internal const string DisplayName = "Beetle SNES";
    internal const string LibraryName = "Mednafen bSNES";
    internal const string LibraryFileName = "mednafen_snes_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_snes_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mednafen_snes.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".smc", ".fig", ".bs", ".st", ".sfc"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
