using GWGUI.App.Constants.Input.Bindings;
using GWGUI.App.Contracts.Input;
using System.Windows.Input;

namespace GWGUI.App.Functions.Input.Bindings;

public static class InputBindingSyntax
{
    public static string Keyboard(string binding) => InputBindingSyntaxConstants.KeyboardPrefix + binding;
    public static string Mouse(string source) => InputBindingSyntaxConstants.MousePrefix + source;
    public static string Controller(int port, string source) => $"{InputBindingSyntaxConstants.XInputPrefix}{port}:{source}";

    public static bool TryRemovePrefix(string? binding, string prefix, out string source)
    {
        source = string.Empty;
        if (string.IsNullOrWhiteSpace(binding) || !binding.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;
        source = binding[prefix.Length..];
        return source.Length > 0;
    }

    public static bool IsReservedShortcut(Key key, ModifierKeys modifiers) =>
        KeyboardChordFunctions.IsWindowsReserved(new KeyboardChord(modifiers, [key]));
}
