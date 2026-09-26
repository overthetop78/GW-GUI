namespace GWGUI.Emulation.Atari.Emulators.Stella.Constants;

internal static class EmulatorConstants
{
    internal const bool SupportsCartridgeRegion = true;

    internal static readonly IReadOnlySet<string> CartridgeExtensions =
        new HashSet<string>(["a26", "bin"], StringComparer.OrdinalIgnoreCase);

    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Stella, "stella2023", "Stella 2023", "stella2023_libretro.dll",
        "https://github.com/libretro/stella",
        "878a9c8d5f03ef0b7cd190b5713d6bf31c48df38", MachineModel.Atari2600);
}
