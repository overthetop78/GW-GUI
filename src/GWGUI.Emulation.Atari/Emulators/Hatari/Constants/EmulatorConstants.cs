namespace GWGUI.Emulation.Atari.Emulators.Hatari.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Hatari, "hatari", "Hatari", "hatari_libretro.dll",
        "https://github.com/libretro/hatari",
        "24e7bd744f24f20b464385f365a3850c269bd140",
        MachineModel.St, MachineModel.Stf, MachineModel.Stfm, MachineModel.MegaSt,
        MachineModel.Ste, MachineModel.MegaSte, MachineModel.Tt, MachineModel.Falcon);
}
