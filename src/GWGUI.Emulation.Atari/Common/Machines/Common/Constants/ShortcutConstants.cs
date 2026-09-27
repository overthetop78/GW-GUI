using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Constants;


internal static class ShortcutConstants
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
