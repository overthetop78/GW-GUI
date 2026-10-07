namespace GWGUI.Emulation.Atari.Emulators.Hatari2014.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "hatari2014";
    internal const string LibraryName = "Hatari2014";
    internal const string LibraryFile = "hatari2014_libretro.dll";
    internal const string SourceUrl = "https://github.com/libretro/hatari/tree/hitari2014-mercurial";
    internal const string InspectedRevision = "ab55c3ed0e620c91e7f059a6d3fbef7acf9bfca8";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Hatari2014, Id, LibraryName, LibraryFile, SourceUrl, InspectedRevision,
        MachineModel.St, MachineModel.Stf, MachineModel.Stfm, MachineModel.MegaSt,
        MachineModel.Ste, MachineModel.MegaSte, MachineModel.Tt, MachineModel.Falcon)
        with { DescriptionResourceKey = StModelConstants.EmulatorDescriptionResourceKey };
}
