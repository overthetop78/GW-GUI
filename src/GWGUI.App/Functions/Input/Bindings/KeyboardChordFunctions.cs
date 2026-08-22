using GWGUI.App.Contracts.Input;
using System.Windows.Input;

namespace GWGUI.App.Functions.Input.Bindings;

public static class KeyboardChordFunctions
{
    public static bool TryParse(string? text, out KeyboardChord chord)
    {
        chord = new KeyboardChord(ModifierKeys.None, []);
        if (string.IsNullOrWhiteSpace(text)) return false;
        var modifiers = ModifierKeys.None;
        var keys = new List<Key>();
        foreach (var part in text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            switch (part.ToLowerInvariant())
            {
                case "ctrl" or "control": modifiers |= ModifierKeys.Control; continue;
                case "shift" or "maj": modifiers |= ModifierKeys.Shift; continue;
                case "alt": modifiers |= ModifierKeys.Alt; continue;
                case "win" or "windows": modifiers |= ModifierKeys.Windows; continue;
            }
            if (!Enum.TryParse<Key>(part, true, out var key) || key == Key.None || IsModifierKey(key)) return false;
            if (!keys.Contains(key)) keys.Add(key);
        }
        if (keys.Count == 0) return false;
        chord = new KeyboardChord(modifiers, keys);
        return true;
    }

    public static bool Matches(KeyboardChord chord, ModifierKeys modifiers, IReadOnlySet<Key> pressed) =>
        modifiers == chord.Modifiers && chord.Keys.Count == pressed.Count && chord.Keys.All(pressed.Contains);

    public static bool Contains(KeyboardChord chord, Key key) => chord.Keys.Contains(key);

    public static bool IsWindowsReserved(KeyboardChord chord) =>
        chord.Modifiers.HasFlag(ModifierKeys.Windows) ||
        chord.Modifiers.HasFlag(ModifierKeys.Alt) && chord.Keys.Any(key => key is Key.Tab or Key.Escape or Key.F4) ||
        chord.Modifiers == ModifierKeys.Control && chord.Keys.Contains(Key.Escape) ||
        chord.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && chord.Keys.Contains(Key.Escape) ||
        chord.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt) && chord.Keys.Contains(Key.Delete);

    public static string Format(ModifierKeys modifiers, IEnumerable<Key> keys)
    {
        var parts = new List<string>();
        if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        parts.AddRange(keys.Select(key => key.ToString()));
        return string.Join("+", parts);
    }

    public static bool IsModifierKey(Key key) => key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift
        or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin;
}
