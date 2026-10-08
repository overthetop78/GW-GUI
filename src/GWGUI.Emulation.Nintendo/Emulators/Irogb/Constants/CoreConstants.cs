using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Irogb.Constants;

internal static class CoreConstants
{
    internal const string Id = "irogb";
    internal const string DisplayName = "IroGB";
    internal const string LibraryName = "IroGB";
    internal const string LibraryFileName = "irogb_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/irogb_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.irogb.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gb", ".gbc", ".zip"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }));
}
