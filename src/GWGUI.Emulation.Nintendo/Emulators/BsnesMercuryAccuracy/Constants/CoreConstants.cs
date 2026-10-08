using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BsnesMercuryAccuracy.Constants;

internal static class CoreConstants
{
    internal const string Id = "bsnes_mercury_accuracy";
    internal const string DisplayName = "bsnes Mercury Accuracy";
    internal const string LibraryName = "bsnes-mercury";
    internal const string LibraryFileName = "bsnes_mercury_accuracy_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/bsnes_mercury_accuracy_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.bsnes_mercury_accuracy.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".sfc", ".smc", ".bml"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
