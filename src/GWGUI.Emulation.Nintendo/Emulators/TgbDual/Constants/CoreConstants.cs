using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.TgbDual.Constants;

internal static class CoreConstants
{
    internal const string Id = "tgbdual";
    internal const string DisplayName = "TGB Dual";
    internal const string LibraryName = "TGB Dual";
    internal const string LibraryFileName = "tgbdual_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/tgbdual_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.tgbdual.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gb", ".dmg", ".gbc", ".cgb", ".sgb"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }));
}
