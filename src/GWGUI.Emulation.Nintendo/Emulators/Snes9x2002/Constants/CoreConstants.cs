using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x2002.Constants;

internal static class CoreConstants
{
    internal const string Id = "snes9x2002";
    internal const string DisplayName = "Snes9x 2002";
    internal const string LibraryName = "Snes9x 2002";
    internal const string LibraryFileName = "snes9x2002_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/snes9x2002_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.snes9x2002.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".smc", ".fig", ".sfc", ".gd3", ".gd7", ".dx2", ".bsx", ".swc"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
