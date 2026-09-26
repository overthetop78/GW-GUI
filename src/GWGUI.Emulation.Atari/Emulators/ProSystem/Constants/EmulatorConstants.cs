namespace GWGUI.Emulation.Atari.Emulators.ProSystem.Constants;

internal static class EmulatorConstants
{
    internal static readonly IReadOnlySet<string> CartridgeExtensions =
        new HashSet<string>(["a78", "bin", "cdf"], StringComparer.OrdinalIgnoreCase);

    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.ProSystem, "prosystem", "ProSystem", "prosystem_libretro.dll",
        "https://github.com/libretro/prosystem-libretro",
        "363b6dfbd3e240762e022c2b4897b4fe55722be3", MachineModel.Atari7800);
}
