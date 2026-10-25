using EmulatorCatalogFunctions = GWGUI.Emulation.Snk.Common.Functions.EmulatorCatalogFunctions;
using GWGUI.Emulation.Snk.Emulators.Common.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Contracts;
namespace GWGUI.Emulation.Snk.Emulators.Mame2003.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "mame2003";
    internal const string LibraryName = "MAME 2003 (0.78)";
    internal const string DllName = "mame2003_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/mame2003-libretro";
    internal const string InspectedRevision = "b9ac708f922386a0ae1f2379dbb31b60d7ec7b25";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mame2003_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mame2003.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.Mame2003, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoMvs) with { DescriptionResourceKey = DescriptionResourceKey };
}
