using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.BlueMSX.Constants;

internal static class CoreConstants
{
    internal const string Id = "bluemsx";
    internal const string DisplayName = "blueMSX";
    internal const string LibraryName = "blueMSX";
    internal const string LibraryFileName = "bluemsx_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/bluemsx_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.bluemsx.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".rom", ".ri", ".mx1", ".mx2", ".dsk", ".col", ".sg", ".sc", ".sf", ".cas", ".m3u"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Sg1000, ModelConstants.Sc3000, ModelConstants.Sf7000 }), MachineOptions: MachineOptionConstants.All, PrepareSystem: Functions.SystemFilesFunctions.Prepare);
}
