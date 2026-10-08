using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.VbaNext.Constants;

internal static class CoreConstants
{
    internal const string Id = "vba_next";
    internal const string DisplayName = "VBA Next";
    internal const string LibraryName = "VBA Next";
    internal const string LibraryFileName = "vba_next_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vba_next_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.vba_next.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gba"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance }));
}
