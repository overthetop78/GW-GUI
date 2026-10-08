using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Fixnes.Constants;

internal static class CoreConstants
{
    internal const string Id = "fixnes";
    internal const string DisplayName = "fixNES";
    internal const string LibraryName = "fixNES";
    internal const string LibraryFileName = "fixnes_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/fixnes_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.fixnes.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".nes", ".fds", ".qd", ".nsf"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nes, ModelConstants.FamicomDisk }));
}
