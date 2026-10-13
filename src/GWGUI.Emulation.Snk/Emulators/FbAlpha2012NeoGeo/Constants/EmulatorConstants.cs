using GWGUI.Emulation.Snk.Emulators.Common.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Contracts;
namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012NeoGeo.Constants;
internal static class EmulatorConstants
{
    internal const string Id = "fbalpha2012_neogeo";
    internal const string LibraryName = "FB Alpha 2012 Neo Geo";
    internal const string DllName = "fbalpha2012_neogeo_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/fbalpha2012_neogeo";
    internal const string InspectedRevision = "e092097037d494c50d17c5144a2758d68538061b";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/fbalpha2012_neogeo_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.fbalpha2012_neogeo.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.FbAlpha2012NeoGeo, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoAes, MachineModel.NeoGeoMvs)with
    {
        DescriptionResourceKey = DescriptionResourceKey
    };
}