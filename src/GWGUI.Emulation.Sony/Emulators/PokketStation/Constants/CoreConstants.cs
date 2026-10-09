using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;

internal static class CoreConstants
{
    internal const string Id = "pokketstation";
    internal const string DisplayName = "pokketstation";
    internal const string LibraryName = "pokketstation";
    internal const string LibraryFileName = "pokketstation_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/pokketstation_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.pokketstation.Description";
    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName,
        GWGUI.Emulation.Sony.Common.Machines.PocketStation.Constants.MediaConstants.FlashImageExtensions,
        new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        [], FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PocketStation }),
        RequiresExternalFirmware: true,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);
}
