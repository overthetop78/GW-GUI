using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;

internal static class CoreConstants
{
    internal const string Id = "dolphin";
    internal const string DisplayName = "Dolphin";
    internal const string LibraryName = "dolphin-emu";
    internal const string LibraryFileName = "dolphin_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/dolphin_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.dolphin.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".elf", ".dol", ".gcm", ".iso", ".tgc", ".wbfs", ".ciso", ".gcz", ".wad", ".wia", ".rvz", ".m3u"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameCube, ModelConstants.Wii }));
}
