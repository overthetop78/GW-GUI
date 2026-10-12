namespace GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Constants;
internal static class EmulatorConstants
{
    internal const string Id = "mame2003_plus";
    internal const string LibraryName = "MAME 2003-Plus";
    internal const string DllName = "mame2003_plus_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/mame2003-plus-libretro";
    internal const string InspectedRevision = "e05caba0aa33cae580d104c45c6caa6406617e3b";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mame2003_plus_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mame2003_plus.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.Mame2003Plus, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoMvs, MachineModel.NeoGeoAes)with
    {
        DescriptionResourceKey = DescriptionResourceKey
    };
}