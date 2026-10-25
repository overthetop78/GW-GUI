using EmulatorCatalogFunctions = GWGUI.Emulation.Apple.Common.Functions.EmulatorCatalogFunctions;
using GWGUI.Emulation.Apple.Emulators.Common.Contracts;
using GWGUI.Emulation.Apple.Emulators.Common.Functions;
namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.MiniVmac, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.MacintoshII)
        with { DescriptionResourceKey = DescriptionResourceKey };
    internal const int MinimumRomBytes = 256 * 1024;
    internal const uint KeyboardDevice = 259;
    internal const long MainMemoryBytes = 8 * 1024 * 1024;
    internal const string PreferredRomFileName = "MacIIx.ROM";
    internal const string RomFileName = "MacII.ROM";
    internal const string RomMd5 = "66223be1497460f1e60885eeb35e03cc";
    internal const string Id = "minivmac";
    internal const string LibraryName = "Mini vMac";
    internal const string DllName = "minivmac_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/libretro-minivmac";
    internal const string InspectedRevision = "babdf7b53d361a7225858b68a8a64efe8b06f5e5";
    internal const string DescriptionResourceKey = "Emulation.Emulator.minivmac.Description";

    /// <summary>Identité du disque brut présenté au cœur.</summary>
    internal const string HardDiskFormatId = "minivmac-raw";
    internal const string HardDiskExtension = ".img";
    internal const string HardDiskInterface = "Macintosh";
    internal const int HardDiskSectorBytes = 512;
    /// <summary>Limite en octets du compteur 32 bits de vSonyGetSize, alignée sur un secteur complet.</summary>
    internal const long HardDiskMaximumBytes = uint.MaxValue / HardDiskSectorBytes * (long)HardDiskSectorBytes;
    internal const long HardDiskDefaultBytes = 32L * 1024 * 1024;
}
