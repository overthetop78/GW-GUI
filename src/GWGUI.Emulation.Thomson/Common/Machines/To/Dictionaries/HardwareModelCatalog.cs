namespace GWGUI.Emulation.Thomson.Common.Machines.To.Dictionaries;

internal static class HardwareModelCatalog
{
    // TO7: Thomas Conté, https://commons.wikimedia.org/wiki/File:Thomson_TO7_(Thomas_Conté)_(cropped).jpg (CC BY-SA 2.0).
    // TO7/70: Superandi73, https://commons.wikimedia.org/wiki/File:Thomson_TO7-70.jpg ; license on the file page.
    // TO8: Association WDA, https://wda-fr.org/forum/viewtopic.php?t=552 ; photo THOMSON_TO8_-_DESSUS_HD.jpg.
    // TO8D: Xavier Caré / Wikimedia Commons / CC-BY-SA, https://commons.wikimedia.org/wiki/File:Thomson_TO8D.jpg ; background removed and resized.
    // TO9: Jose Antonio Ortigueira Amor, https://retroordenadoresorty.blogspot.com/p/thomson.html ; photo TO9 F2b.
    // TO9+: https://www.system-cfg.com/detailcollection.php?ident=178 ; photo thomson_to9p_1.jpg.
    // All images: original photographs, background removed along each actual silhouette and resized without altering the hardware.
    // Physical base RAM including video RAM, without optional expansions; MAME thomson.cpp hardware documentation.
    internal static IReadOnlyList<HardwareModel> All { get; } = [
        new(MachineModel.To7, "Motorola MC6809E", 1_000_000, 24_576),
        new(MachineModel.To770, "Motorola MC6809E", 1_000_000, 65_536),
        new(MachineModel.To8, "Motorola MC6809E", 1_000_000, 262_144),
        new(MachineModel.To8D, "Motorola MC6809E", 1_000_000, 262_144),
        new(MachineModel.To9, "Motorola MC6809E", 1_000_000, 131_072),
        new(MachineModel.To9Plus, "Motorola MC6809E", 1_000_000, 524_288)
    ];
}
