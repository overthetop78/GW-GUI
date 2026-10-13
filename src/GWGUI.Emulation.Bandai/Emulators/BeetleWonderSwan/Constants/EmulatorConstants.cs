using GWGUI.Emulation.Bandai.Emulators.Common.Contracts;
using GWGUI.Emulation.Bandai.Emulators.Common.Functions;
namespace GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.BeetleWonderSwan, Id, LibraryName, DllName, SourceUri, InspectedRevision,
        MachineModel.WonderSwan, MachineModel.WonderSwanColor, MachineModel.SwanCrystal)
        with { DescriptionResourceKey = DescriptionResourceKey };
    internal const string Id = "mednafen_wswan";
    internal const string RawCartridgeExtension = "bin";
    internal const string LibraryName = "Beetle WonderSwan";
    internal const string DllName = "mednafen_wswan_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/beetle-wswan-libretro";
    internal const string InspectedRevision = "4b01295838ea89e3f1355bbe4cb5cf98aa6108cd";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mednafen_wswan.Description";
}
