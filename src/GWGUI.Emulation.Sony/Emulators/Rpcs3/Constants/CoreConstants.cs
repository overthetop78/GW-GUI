using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Rpcs3.Dictionaries;

namespace GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants;

internal static class CoreConstants
{
    internal const string Id = "rpcs3";
    internal const string DisplayName = "RPCS3";
    internal const string LibraryName = "RPCS3";
    internal const string LibraryFileName = "rpcs3_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/rpcs3_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.rpcs3.Description";
    internal const int FirmwareInstallationTimeoutMinutes = 10;
    internal const int CacheShutdownTimeoutMinutes = 5;
    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName, FirmwareConstants.ContentExtensions,
        new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionCatalog.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PlayStation3 }),
        RequiresExternalFirmware: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None],
        StartupTimeout: TimeSpan.FromMinutes(FirmwareInstallationTimeoutMinutes),
        ShutdownTimeout: TimeSpan.FromMinutes(CacheShutdownTimeoutMinutes));
}
