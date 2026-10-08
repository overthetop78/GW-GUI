using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Skyemu.Constants;

internal static class CoreConstants
{
    internal const string Id = "skyemu";
    internal const string DisplayName = "SkyEmu";
    internal const string LibraryName = "SkyEmu";
    internal const string LibraryFileName = "skyemu_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/skyemu_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.skyemu.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gb", ".gbc", ".gba", ".nds"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs, ModelConstants.GameBoy, ModelConstants.GameBoyAdvance, ModelConstants.GameBoyColor }));
}
