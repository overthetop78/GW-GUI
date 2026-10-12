namespace GWGUI.Emulation.Coleco.Emulators.JollyCV.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.JollyCV, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.ColecoVision)
        with { DescriptionResourceKey = DescriptionResourceKey };
    internal const string Id = "jollycv";
    internal const string LibraryName = "JollyCV";
    internal const string DllName = "jollycv_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/jollycv";
    internal const string InspectedRevision = "2790d59c7194d29e83f8603ffd7e9da17f11575e";
    internal const string DescriptionResourceKey = "Emulation.Emulator.jollycv.Description";
}
