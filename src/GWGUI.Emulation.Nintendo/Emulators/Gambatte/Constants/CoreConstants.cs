using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Gambatte.Constants;

internal static class CoreConstants
{
    internal const string Id = "gambatte";
    internal const string DisplayName = "Gambatte";
    internal const string LibraryName = "Gambatte";
    internal const string LibraryFileName = "gambatte_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/gambatte_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.gambatte.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gb", ".gbc", ".dmg"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor }));
}
