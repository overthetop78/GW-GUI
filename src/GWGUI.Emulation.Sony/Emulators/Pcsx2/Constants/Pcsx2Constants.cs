using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

internal static class Pcsx2Constants
{
    internal const string LibraryFileName = "pcsx2_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/pcsx2_libretro.dll.zip";
    internal static CoreReleaseSettings ReleaseSettings { get; } = new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName);

    internal const string Id = "pcsx2";
    internal const string DisplayName = "PCSX2";
    internal const string LibraryName = "PCSX2";
    internal const string CurrentLibraryName = "LRPS2";
    internal const string DescriptionResourceKey = "Emulation.Emulator.pcsx2.Description";

    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName, FirmwareConstants.ContentExtensions, ReleaseSettings,
        Dictionaries.OptionCatalog.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PlayStation2 }),
        RequiresExternalFirmware: true,
        ControllerTypes: [PeripheralCategory.Joystick, PeripheralCategory.Keyboard, PeripheralCategory.Mouse, PeripheralCategory.KeyboardAndMouse, PeripheralCategory.None],
        FeedbackControllerTypes: [PeripheralCategory.Joystick],
        PreviousLibraryNames: [CurrentLibraryName],
        FirmwarePersistence: FirmwareConstants.Persistence,
        PrepareRuntimeConfiguration: FileSystemFunctions.ConfigureRuntime,
        ResolveContentMedia: HardDiskRuntimeFunctions.ResolveContentMedia,
        ResolveNativeSystemDirectory: FileSystemFunctions.SystemDirectory,
        ResolveVirtualPath: FileSystemFunctions.Resolve,
        FullContentPathsRequired: Pcsx2Exceptions.FullContentPathsRequired);
}
