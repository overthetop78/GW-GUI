namespace GWGUI.Emulation.Mattel.Emulators.FreeIntv.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "freeintv";
    internal const string LibraryName = "FreeIntv";
    internal const string DllName = "freeintv_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/FreeIntv";
    internal const string InspectedRevision = "ef3e0fe322bec62a7f916c0bb0834c08c348d0b4";
    internal const string DescriptionResourceKey = "Emulation.Emulator.freeintv.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.FreeIntv, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.Intellivision)
        with { DescriptionResourceKey = DescriptionResourceKey };
}
