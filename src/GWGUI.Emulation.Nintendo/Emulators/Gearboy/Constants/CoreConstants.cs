using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Gearboy.Constants;

internal static class CoreConstants
{
    internal const string Id = "gearboy";
    internal const string DisplayName = "GearBoy";
    internal const string LibraryName = "Gearboy";
    internal const string LibraryFileName = "gearboy_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/gearboy_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.gearboy.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gb", ".dmg", ".gbc", ".cgb", ".sgb"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }));
}
