namespace GWGUI.Emulation.Bandai.Emulators.TamaLibretro.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.TamaLibretro, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.TamagotchiP1)
        with { DescriptionResourceKey = DescriptionResourceKey };
    internal const string Id = "tamalibretro";
    internal const string LibraryName = "TamaLIBretro";
    internal const string DllName = "tamalibretro_libretro.dll";
    internal const string SourceUri = "https://github.com/celerizer/tamalibretro";
    internal const string InspectedRevision = "ea8dd612eaab375a04f976149dc693cb5d208b27";
    internal const string DescriptionResourceKey = "Emulation.Emulator.tamalibretro.Description";
}
