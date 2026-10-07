namespace GWGUI.Emulation.Atari.Emulators.HatariB.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "hatarib";
    internal const string LibraryName = "HatariB";
    internal const string LibraryFile = "hatarib_libretro.dll";
    internal const string SourceUrl = "https://github.com/bbbradsmith/hatariB";
    internal const string InspectedRevision = "4fb7ebdfa14d5de7df3076143f255d7d8bf0323c";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.HatariB, Id, LibraryName, LibraryFile, SourceUrl, InspectedRevision,
        MachineModel.St, MachineModel.Stf, MachineModel.Stfm, MachineModel.MegaSt,
        MachineModel.Ste, MachineModel.MegaSte, MachineModel.Tt, MachineModel.Falcon)
        with { DescriptionResourceKey = StModelConstants.EmulatorDescriptionResourceKey };
}
