namespace GWGUI.Domain.Settings.Emulation;

public static class EmulationShortcutDefaultFunctions
{
    public static Dictionary<string, string> Create() =>
        new(EmulationShortcutDefaults.Values, StringComparer.Ordinal);
}
