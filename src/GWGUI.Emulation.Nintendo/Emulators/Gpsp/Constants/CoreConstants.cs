using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Gpsp.Constants;

internal static class CoreConstants
{
    internal const string Id = "gpsp";
    internal const string DisplayName = "gpSP";
    internal const string LibraryName = "gpSP";
    internal const string LibraryFileName = "gpsp_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/gpsp_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.gpsp.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gba", ".bin", ".agb", ".gbz", ".u1"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance }));
}
