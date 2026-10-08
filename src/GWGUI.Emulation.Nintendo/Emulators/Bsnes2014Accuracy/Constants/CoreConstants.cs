using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Bsnes2014Accuracy.Constants;

internal static class CoreConstants
{
    internal const string Id = "bsnes2014_accuracy";
    internal const string DisplayName = "bsnes2014 Accuracy";
    internal const string LibraryName = "bsnes2014";
    internal const string LibraryFileName = "bsnes2014_accuracy_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/bsnes2014_accuracy_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.bsnes2014_accuracy.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".sfc", ".smc", ".bml"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
