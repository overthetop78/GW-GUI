using EmulatorCatalogFunctions = GWGUI.Emulation.Snk.Common.Functions.EmulatorCatalogFunctions;
using GWGUI.Emulation.Snk.Emulators.Common.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Contracts;
namespace GWGUI.Emulation.Snk.Emulators.Mame2015.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "mame2015";
    internal const string LibraryName = "MAME 2015";
    internal const string DllName = "mame2015_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/mame2015-libretro";
    internal const string InspectedRevision = "7adbf440d5f554097f58f8c6ebb673adc4320a31";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mame2015_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mame2015.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.Mame2015, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoMvs) with { DescriptionResourceKey = DescriptionResourceKey };
}
