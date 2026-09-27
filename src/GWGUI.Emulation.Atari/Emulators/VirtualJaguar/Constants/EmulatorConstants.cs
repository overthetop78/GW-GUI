namespace GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Constants;

internal static class EmulatorConstants
{
    internal const bool SupportsCartridgeRegion = true;

    internal static readonly IReadOnlySet<string> CartridgeExtensions =
        new HashSet<string>(["j64", "jag", "rom", "abs", "cof", "bin", "prg"],
            StringComparer.OrdinalIgnoreCase);

    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.VirtualJaguar, "virtual-jaguar", "Virtual Jaguar", "virtualjaguar_libretro.dll",
        "https://github.com/libretro/virtualjaguar-libretro",
        "385c4d458538fd473c4bc8dc8dab4778897e8ac6", MachineModel.Jaguar, MachineModel.JaguarCd);
}
