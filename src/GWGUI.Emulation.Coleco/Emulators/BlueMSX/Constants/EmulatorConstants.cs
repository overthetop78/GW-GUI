using GWGUI.Emulation.Coleco.Emulators.Common.Contracts;
using GWGUI.Emulation.Coleco.Emulators.Common.Functions;
namespace GWGUI.Emulation.Coleco.Emulators.BlueMSX.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.BlueMSX, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.ColecoVision)
        with { DescriptionResourceKey = DescriptionResourceKey };
    internal const string Id = "bluemsx";
    internal const string LibraryName = "blueMSX";
    internal const string DllName = "bluemsx_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/blueMSX-libretro";
    internal const string InspectedRevision = "e3086eb5d36d77fa11704cf53dc176686e70127d";
    internal const string DescriptionResourceKey = "Emulation.Emulator.bluemsx.Description";
}
