namespace GWGUI.Domain.Settings;

public static class EmulationShortcutDefaultFunctions
{
    public static Dictionary<string, string> Create() =>
        new(EmulationShortcutDefaults.Values, StringComparer.Ordinal);
}
