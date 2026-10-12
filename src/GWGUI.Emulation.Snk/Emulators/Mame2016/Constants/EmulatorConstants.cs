namespace GWGUI.Emulation.Snk.Emulators.Mame2016.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "mame2016";
    internal const string LibraryName = "MAME 2016";
    internal const string DllName = "mame2016_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/mame2016-libretro";
    internal const string InspectedRevision = "ae07c2f88ff2482ba9f50ffc8c9e7e6fbfe97d0a";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mame2016_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mame2016.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Mame2016,
        Id,
        LibraryName,
        DllName,
        new Uri(ArchiveUri, UriKind.Absolute),
        SourceUri,
        InspectedRevision,
        MachineModel.NeoGeoAes,
        MachineModel.NeoGeoMvs,
        MachineModel.NeoGeoPocket,
        MachineModel.NeoGeoPocketColor,
        MachineModel.NeoGeoCd,
        MachineModel.NeoGeoCdz
    )with
    {
        DescriptionResourceKey = DescriptionResourceKey
    };
}
