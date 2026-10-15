using System.Windows.Input;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Input.Keyboard;

public static class EmulationKeyMapper
{
    private static readonly EmulationKey[] FunctionKeys =
    [
        EmulationKey.F1, EmulationKey.F2, EmulationKey.F3, EmulationKey.F4,
        EmulationKey.F5, EmulationKey.F6, EmulationKey.F7, EmulationKey.F8,
        EmulationKey.F9, EmulationKey.F10, EmulationKey.F11, EmulationKey.F12,
        EmulationKey.F13, EmulationKey.F14, EmulationKey.F15, EmulationKey.F16,
        EmulationKey.F17, EmulationKey.F18, EmulationKey.F19, EmulationKey.F20,
        EmulationKey.F21, EmulationKey.F22, EmulationKey.F23, EmulationKey.F24
    ];
    public static bool TryMap(EmulationKey source, out Key result)
    {
        foreach (var key in Enum.GetValues<Key>())
            if (TryMap(key, out var mapped) && mapped == source)
            {
                result = key;
                return true;
            }
        result = Key.None;
        return false;
    }

    public static bool TryMap(Key key, out EmulationKey result)
    {
        if (key is >= Key.A and <= Key.Z)
        {
            result = (EmulationKey)((int)EmulationKey.A + key - Key.A);
            return true;
        }
        if (key is >= Key.D0 and <= Key.D9)
        {
            result = (EmulationKey)((int)EmulationKey.D0 + key - Key.D0);
            return true;
        }
        if (key is >= Key.F1 and <= Key.F24)
        {
            result = FunctionKeys[key - Key.F1];
            return true;
        }
        if (key is >= Key.NumPad0 and <= Key.NumPad9)
        {
            result = (EmulationKey)((int)EmulationKey.Numpad0 + key - Key.NumPad0);
            return true;
        }
        result = key switch
        {
            Key.Back => EmulationKey.Backspace, Key.Tab => EmulationKey.Tab, Key.Enter => EmulationKey.Return,
            Key.Escape => EmulationKey.Escape, Key.Space => EmulationKey.Space, Key.Left => EmulationKey.Left,
            Key.Right => EmulationKey.Right, Key.Up => EmulationKey.Up, Key.Down => EmulationKey.Down,
            Key.LeftShift => EmulationKey.LeftShift, Key.RightShift => EmulationKey.RightShift,
            Key.LeftCtrl => EmulationKey.LeftControl, Key.RightCtrl => EmulationKey.RightControl,
            Key.LeftAlt => EmulationKey.LeftAlt, Key.RightAlt => EmulationKey.RightAlt,
            Key.LWin => EmulationKey.LeftWindows, Key.RWin => EmulationKey.RightWindows,
            Key.Delete => EmulationKey.Delete, Key.Insert => EmulationKey.Insert,
            Key.Home => EmulationKey.Home, Key.End => EmulationKey.End,
            Key.PageUp => EmulationKey.PageUp, Key.PageDown => EmulationKey.PageDown,
            Key.CapsLock => EmulationKey.CapsLock, Key.Help => EmulationKey.Help,
            Key.Pause => EmulationKey.Break,
            Key.OemComma => EmulationKey.Comma, Key.OemPeriod => EmulationKey.Period,
            Key.OemQuestion => EmulationKey.Slash, Key.OemMinus => EmulationKey.Minus,
            Key.OemPlus => EmulationKey.Equals, Key.OemSemicolon => EmulationKey.Semicolon,
            Key.OemQuotes => EmulationKey.Quote, Key.OemOpenBrackets => EmulationKey.LeftBracket,
            Key.OemCloseBrackets => EmulationKey.RightBracket, Key.OemBackslash => EmulationKey.Backslash,
            Key.Oem3 => EmulationKey.Backquote, Key.Decimal => EmulationKey.NumpadPeriod,
            Key.Divide => EmulationKey.NumpadDivide, Key.Multiply => EmulationKey.NumpadMultiply,
            Key.Subtract => EmulationKey.NumpadMinus, Key.Add => EmulationKey.NumpadPlus,
            _ => EmulationKey.Unknown
        };
        return result != EmulationKey.Unknown;
    }
}
