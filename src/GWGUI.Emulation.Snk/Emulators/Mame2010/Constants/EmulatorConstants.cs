namespace GWGUI.Emulation.Snk.Emulators.Mame2010.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "mame2010";
    internal const string LibraryName = "MAME 2010";
    internal const string DllName = "mame2010_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/mame2010-libretro";
    internal const string InspectedRevision = "dff8aadd1c3f38215af3955746d6e19abe0ddcea";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mame2010_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mame2010.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.Mame2010, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoMvs) with { DescriptionResourceKey = DescriptionResourceKey };
}
