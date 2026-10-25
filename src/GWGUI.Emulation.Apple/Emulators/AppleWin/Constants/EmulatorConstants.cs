using EmulatorCatalogFunctions = GWGUI.Emulation.Apple.Common.Functions.EmulatorCatalogFunctions;
using GWGUI.Emulation.Apple.Emulators.Common.Contracts;
using GWGUI.Emulation.Apple.Emulators.Common.Functions;
namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.AppleWin, Id, LibraryName, DllName, SourceUri, InspectedRevision,
        MachineModel.AppleII, MachineModel.AppleIIPlus, MachineModel.AppleIIJPlus,
        MachineModel.AppleIIe, MachineModel.AppleIIeEnhanced)
        with { DescriptionResourceKey = DescriptionResourceKey };
    internal const string Id = "applewin";
    internal const string LibraryName = "AppleWin";
    internal const string DllName = "applewin_libretro.dll";
    internal const string SourceUri = "https://github.com/audetto/AppleWin";
    internal const string InspectedRevision = "7062ae417896da6050e2e39030215062bb4672ee";
    internal const string DescriptionResourceKey = "Emulation.Emulator.applewin.Description";

    /// <summary>Identité du format brut utilisé par le contrôleur SmartPort.</summary>
    internal const string HardDiskFormatId = "applewin-hdv";
    internal const string HardDiskExtension = ".hdv";
    internal const string HardDiskInterface = "SmartPort";
    internal const int HardDiskSectorBytes = 512;
    /// <summary>Limite de blocs annoncée par Harddisk.h, convertie en octets sans arrondi supplémentaire.</summary>
    internal const long HardDiskMaximumBytes = 0x007FFFFFL * HardDiskSectorBytes;
    internal const long HardDiskDefaultBytes = 32L * 1024 * 1024;
}
