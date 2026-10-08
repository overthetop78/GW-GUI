using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.TgbDual.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("tgbdual_gblink_enable", "Emulation.Option.tgbdual.tgbdual_gblink_enable", null, "", "disabled", "disabled", [new("disabled", "Emulation.Option.tgbdual.tgbdual_gblink_enable.Value.disabled"), new("enabled", "Emulation.Option.tgbdual.tgbdual_gblink_enable.Value.enabled")], Tab: EmulationMachineTab.Controllers),
        new("tgbdual_screen_placement", "Emulation.Option.tgbdual.tgbdual_screen_placement", null, "", "left-right", "left-right", [new("left-right", "Emulation.Option.tgbdual.tgbdual_screen_placement.Value.left-right"), new("top-down", "Emulation.Option.tgbdual.tgbdual_screen_placement.Value.top-down")], Tab: EmulationMachineTab.Video),
        new("tgbdual_switch_screens", "Emulation.Option.tgbdual.tgbdual_switch_screens", null, "", "normal", "normal", [new("normal", "Emulation.Option.tgbdual.tgbdual_switch_screens.Value.normal"), new("switched", "Emulation.Option.tgbdual.tgbdual_switch_screens.Value.switched")], Tab: EmulationMachineTab.Video),
        new("tgbdual_single_screen_mp", "Emulation.Option.tgbdual.tgbdual_single_screen_mp", null, "", "both players", "both players", [new("both players", "Emulation.Option.tgbdual.tgbdual_single_screen_mp.Value.both players"), new("player 1 only", "Emulation.Option.tgbdual.tgbdual_single_screen_mp.Value.player 1 only"), new("player 2 only", "Emulation.Option.tgbdual.tgbdual_single_screen_mp.Value.player 2 only")], Tab: EmulationMachineTab.Video),
        new("tgbdual_audio_output", "Emulation.Option.tgbdual.tgbdual_audio_output", null, "", "Game Boy #1", "Game Boy #1", [new("Game Boy #1", "Emulation.Option.tgbdual.tgbdual_audio_output.Value.Game Boy #1"), new("Game Boy #2", "Emulation.Option.tgbdual.tgbdual_audio_output.Value.Game Boy #2"), new("Both (mixed)", "Emulation.Option.tgbdual.tgbdual_audio_output.Value.Both (mixed)")], Tab: EmulationMachineTab.Audio)
    ];
}
