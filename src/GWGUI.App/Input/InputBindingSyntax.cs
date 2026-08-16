using System.Windows.Input;

namespace GWGUI.App.Input;

public static class InputBindingSyntax
{
    public const string KeyboardPrefix = "Keyboard:";
    public const string MousePrefix = "Mouse:";
    public const string ControllerPrefix = "Controller:";
    public const string XInputPrefix = "Controller:xinput:";

    public static string Keyboard(string binding) => KeyboardPrefix + binding;
    public static string Mouse(string source) => MousePrefix + source;
    public static string Controller(int port, string source) => $"{XInputPrefix}{port}:{source}";

    public static bool TryRemovePrefix(string? binding, string prefix, out string source)
    {
        source = string.Empty;
        if (string.IsNullOrWhiteSpace(binding) || !binding.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;
        source = binding[prefix.Length..];
        return source.Length > 0;
    }

    public static bool IsReservedShortcut(Key key, ModifierKeys modifiers) =>
        KeyboardChord.IsWindowsReserved(new KeyboardChord(modifiers, [key]));
}
