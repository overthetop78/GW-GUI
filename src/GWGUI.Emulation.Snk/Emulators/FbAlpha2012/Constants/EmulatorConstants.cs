using GWGUI.Emulation.Snk.Emulators.Common.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Contracts;
namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Constants;
internal static class EmulatorConstants
{
    internal const string Id = "fbalpha2012";
    internal const string LibraryName = "FB Alpha 2012";
    internal const string DllName = "fbalpha2012_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/fbalpha2012";
    internal const string InspectedRevision = "0ce31536bef3162fe7e69ff5f555334ec4913cef";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/fbalpha2012_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.fbalpha2012.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.FbAlpha2012, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoAes, MachineModel.NeoGeoMvs)with
    {
        DescriptionResourceKey = DescriptionResourceKey
    };
}