namespace GWGUI.Emulation.Panasonic.Common.Machines.ThreeDO.Dictionaries;

internal static class HardwareModelCatalog
{
    // Machine photo: Evan-Amos; cutout by Gunnar.offel, public domain. https://commons.wikimedia.org/wiki/File:3DO-FZ-1-Console-FL.png
    // Panasonic word mark, PD-textlogo. https://commons.wikimedia.org/wiki/File:Panasonic_logo.svg ; both resized, no hardware alteration.
    // Opera pinned revision 5a09dc47c1edc7de599fa7302d4992beacb55349: opera_mem.h and CPU clock options.
    internal static IReadOnlyList<HardwareModel> All { get; } = [new(MachineModel.ThreeDO, "ARM60", 12_500_000, 2 * 1024 * 1024, 1024 * 1024, 32 * 1024)];
}
