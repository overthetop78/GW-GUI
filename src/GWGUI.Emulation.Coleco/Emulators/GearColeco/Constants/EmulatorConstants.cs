namespace GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.GearColeco, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.ColecoVision, MachineModel.Adam)
        with { DescriptionResourceKey = DescriptionResourceKey };
    internal const string Id = "gearcoleco";
    internal const string LibraryName = "Gearcoleco";
    internal const string DllName = "gearcoleco_libretro.dll";
    internal const string SourceUri = "https://github.com/drhelius/Gearcoleco";
    internal const string InspectedRevision = "3161e3f63722a8ad3c0c7a934e17ef3d72ac93a3";
    internal const string DescriptionResourceKey = "Emulation.Emulator.gearcoleco.Description";
}
