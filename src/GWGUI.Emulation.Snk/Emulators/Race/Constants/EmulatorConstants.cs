namespace GWGUI.Emulation.Snk.Emulators.Race.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "race";
    internal const string LibraryName = "RACE";
    internal const string DllName = "race_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/RACE";
    internal const string InspectedRevision = "c7810dd7f172827bfa2004813bc000b13786636b";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/race_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.race.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.Race, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.NeoGeoPocket, MachineModel.NeoGeoPocketColor) with { DescriptionResourceKey = DescriptionResourceKey };
}
