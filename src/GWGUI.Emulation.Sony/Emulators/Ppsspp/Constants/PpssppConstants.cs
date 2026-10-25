using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;

internal static class PpssppConstants
{
    internal const string LibraryFileName = "ppsspp_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/ppsspp_libretro.dll.zip";
    internal static CoreReleaseSettings ReleaseSettings { get; } = new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName);

    internal const string Id = "ppsspp";
    internal const string Psp1000Model = "psp_1000";
    internal const int ExpandedMemoryKib = 64 * 1024;
    internal const string DisplayName = "PPSSPP";
    internal const string LibraryName = "PPSSPP";
    internal const string DescriptionResourceKey = "Emulation.Emulator.ppsspp.Description";
    internal const string ElfExtension = ".elf";
    internal const string IsoExtension = ".iso";
    internal const string CsoExtension = ".cso";
    internal const string PrxExtension = ".prx";
    internal const string PbpExtension = ".pbp";
    internal const string ChdExtension = ".chd";
    internal static IReadOnlyList<string> ContentExtensions { get; } =
        [ElfExtension, IsoExtension, CsoExtension, PrxExtension, PbpExtension, ChdExtension];

    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName, ContentExtensions, ReleaseSettings,
        Dictionaries.OptionCatalog.All, [],
        new(Id, DisplayName, DescriptionResourceKey, new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Psp }),
        RequiresExternalFirmware: false,
        ControllerTypes: [PeripheralCategory.Joystick],
        ResolveSaveDirectory: StorageFunctions.ResolveSaveDirectory,
        ResolveSystemDirectory: SystemFilesFunctions.DirectoryForCore,
        FullContentPathsRequired: PpssppExceptions.FullContentPathsRequired,
        ResolveOptions: PpssppOptionFunctions.Options,
        IsInstallationComplete: SystemFilesFunctions.IsInstalled,
        PrepareInstallation: SystemFilesFunctions.InstallAsync);
}
