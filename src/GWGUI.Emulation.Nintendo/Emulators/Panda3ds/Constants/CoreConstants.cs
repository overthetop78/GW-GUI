using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Panda3ds.Constants;

internal static class CoreConstants
{
    internal const string Id = "panda3ds";
    internal const string DisplayName = "Panda3DS";
    internal const string LibraryName = "Panda3DS";
    internal const string LibraryFileName = "panda3ds_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/panda3ds_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.panda3ds.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".3ds", ".3dsx", ".elf", ".axf", ".cci", ".cxi", ".app"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nintendo3Ds }));
}
