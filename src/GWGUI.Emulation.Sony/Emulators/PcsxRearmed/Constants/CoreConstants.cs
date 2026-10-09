using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Dictionaries;

namespace GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants;

internal static class CoreConstants
{
    internal const string Id = "pcsx_rearmed";
    internal const string DisplayName = "PCSX-ReARMed";
    internal const string LibraryName = "PCSX-ReARMed";
    internal const string LibraryFileName = "pcsx_rearmed_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/pcsx_rearmed_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.pcsx_rearmed.Description";
    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName, FirmwareConstants.ContentExtensions,
        new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionCatalog.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PlayStation }),
        RequiresExternalFirmware: false,
        ControllerTypes: [ControllerType.Joystick, ControllerType.DualShock,
            ControllerType.AnalogJoystick, ControllerType.GunCon,
            ControllerType.Justifier, ControllerType.Mouse, ControllerType.NeGcon,
            ControllerType.None]);
}
