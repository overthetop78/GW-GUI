using GWGUI.App.Constants.Input.Bindings;
using GWGUI.App.Contracts.Input;
using System.Windows.Input;

namespace GWGUI.App.Functions.Input.Bindings;

public static class InputBindingSyntax
{
    public static string Keyboard(string binding) => InputBindingSyntaxConstants.KeyboardPrefix + binding;
    public static string Mouse(string source) => InputBindingSyntaxConstants.MousePrefix + source;
    public static string Controller(string deviceId, string source) => $"{InputBindingSyntaxConstants.ControllerPrefix}{deviceId}:{source}";

    public static bool TryRemovePrefix(string? binding, string prefix, out string source)
    {
        source = string.Empty;
        if (string.IsNullOrWhiteSpace(binding) || !binding.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;
        source = binding[prefix.Length..];
        return source.Length > 0;
    }

    public static bool IsReservedShortcut(Key key, ModifierKeys modifiers) => KeyboardChordFunctions.IsWindowsReserved(new KeyboardChord(modifiers, [key]));

    public static bool TryGetKeyboardChord(string binding, out KeyboardChord chord)
    {
        var parts = binding.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var keys = parts.Where(part => !part.StartsWith(MousePrefix, StringComparison.OrdinalIgnoreCase) && !part.StartsWith(ControllerPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(part => TryRemovePrefix(part, InputBindingSyntaxConstants.KeyboardPrefix, out var key) ? key : part);
        return KeyboardChordFunctions.TryParse(string.Join("+", keys), out chord);
    }

    private const string MousePrefix = InputBindingSyntaxConstants.MousePrefix;
    private const string ControllerPrefix = InputBindingSyntaxConstants.ControllerPrefix;

    public static bool HasNonKeyboardSource(string binding) => binding.Split('+').Any(part => part.StartsWith(MousePrefix, StringComparison.OrdinalIgnoreCase) || part.StartsWith(ControllerPrefix, StringComparison.OrdinalIgnoreCase));

    public static bool Conflicts(string first, string second)
    {
        static string[] PhysicalParts(string binding) => binding.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(part => part.StartsWith(MousePrefix, StringComparison.OrdinalIgnoreCase) || part.StartsWith(ControllerPrefix, StringComparison.OrdinalIgnoreCase)).OrderBy(part => part, StringComparer.OrdinalIgnoreCase).ToArray();
        if (!PhysicalParts(first).SequenceEqual(PhysicalParts(second), StringComparer.OrdinalIgnoreCase)) return false;
        var firstKeyboard = TryGetKeyboardChord(first, out var left);
        var secondKeyboard = TryGetKeyboardChord(second, out var right);
        if (!firstKeyboard || !secondKeyboard) return firstKeyboard == secondKeyboard
            && (PhysicalParts(first).Length > 0 || string.Equals(first.Trim(), second.Trim(), StringComparison.OrdinalIgnoreCase));
        if (left.Modifiers != right.Modifiers || !left.Keys.Where(key => !KeyboardChordFunctions.IsModifierKey(key)).Order().SequenceEqual(right.Keys.Where(key => !KeyboardChordFunctions.IsModifierKey(key)).Order())) return false;
        foreach (var modifier in new[] { ModifierKeys.Control, ModifierKeys.Alt, ModifierKeys.Shift })
        {
            var leftSides = left.Keys.Where(key => KeyboardChordFunctions.ModifierForKey(key) == modifier).Order().ToArray();
            var rightSides = right.Keys.Where(key => KeyboardChordFunctions.ModifierForKey(key) == modifier).Order().ToArray();
            if (leftSides.Length > 0 && rightSides.Length > 0 && !leftSides.SequenceEqual(rightSides)) return false;
        }
        return true;
    }
}
