namespace GWGUI.Emulation.Atari.Emulators.Atari800.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Atari800, "atari800", "Atari800", "atari800_libretro.dll",
        "https://github.com/libretro/libretro-atari800",
        "cd721790a0aa0e0772810949abcf5bd699c15371",
        MachineModel.Atari400, MachineModel.Atari800, MachineModel.Atari800Xl,
        MachineModel.Atari130Xe, MachineModel.Xegs, MachineModel.XlXe, MachineModel.Atari5200);
}
