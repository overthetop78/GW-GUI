using GWGUI.Emulation.Acorn.Emulators.Common.Functions;
using GWGUI.Emulation.Acorn.Emulators.Common.Contracts;
namespace GWGUI.Emulation.Acorn.Emulators.B2.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "b2";
    internal const string LibraryName = "b2";
    internal const string DllName = "b2_libretro.dll";
    internal const string SourceUri = "https://github.com/zoltanvb/b2-libretro";
    internal const string InspectedRevision = "9793f6a8040c416d147003cada29363b955b54e7";
    internal const string DescriptionResourceKey = "Emulation.Emulator.b2.Description";

    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.B2, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.BbcMicroModelB, MachineModel.BbcMaster128)
        with { DescriptionResourceKey = DescriptionResourceKey };
}
