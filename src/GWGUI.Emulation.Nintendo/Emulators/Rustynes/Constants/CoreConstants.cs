using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Rustynes.Constants;

internal static class CoreConstants
{
    internal const string Id = "rustynes";
    internal const string DisplayName = "RustyNES";
    internal const string LibraryName = "RustyNES";
    internal const string LibraryFileName = "rustynes_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/rustynes_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.rustynes.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".nes", ".fds", ".unf", ".unif"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nes, ModelConstants.FamicomDisk }));
}
