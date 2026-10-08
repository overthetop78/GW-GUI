using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Citra.Constants;

internal static class CoreConstants
{
    internal const string Id = "citra";
    internal const string DisplayName = "Citra";
    internal const string LibraryName = "Citra";
    internal const string LibraryFileName = "citra_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/citra_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.citra.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".3ds", ".3dsx", ".cia", ".elf"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nintendo3Ds }));
}
