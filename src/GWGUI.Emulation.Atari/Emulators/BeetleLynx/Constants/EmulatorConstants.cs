namespace GWGUI.Emulation.Atari.Emulators.BeetleLynx.Constants;

internal static class EmulatorConstants
{
    internal static readonly IReadOnlySet<string> CartridgeExtensions =
        new HashSet<string>(["lnx", "lyx", "bll", "o"], StringComparer.OrdinalIgnoreCase);

    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.BeetleLynx, "beetle-lynx", "Beetle Lynx", "mednafen_lynx_libretro.dll",
        "https://github.com/libretro/beetle-lynx-libretro",
        "fcdefcfb3c11d6d2e71be076a5d3df2e88ab73ed", MachineModel.Lynx);
}
