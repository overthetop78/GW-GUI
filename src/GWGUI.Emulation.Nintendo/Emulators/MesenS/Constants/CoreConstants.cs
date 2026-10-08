using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.MesenS.Constants;

internal static class CoreConstants
{
    internal const string Id = "mesen_s";
    internal const string DisplayName = "Mesen-S";
    internal const string LibraryName = "Mesen-S";
    internal const string LibraryFileName = "mesen-s_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mesen-s_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mesen_s.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".sfc", ".smc", ".fig", ".swc", ".gb", ".gbc", ".bs"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.Snes, ModelConstants.GameBoyColor }));
}
