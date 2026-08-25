using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariShortcutConstants
{
    internal const int MinimumMediaForSelection = 1;

    internal static readonly IReadOnlyList<string> CommonActions =
    [
        EmulationShortcutActions.ReleaseMouse,
        EmulationShortcutActions.PauseResume,
        EmulationShortcutActions.ToggleFullscreen,
        EmulationShortcutActions.Power,
        EmulationShortcutActions.SoftReset,
        EmulationShortcutActions.HardReset,
        EmulationShortcutActions.Screenshot,
        EmulationShortcutActions.ToggleMute,
        EmulationShortcutActions.FastForward
    ];
}
