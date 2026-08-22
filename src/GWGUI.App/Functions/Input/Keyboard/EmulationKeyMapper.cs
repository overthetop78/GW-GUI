using System.Windows.Input;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Input.Keyboard;

public static class EmulationKeyMapper
{
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
        if (key is >= Key.F1 and <= Key.F10)
        {
            result = (EmulationKey)((int)EmulationKey.F1 + key - Key.F1);
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
            Key.LWin => EmulationKey.LeftAmiga, Key.RWin => EmulationKey.RightAmiga,
            Key.Delete => EmulationKey.Delete, Key.Insert => EmulationKey.Insert,
            Key.Home => EmulationKey.Home, Key.End => EmulationKey.End,
            Key.PageUp => EmulationKey.PageUp, Key.PageDown => EmulationKey.PageDown,
            Key.CapsLock => EmulationKey.CapsLock, Key.Help => EmulationKey.Help,
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
