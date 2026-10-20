using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
internal static class KeyboardFunctions
{
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap(Emulator emulator)
    {
        var map = new Dictionary<EmulationKey, uint>
        {
            [EmulationKey.Backspace] = KeyboardConstants.Backspace,
            [EmulationKey.Tab] = KeyboardConstants.Tab,
            [EmulationKey.Return] = KeyboardConstants.Return,
            [EmulationKey.Escape] = KeyboardConstants.Escape,
            [EmulationKey.Space] = KeyboardConstants.Space,
            [EmulationKey.Comma] = ',',
            [EmulationKey.Minus] = '-',
            [EmulationKey.Period] = '.',
            [EmulationKey.Slash] = '/',
            [EmulationKey.Semicolon] = ';',
            [EmulationKey.Equals] = '=',
            [EmulationKey.LeftBracket] = '[',
            [EmulationKey.Backslash] = '\\',
            [EmulationKey.RightBracket] = ']',
            [EmulationKey.Quote] = '\'',
            [EmulationKey.Backquote] = '`',
            [EmulationKey.Delete] = KeyboardConstants.Delete,
            [EmulationKey.Up] = KeyboardConstants.Up,
            [EmulationKey.Down] = KeyboardConstants.Down,
            [EmulationKey.Right] = KeyboardConstants.Right,
            [EmulationKey.Left] = KeyboardConstants.Left,
            [EmulationKey.Insert] = KeyboardConstants.Insert,
            [EmulationKey.Home] = KeyboardConstants.Home,
            [EmulationKey.End] = KeyboardConstants.End,
            [EmulationKey.PageUp] = KeyboardConstants.PageUp,
            [EmulationKey.PageDown] = KeyboardConstants.PageDown,
            [EmulationKey.CapsLock] = KeyboardConstants.CapsLock,
            [EmulationKey.RightShift] = KeyboardConstants.RightShift,
            [EmulationKey.LeftShift] = KeyboardConstants.LeftShift,
            [EmulationKey.RightControl] = KeyboardConstants.RightControl,
            [EmulationKey.LeftControl] = KeyboardConstants.LeftControl,
            [EmulationKey.RightAlt] = KeyboardConstants.RightAlt,
            [EmulationKey.LeftAlt] = KeyboardConstants.LeftAlt,
            [EmulationKey.NumpadPeriod] = KeyboardConstants.KeypadPeriod,
            [EmulationKey.NumpadDivide] = KeyboardConstants.KeypadDivide,
            [EmulationKey.NumpadMultiply] = KeyboardConstants.KeypadMultiply,
            [EmulationKey.NumpadMinus] = KeyboardConstants.KeypadMinus,
            [EmulationKey.NumpadPlus] = KeyboardConstants.KeypadPlus,
            [EmulationKey.NumpadEnter] = KeyboardConstants.KeypadEnter
        };
        AddRange(map, EmulationKey.A, KeyboardConstants.FirstLetter, KeyboardConstants.LetterCount);
        AddRange(map, EmulationKey.D0, KeyboardConstants.FirstDigit, KeyboardConstants.DigitCount);
        AddRange(map, EmulationKey.F1, KeyboardConstants.FirstFunctionKey, KeyboardConstants.FunctionKeyCount);
        AddRange(map, EmulationKey.Numpad0, KeyboardConstants.FirstKeypadDigit, KeyboardConstants.DigitCount);
        if (emulator == Emulator.Px68k)
        {
            map.Remove(EmulationKey.LeftAlt);
            map.Remove(EmulationKey.RightAlt);
            map.Remove(EmulationKey.End);
            map[EmulationKey.Help] = KeyboardConstants.HelpFunctionKey;
            map[EmulationKey.Undo] = KeyboardConstants.End;
            map[EmulationKey.Break] = KeyboardConstants.Pause;
            map[EmulationKey.SharpOption1] = KeyboardConstants.LeftSuper;
            map[EmulationKey.SharpOption2] = KeyboardConstants.RightSuper;
            map[EmulationKey.SharpSymbol] = KeyboardConstants.Print;
            map[EmulationKey.SharpRegister] = KeyboardConstants.ScrollLock;
            map[EmulationKey.SharpKeypadEquals] = KeyboardConstants.KeypadEquals;
        }
        else if (emulator == Emulator.X1)
        {
            map.Remove(EmulationKey.LeftAlt);
            map.Remove(EmulationKey.RightAlt);
            map[EmulationKey.Break] = KeyboardConstants.Pause;
            map[EmulationKey.SharpGraph] = KeyboardConstants.LeftAlt;
            map[EmulationKey.SharpCopy] = KeyboardConstants.Print;
            map[EmulationKey.SharpKeypadEquals] = KeyboardConstants.KeypadEquals;
        }

        return map;
    }

    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => (ushort)((HasShift(keys) ? KeyboardConstants.ShiftModifier : KeyboardConstants.NoModifiers)
        | (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl) ? KeyboardConstants.ControlModifier : KeyboardConstants.NoModifiers)
        | (keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt) ? KeyboardConstants.AltModifier : KeyboardConstants.NoModifiers));
    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys)
        => GWGUI.Emulation.Functions.ExternalCoreKeyboardFunctions.Character(code, keys);
    private static bool HasShift(IReadOnlySet<EmulationKey> keys) => keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);
    private static void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count)
        => GWGUI.Emulation.Functions.ExternalCoreKeyboardFunctions.AddRange(map, firstKey, firstCode, count);
}
