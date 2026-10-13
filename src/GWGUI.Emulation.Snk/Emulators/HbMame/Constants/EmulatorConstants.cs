using GWGUI.Emulation.Snk.Emulators.Common.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Contracts;
namespace GWGUI.Emulation.Snk.Emulators.HbMame.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "hbmame";
    internal const string LibraryName = "HBMAME";
    internal const string NativeLibraryName = "MAME";
    internal const string DllName = "hbmame_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/hbmame-libretro";
    internal const string InspectedRevision = "840eeef5ee0fd75dab62c90bf25789b898f3dba3";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/hbmame_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.hbmame.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.HbMame, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoMvs) with { DescriptionResourceKey = DescriptionResourceKey };
}
