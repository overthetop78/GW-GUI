namespace GWGUI.Domain.Settings.Emulation;

public static class EmulationShortcutDefaults
{
    public const string ReleaseMouse = "release-mouse";
    public const string PauseResume = "pause-resume";
    public const string ToggleFullscreen = "toggle-fullscreen";
    public const string Power = "power";
    public const string SoftReset = "soft-reset";
    public const string HardReset = "hard-reset";
    public const string QuickSave = "quick-save";
    public const string QuickLoad = "quick-load";
    public const string Screenshot = "screenshot";
    public const string ToggleMute = "toggle-mute";
    public const string FastForward = "fast-forward";
    public const string InsertMedia = "insert-media";
    public const string EjectMedia = "eject-media";
    public const string NextMedia = "next-media";

    public static IReadOnlyDictionary<string, string> Values { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [ReleaseMouse] = "F12",
        [PauseResume] = "Pause",
        [ToggleFullscreen] = "Alt+Enter",
        [Power] = "Ctrl+Alt+End",
        [SoftReset] = "Ctrl+Alt+R",
        [HardReset] = "Ctrl+Shift+Alt+R",
        [QuickSave] = "Ctrl+F5",
        [QuickLoad] = "Ctrl+F9",
        [Screenshot] = "F11",
        [ToggleMute] = "Ctrl+Shift+M",
        [FastForward] = "Ctrl+F11",
        [InsertMedia] = "Ctrl+I",
        [EjectMedia] = "Ctrl+E",
        [NextMedia] = "Ctrl+PageDown"
    };
}
