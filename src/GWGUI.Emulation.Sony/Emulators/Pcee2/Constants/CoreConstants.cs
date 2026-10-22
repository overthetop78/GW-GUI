using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Dictionaries;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Constants;

internal static class CoreConstants
{
    internal const string Id = "pcee2";
    internal const string DisplayName = "PCEE2";
    internal const string LibraryName = "PCEE2";
    internal const string LibraryFileName = "pcee2_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/pcee2_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.pcee2.Description";
    internal const string InitializationFailureDiagnostic = "VMManager::Initialize() failed.";
    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName, FirmwareConstants.ContentExtensions,
        new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionCatalog.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PlayStation2 }),
        RequiresExternalFirmware: true,
        ControllerTypes: [PeripheralCategory.Joystick, PeripheralCategory.None, PeripheralCategory.GunCon2],
        StartupFailureDiagnostics: [InitializationFailureDiagnostic],
        FeedbackControllerTypes: [PeripheralCategory.Joystick],
        CreateControllerPorts: ControllerConfigurationFunctions.CreatePorts,
        FirmwarePersistence: FirmwareConstants.Persistence,
        PrepareSystemFiles: NetworkFunctions.Prepare);
}
