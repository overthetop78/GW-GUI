using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Dictionaries;

namespace GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants;

internal static class CoreConstants
{
    internal const string Id = "mednafen_psx_hw";
    internal const string DisplayName = "Beetle PSX HW";
    internal const string LibraryName = "Beetle PSX HW";
    internal const string LibraryFileName = "mednafen_psx_hw_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_psx_hw_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mednafen_psx_hw.Description";
    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName, FirmwareConstants.ContentExtensions,
        new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionCatalog.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PlayStation }),
        RequiresExternalFirmware: false,
        ControllerTypes: [PeripheralCategory.Joystick, PeripheralCategory.DualShock,
            PeripheralCategory.AnalogController, PeripheralCategory.AnalogJoystick,
            PeripheralCategory.GunCon, PeripheralCategory.Justifier, PeripheralCategory.Mouse,
            PeripheralCategory.NeGcon, PeripheralCategory.NeGconRumble, PeripheralCategory.None],
        FeedbackControllerTypes: [PeripheralCategory.DualShock]);
}
