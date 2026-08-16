using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariKeyboardFunctions
{
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap()
    {
        var map = new Dictionary<EmulationKey, uint>
        {
            [EmulationKey.Backspace] = AtariKeyboardConstants.Backspace,
            [EmulationKey.Tab] = AtariKeyboardConstants.Tab,
            [EmulationKey.Return] = AtariKeyboardConstants.Return,
            [EmulationKey.Escape] = AtariKeyboardConstants.Escape,
            [EmulationKey.Space] = AtariKeyboardConstants.Space,
            [EmulationKey.Comma] = ',', [EmulationKey.Minus] = '-', [EmulationKey.Period] = '.',
            [EmulationKey.Slash] = '/', [EmulationKey.Semicolon] = ';', [EmulationKey.Equals] = '=',
            [EmulationKey.LeftBracket] = '[', [EmulationKey.Backslash] = '\\',
            [EmulationKey.RightBracket] = ']', [EmulationKey.Quote] = '\'', [EmulationKey.Backquote] = '`',
            [EmulationKey.Delete] = AtariKeyboardConstants.Delete,
            [EmulationKey.Up] = AtariKeyboardConstants.Up, [EmulationKey.Down] = AtariKeyboardConstants.Down,
            [EmulationKey.Right] = AtariKeyboardConstants.Right, [EmulationKey.Left] = AtariKeyboardConstants.Left,
            [EmulationKey.Insert] = AtariKeyboardConstants.Insert, [EmulationKey.Home] = AtariKeyboardConstants.Home,
            [EmulationKey.End] = AtariKeyboardConstants.End, [EmulationKey.PageUp] = AtariKeyboardConstants.PageUp,
            [EmulationKey.PageDown] = AtariKeyboardConstants.PageDown,
            [EmulationKey.CapsLock] = AtariKeyboardConstants.CapsLock,
            [EmulationKey.RightShift] = AtariKeyboardConstants.RightShift,
            [EmulationKey.LeftShift] = AtariKeyboardConstants.LeftShift,
            [EmulationKey.RightControl] = AtariKeyboardConstants.RightControl,
            [EmulationKey.LeftControl] = AtariKeyboardConstants.LeftControl,
            [EmulationKey.RightAlt] = AtariKeyboardConstants.RightAlt,
            [EmulationKey.LeftAlt] = AtariKeyboardConstants.LeftAlt,
            [EmulationKey.LeftAmiga] = AtariKeyboardConstants.LeftMeta,
            [EmulationKey.RightAmiga] = AtariKeyboardConstants.RightMeta,
            [EmulationKey.Help] = AtariKeyboardConstants.Help,
            [EmulationKey.AtariBreak] = AtariKeyboardConstants.Break,
            [EmulationKey.AtariUndo] = AtariKeyboardConstants.Undo,
            [EmulationKey.NumpadPeriod] = AtariKeyboardConstants.KeypadPeriod,
            [EmulationKey.NumpadDivide] = AtariKeyboardConstants.KeypadDivide,
            [EmulationKey.NumpadMultiply] = AtariKeyboardConstants.KeypadMultiply,
            [EmulationKey.NumpadMinus] = AtariKeyboardConstants.KeypadMinus,
            [EmulationKey.NumpadPlus] = AtariKeyboardConstants.KeypadPlus,
            [EmulationKey.NumpadEnter] = AtariKeyboardConstants.KeypadEnter
        };
        AddRange(map, EmulationKey.A, AtariKeyboardConstants.FirstLetter, AtariKeyboardConstants.LetterCount);
        AddRange(map, EmulationKey.D0, AtariKeyboardConstants.FirstDigit, AtariKeyboardConstants.DigitCount);
        AddRange(map, EmulationKey.F1, AtariKeyboardConstants.FirstFunctionKey, AtariKeyboardConstants.FunctionKeyCount);
        AddRange(map, EmulationKey.Numpad0, AtariKeyboardConstants.FirstKeypadDigit, AtariKeyboardConstants.DigitCount);
        return map;
    }

    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => (ushort)(
        (HasShift(keys) ? AtariKeyboardConstants.ShiftModifier : AtariKeyboardConstants.NoModifiers) |
        (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl)
            ? AtariKeyboardConstants.ControlModifier : AtariKeyboardConstants.NoModifiers) |
        (keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt)
            ? AtariKeyboardConstants.AltModifier : AtariKeyboardConstants.NoModifiers));

    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys)
    {
        var shifted = HasShift(keys);
        if (code is >= AtariKeyboardConstants.FirstLetter and <= AtariKeyboardConstants.LastLetter)
            return shifted ^ keys.Contains(EmulationKey.CapsLock) ? code - AtariKeyboardConstants.UppercaseOffset : code;
        if (!shifted) return IsPrintable(code) ? code : AtariKeyboardConstants.NoCharacter;
        return code switch
        {
            '0' => ')', '1' => '!', '2' => '@', '3' => '#', '4' => '$', '5' => '%',
            '6' => '^', '7' => '&', '8' => '*', '9' => '(', '-' => '_', '=' => '+',
            '[' => '{', ']' => '}', '\\' => '|', ';' => ':', '\'' => '"', ',' => '<',
            '.' => '>', '/' => '?', '`' => '~', _ => IsPrintable(code) ? code : AtariKeyboardConstants.NoCharacter
        };
    }

    internal static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift
        or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt
        or EmulationKey.LeftAmiga or EmulationKey.RightAmiga;

    internal static bool IsConsoleKeyActive(IReadOnlySet<EmulationKey> keys, uint buttonId) => buttonId switch
    {
        AtariInputConstants.JoypadLeftShoulderId => keys.Contains(EmulationKey.AtariOption),
        AtariInputConstants.JoypadSelectId => keys.Contains(EmulationKey.AtariSelect),
        AtariInputConstants.JoypadStartId => keys.Contains(EmulationKey.AtariStart),
        AtariInputConstants.JoypadRightTriggerId => keys.Contains(EmulationKey.Help),
        _ => false
    };

    private static bool HasShift(IReadOnlySet<EmulationKey> keys) =>
        keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);

    private static bool IsPrintable(uint code) =>
        code is >= AtariKeyboardConstants.FirstPrintableCharacter and <= AtariKeyboardConstants.LastPrintableCharacter;

    private static void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count)
    {
        for (var offset = AtariKeyboardConstants.FirstRangeOffset; offset < count; offset++)
            map[(EmulationKey)((int)firstKey + offset)] = firstCode + checked((uint)offset);
    }
}
