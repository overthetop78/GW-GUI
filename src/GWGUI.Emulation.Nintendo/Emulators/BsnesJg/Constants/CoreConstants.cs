using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BsnesJg.Constants;

internal static class CoreConstants
{
    internal const string Id = "bsnes_jg";
    internal const string DisplayName = "bsnes-jg";
    internal const string LibraryName = "bsnes-jg";
    internal const string LibraryFileName = "bsnes-jg_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/bsnes-jg_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.bsnes_jg.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".smc", ".sfc", ".gb", ".gbc", ".bs", ".st"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
