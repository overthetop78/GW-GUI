using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Fixgb.Constants;

internal static class CoreConstants
{
    internal const string Id = "fixgb";
    internal const string DisplayName = "fixGB";
    internal const string LibraryName = "fixGB";
    internal const string LibraryFileName = "fixgb_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/fixgb_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.fixgb.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gb", ".gbc", ".gbs"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }));
}
