namespace GWGUI.Emulation.Snk.Emulators.FbNeo.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "fbneo";
    internal const string LibraryName = "FinalBurn Neo";
    internal const string DllName = "fbneo_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/FBNeo";
    internal const string InspectedRevision = "6e7b7ea6c61a594fbaf2a2071dff326290fedcd1";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/fbneo_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.fbneo.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.FbNeo, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoAes, MachineModel.NeoGeoMvs, MachineModel.NeoGeoCdz, MachineModel.NeoGeoPocket, MachineModel.NeoGeoPocketColor) with { DescriptionResourceKey = DescriptionResourceKey };
}
