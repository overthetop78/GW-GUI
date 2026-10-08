using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Bsnes2014Performance.Constants;

internal static class CoreConstants
{
    internal const string Id = "bsnes2014_performance";
    internal const string DisplayName = "bsnes2014 Performance";
    internal const string LibraryName = "bsnes2014";
    internal const string LibraryFileName = "bsnes2014_performance_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/bsnes2014_performance_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.bsnes2014_performance.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".sfc", ".smc", ".bml"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Snes }));
}
