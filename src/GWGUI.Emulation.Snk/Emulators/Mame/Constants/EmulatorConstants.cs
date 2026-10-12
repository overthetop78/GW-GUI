namespace GWGUI.Emulation.Snk.Emulators.Mame.Constants;
internal static class EmulatorConstants
{
    internal const string Id = "mame";
    internal const string LibraryName = "MAME";
    internal const string DllName = "mame_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/mame";
    internal const string InspectedRevision = "5f5a07748ce5c239c4118463ce8e6473743d4859";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mame_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mame.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Mame,
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
    ) with
    {
        DescriptionResourceKey = DescriptionResourceKey
    };
}