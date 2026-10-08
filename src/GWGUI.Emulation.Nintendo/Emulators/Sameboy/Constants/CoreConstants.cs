using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Sameboy.Constants;

internal static class CoreConstants
{
    internal const string Id = "sameboy";
    internal const string DisplayName = "SameBoy";
    internal const string LibraryName = "SameBoy";
    internal const string LibraryFileName = "sameboy_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/sameboy_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.sameboy.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gb", ".gbc"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }));
}
