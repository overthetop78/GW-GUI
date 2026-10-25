namespace GWGUI.Emulation.Watara.Common.Machines.Supervision.Dictionaries;

internal static class HardwareModelCatalog
{
    // Machine photo and isolated printed brand: Letzte*Spieler; transparency by Alexis Jazz; resized/cropped.
    // https://commons.wikimedia.org/wiki/File:Watara_Supervision_game_console.png ; CC BY-SA 3.0 https://creativecommons.org/licenses/by-sa/3.0/
    // Physical memory map and clock: MAME src/mame/svision/svision.cpp.
    internal static IReadOnlyList<HardwareModel> All { get; } = [new(MachineModel.Supervision, "WDC W65C02", 4_000_000, 8_192, 8_192)];
}