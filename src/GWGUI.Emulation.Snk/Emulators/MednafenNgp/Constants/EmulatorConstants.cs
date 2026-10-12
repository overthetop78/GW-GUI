namespace GWGUI.Emulation.Snk.Emulators.MednafenNgp.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "mednafen_ngp";
    internal const string LibraryName = "Beetle NeoPop";
    internal const string DllName = "mednafen_ngp_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/beetle-ngp-libretro";
    internal const string InspectedRevision = "a50d5ac288a81f2104ddf43195a4efdd15c72227";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_ngp_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mednafen_ngp.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.MednafenNgp, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoPocket, MachineModel.NeoGeoPocketColor)with
    {
        DescriptionResourceKey = DescriptionResourceKey
    };
}
