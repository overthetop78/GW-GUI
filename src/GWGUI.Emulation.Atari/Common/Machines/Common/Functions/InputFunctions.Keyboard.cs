using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class KeyboardFunctions
{
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap()
    {
        var map = new Dictionary<EmulationKey, uint>
        {
            [EmulationKey.Backspace] = KeyboardConstants.Backspace,
            [EmulationKey.Tab] = KeyboardConstants.Tab,
            [EmulationKey.Return] = KeyboardConstants.Return,
            [EmulationKey.Escape] = KeyboardConstants.Escape,
            [EmulationKey.Space] = KeyboardConstants.Space,
            [EmulationKey.Comma] = ',', [EmulationKey.Minus] = '-', [EmulationKey.Period] = '.',
            [EmulationKey.Slash] = '/', [EmulationKey.Semicolon] = ';', [EmulationKey.Equals] = '=',
            [EmulationKey.LeftBracket] = '[', [EmulationKey.Backslash] = '\\',
            [EmulationKey.RightBracket] = ']', [EmulationKey.Quote] = '\'', [EmulationKey.Backquote] = '`',
            [EmulationKey.Delete] = KeyboardConstants.Delete,
            [EmulationKey.Up] = KeyboardConstants.Up, [EmulationKey.Down] = KeyboardConstants.Down,
            [EmulationKey.Right] = KeyboardConstants.Right, [EmulationKey.Left] = KeyboardConstants.Left,
            [EmulationKey.Insert] = KeyboardConstants.Insert, [EmulationKey.Home] = KeyboardConstants.Home,
            [EmulationKey.End] = KeyboardConstants.End, [EmulationKey.PageUp] = KeyboardConstants.PageUp,
            [EmulationKey.PageDown] = KeyboardConstants.PageDown,
            [EmulationKey.CapsLock] = KeyboardConstants.CapsLock,
            [EmulationKey.RightShift] = KeyboardConstants.RightShift,
            [EmulationKey.LeftShift] = KeyboardConstants.LeftShift,
            [EmulationKey.RightControl] = KeyboardConstants.RightControl,
            [EmulationKey.LeftControl] = KeyboardConstants.LeftControl,
            [EmulationKey.RightAlt] = KeyboardConstants.RightAlt,
            [EmulationKey.LeftAlt] = KeyboardConstants.LeftAlt,
            [EmulationKey.LeftAmiga] = KeyboardConstants.LeftMeta,
            [EmulationKey.RightAmiga] = KeyboardConstants.RightMeta,
            [EmulationKey.Help] = KeyboardConstants.Help,
            [EmulationKey.Break] = KeyboardConstants.Break,
            [EmulationKey.Undo] = KeyboardConstants.Undo,
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
        return map;
    }

    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => (ushort)(
        (HasShift(keys) ? KeyboardConstants.ShiftModifier : KeyboardConstants.NoModifiers) |
        (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl)
            ? KeyboardConstants.ControlModifier : KeyboardConstants.NoModifiers) |
        (keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt)
            ? KeyboardConstants.AltModifier : KeyboardConstants.NoModifiers));

    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys)
    {
        var shifted = HasShift(keys);
        if (code is >= KeyboardConstants.FirstLetter and <= KeyboardConstants.LastLetter)
            return shifted ^ keys.Contains(EmulationKey.CapsLock) ? code - KeyboardConstants.UppercaseOffset : code;
        if (!shifted) return IsPrintable(code) ? code : KeyboardConstants.NoCharacter;
        return code switch
        {
            '0' => ')', '1' => '!', '2' => '@', '3' => '#', '4' => '$', '5' => '%',
            '6' => '^', '7' => '&', '8' => '*', '9' => '(', '-' => '_', '=' => '+',
            '[' => '{', ']' => '}', '\\' => '|', ';' => ':', '\'' => '"', ',' => '<',
            '.' => '>', '/' => '?', '`' => '~', _ => IsPrintable(code) ? code : KeyboardConstants.NoCharacter
        };
    }

    internal static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift
        or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt
        or EmulationKey.LeftAmiga or EmulationKey.RightAmiga;

    internal static bool IsConsoleKeyActive(IReadOnlySet<EmulationKey> keys, uint buttonId) => buttonId switch
    {
        InputConstants.JoypadLeftShoulderId => keys.Contains(EmulationKey.AtariOption),
        InputConstants.JoypadSelectId => keys.Contains(EmulationKey.AtariSelect),
        InputConstants.JoypadStartId => keys.Contains(EmulationKey.AtariStart),
        InputConstants.JoypadRightTriggerId => keys.Contains(EmulationKey.Help),
        _ => false
    };

    private static bool HasShift(IReadOnlySet<EmulationKey> keys) =>
        keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);

    private static bool IsPrintable(uint code) =>
        code is >= KeyboardConstants.FirstPrintableCharacter and <= KeyboardConstants.LastPrintableCharacter;

    private static void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count)
    {
        for (var offset = KeyboardConstants.FirstRangeOffset; offset < count; offset++)
            map[(EmulationKey)((int)firstKey + offset)] = firstCode + checked((uint)offset);
    }
}
