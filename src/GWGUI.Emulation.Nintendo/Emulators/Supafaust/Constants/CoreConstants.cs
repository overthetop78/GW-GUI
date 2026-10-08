using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Supafaust.Constants;

internal static class CoreConstants
{
    internal const string Id = "mednafen_supafaust";
    internal const string DisplayName = "Supafaust";
    internal const string LibraryName = "Beetle Supafaust";
    internal const string LibraryFileName = "mednafen_supafaust_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_supafaust_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mednafen_supafaust.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".smc", ".swc", ".sfc", ".fig"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
