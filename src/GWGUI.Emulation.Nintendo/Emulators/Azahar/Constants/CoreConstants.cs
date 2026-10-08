using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Azahar.Constants;

internal static class CoreConstants
{
    internal const string Id = "azahar";
    internal const string DisplayName = "Azahar";
    internal const string LibraryName = "Azahar";
    internal const string LibraryFileName = "azahar_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/azahar_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.azahar.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".3ds", ".3dsx", ".z3dsx", ".elf", ".axf", ".cci", ".zcci", ".cxi", ".zcxi", ".app"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nintendo3Ds }));
}
