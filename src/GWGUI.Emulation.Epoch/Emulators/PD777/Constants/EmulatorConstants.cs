using GWGUI.Emulation.Epoch.Emulators.Common.Contracts;
using GWGUI.Emulation.Epoch.Emulators.Common.Functions;
namespace GWGUI.Emulation.Epoch.Emulators.PD777.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "pd777";
    internal const string LibraryName = "PD777";
    internal const string DllName = "pd777_libretro.dll";
    internal const string SourceUri = "https://github.com/mittonk/PD777";
    internal const string InspectedRevision = "331af5f53cc0ffbab90f915681c8dfe3a4cda0a2";
    internal const string DescriptionResourceKey = "Emulation.Emulator.pd777.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.PD777, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.CassetteVision, MachineModel.CassetteVisionJr)
        with { DescriptionResourceKey = DescriptionResourceKey };
}
