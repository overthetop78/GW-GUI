using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Citra2018.Constants;

internal static class CoreConstants
{
    internal const string Id = "citra2018";
    internal const string DisplayName = "Citra 2018";
    internal const string LibraryName = "Citra2018";
    internal const string LibraryFileName = "citra2018_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/citra2018_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.citra2018.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".3ds", ".3dsx", ".cia", ".elf"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nintendo3Ds }));
}
