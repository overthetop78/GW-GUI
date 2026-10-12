namespace GWGUI.Emulation.Snk.Emulators.NeoCd.Constants;
internal static class EmulatorConstants
{
    internal const string Id = "neocd";
    internal const string LibraryName = "NeoCD";
    internal const string DllName = "neocd_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/neocd_libretro";
    internal const string InspectedRevision = "b1e04c738cb48a1dae0574b8877f6a116d270ca1";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/neocd_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.neocd.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.NeoCd, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoCd, MachineModel.NeoGeoCdz)with
    {
        DescriptionResourceKey = DescriptionResourceKey
    };
}