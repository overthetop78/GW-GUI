using EmulatorCatalogFunctions = GWGUI.Emulation.Philips.Common.Functions.EmulatorCatalogFunctions;
using GWGUI.Emulation.Philips.Emulators.Common.Contracts;
using GWGUI.Emulation.Philips.Emulators.Common.Functions;
namespace GWGUI.Emulation.Philips.Emulators.SameCdi.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "same_cdi";
    internal const string LibraryName = "SAME_CDI";
    internal const string DllName = "same_cdi_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/same_cdi";
    internal const string InspectedRevision = "ff9bb99a50d518aef6d2add1c49b7c44447e3e78";
    internal const string DescriptionResourceKey = "Emulation.Emulator.same_cdi.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.SameCdi, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.PhilipsCdi)
        with { DescriptionResourceKey = DescriptionResourceKey };
}
