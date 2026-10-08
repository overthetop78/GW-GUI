using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;

internal static class CoreConstants
{
    internal const string Id = "swanstation";
    internal const string DisplayName = "SwanStation";
    internal const string LibraryName = "SwanStation";
    internal const string LibraryFileName = "swanstation_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/swanstation_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.swanstation.Description";
    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName, FirmwareConstants.ContentExtensions,
        new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        [], FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PlayStation }),
        RequiresExternalFirmware: true);
}