using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Quicknes.Constants;

internal static class CoreConstants
{
    internal const string Id = "quicknes";
    internal const string DisplayName = "QuickNES";
    internal const string LibraryName = "QuickNES";
    internal const string LibraryFileName = "quicknes_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/quicknes_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.quicknes.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".nes"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nes }));
}
