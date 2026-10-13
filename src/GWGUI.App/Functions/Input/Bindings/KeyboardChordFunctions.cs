using GWGUI.App.Contracts.Input;
using GWGUI.App.Functions.Input.Keyboard;
using GWGUI.Emulation;
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
            if (!Enum.TryParse<Key>(part, true, out var key) || key == Key.None)
            {
                if (!Enum.TryParse<EmulationKey>(part, true, out var emulated) || !EmulationKeyMapper.TryMap(emulated, out key)) return false;
            }
            modifiers |= ModifierForKey(key);
            if (!keys.Contains(key)) keys.Add(key);
        }
        if (keys.Count == 0 && modifiers == ModifierKeys.None) return false;
        chord = new KeyboardChord(modifiers, keys);
        return true;
    }

    public static bool Matches(KeyboardChord chord, ModifierKeys modifiers, IReadOnlySet<Key> pressed) => modifiers == chord.Modifiers
        && chord.Keys.Count(key => !IsModifierKey(key)) == pressed.Count(key => !IsModifierKey(key)) && chord.Keys.All(pressed.Contains);

    public static ModifierKeys ModifierForKey(Key key) => key switch
    {
        Key.LeftCtrl or Key.RightCtrl => ModifierKeys.Control,
        Key.LeftShift or Key.RightShift => ModifierKeys.Shift,
        Key.LeftAlt or Key.RightAlt => ModifierKeys.Alt,
        Key.LWin or Key.RWin => ModifierKeys.Windows,
        _ => ModifierKeys.None
    };

    public static bool Contains(KeyboardChord chord, Key key) => chord.Keys.Contains(key);

    public static bool IsWindowsReserved(KeyboardChord chord) => GWGUI.App.Dictionaries.Input.Bindings.ReservedKeyboardChordCatalog.Contains(chord);

    public static string Format(ModifierKeys modifiers, IEnumerable<Key> keys)
    {
        var parts = new List<string>();
        var ordered = keys.Distinct().OrderBy(key => key).ToArray();
        foreach (var (modifier, name) in new[] { (ModifierKeys.Control, "Ctrl"), (ModifierKeys.Alt, "Alt"), (ModifierKeys.Shift, "Shift"), (ModifierKeys.Windows, "Win") })
        {
            var explicitKeys = ordered.Where(key => ModifierForKey(key) == modifier).ToArray();
            if (explicitKeys.Length > 0) parts.AddRange(explicitKeys.Select(key => key.ToString()));
            else if (modifiers.HasFlag(modifier)) parts.Add(name);
        }
        parts.AddRange(ordered.Where(key => !IsModifierKey(key)).Select(key => key.ToString()));
        return string.Join("+", parts);
    }

    public static bool IsModifierKey(Key key) => key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin;
}
