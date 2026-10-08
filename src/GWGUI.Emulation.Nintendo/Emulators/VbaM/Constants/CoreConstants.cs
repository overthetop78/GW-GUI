using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.VbaM.Constants;

internal static class CoreConstants
{
    internal const string Id = "vbam";
    internal const string DisplayName = "VBA-M";
    internal const string LibraryName = "VBA-M";
    internal const string LibraryFileName = "vbam_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vbam_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.vbam.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".dmg", ".gb", ".gbc", ".cgb", ".sgb", ".gba"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance }));
}
