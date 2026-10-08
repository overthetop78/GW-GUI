using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Cemu.Constants;

internal static class CoreConstants
{
    internal const string Id = "cemu";
    internal const string DisplayName = "Cemu";
    internal const string LibraryName = "Cemu";
    internal const string LibraryFileName = "cemu_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/cemu_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.cemu.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".wud", ".wux", ".wua", ".iso", ".rpx", ".elf", ".tmd"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.WiiU }));
}
