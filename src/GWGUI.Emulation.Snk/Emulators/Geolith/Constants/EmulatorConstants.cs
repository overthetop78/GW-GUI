namespace GWGUI.Emulation.Snk.Emulators.Geolith.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "geolith";
    internal const string LibraryName = "Geolith";
    internal const string DllName = "geolith_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/geolith-libretro";
    internal const string InspectedRevision = "194024931935eff2092e36fc4f8e53e62ed11097";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/geolith_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.geolith.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.Geolith, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoAes, MachineModel.NeoGeoMvs, MachineModel.NeoGeoCd, MachineModel.NeoGeoCdz) with { DescriptionResourceKey = DescriptionResourceKey };
}
