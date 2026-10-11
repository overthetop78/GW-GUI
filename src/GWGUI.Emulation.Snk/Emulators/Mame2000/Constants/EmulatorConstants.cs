namespace GWGUI.Emulation.Snk.Emulators.Mame2000.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "mame2000";
    internal const string LibraryName = "MAME 2000";
    internal const string DllName = "mame2000_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/mame2000-libretro";
    internal const string InspectedRevision = "181b42e5da0d2f0c7a2e59244f23633ed594de2e";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mame2000_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mame2000.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.Mame2000, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoMvs) with { DescriptionResourceKey = DescriptionResourceKey };
}
