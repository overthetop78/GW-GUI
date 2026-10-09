using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Dictionaries;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;

internal static class CoreConstants
{
    internal const string Id = "swanstation";
    internal const string DisplayName = "SwanStation";
    internal const string LibraryName = "SwanStation";
    internal const string LibraryFileName = "swanstation_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/swanstation_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.swanstation.Description";
    internal const int LightGunPortCount = 2;
    internal const int SteeringControllerPortCount = 4;
    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName, FirmwareConstants.ContentExtensions,
        new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionCatalog.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PlayStation }),
        RequiresExternalFirmware: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.DualShock,
            ControllerType.AnalogJoystick, ControllerType.GunCon, ControllerType.Mouse,
            ControllerType.NeGcon, ControllerType.NeGconRumble, ControllerType.None]);
}
