using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x.Constants;

internal static class CoreConstants
{
    internal const string Id = "snes9x";
    internal const string DisplayName = "Snes9x";
    internal const string LibraryName = "Snes9x";
    internal const string LibraryFileName = "snes9x_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/snes9x_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.snes9x.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".smc", ".sfc", ".swc", ".fig", ".bs", ".st"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
