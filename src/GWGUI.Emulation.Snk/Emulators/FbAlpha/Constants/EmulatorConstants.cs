namespace GWGUI.Emulation.Snk.Emulators.FbAlpha.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "fbalpha";
    internal const string LibraryName = "FB Alpha";
    internal const string DllName = "fbalpha_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/fbalpha";
    internal const string InspectedRevision = "d584c83";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/fbalpha_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.fbalpha.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.FbAlpha, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoAes, MachineModel.NeoGeoMvs, MachineModel.NeoGeoCdz) with { DescriptionResourceKey = DescriptionResourceKey };
}
