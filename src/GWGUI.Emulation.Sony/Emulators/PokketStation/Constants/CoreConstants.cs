using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;

internal static class CoreConstants
{
    internal const string Id = "pokketstation";
    internal const string DisplayName = "Pokket";
    internal const string LibraryName = "Pokket";
    internal const string PreviousLibraryName = "pokketstation";
    internal const string ArchiveLibraryFileName = "pokket_libretro.dll";
    internal const string LibraryFileName = "pokketstation_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/pokket_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.pokketstation.Description";
    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName,
        GWGUI.Emulation.Sony.Common.Machines.PocketStation.Constants.MediaConstants.FlashImageExtensions,
        new(new Uri(OfficialPackageUrl), ArchiveLibraryFileName, LibraryFileName, DisplayName),
        Dictionaries.OptionCatalog.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PocketStation }),
        RequiresExternalFirmware: true,
        ControllerTypes: [PeripheralCategory.Joystick, PeripheralCategory.None],
        PreviousLibraryNames: [PreviousLibraryName],
        CreateContentLoadPlan: Functions.LinkFunctions.CreateLoadPlan);
}
