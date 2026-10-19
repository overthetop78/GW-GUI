using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Functions;

internal static class ExternalCoreKeyboardFunctions
{
    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => (ushort)(
        (HasShift(keys) ? ExternalCoreKeyboardConstants.ShiftModifier : ExternalCoreKeyboardConstants.NoModifiers) |
        (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl) ? ExternalCoreKeyboardConstants.ControlModifier : ExternalCoreKeyboardConstants.NoModifiers) |
        (keys.Contains(EmulationKey.LeftAlt) || keys.Contains(EmulationKey.RightAlt) ? ExternalCoreKeyboardConstants.AltModifier : ExternalCoreKeyboardConstants.NoModifiers));

    internal static void PublishTransitions(IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<EmulationKey, uint> keyMap, ushort modifiers,
        ref IReadOnlySet<EmulationKey> previous, ref ExternalCoreApi.KeyboardEvent? keyboardEvent,
        Func<EmulationKey, bool> isModifier, Func<uint, IReadOnlySet<EmulationKey>, uint> characterFor)
    {
        foreach (var key in previous.Except(keys).OrderBy(key => isModifier(key) ? ExternalCoreKeyboardConstants.ModifierLastOrder : ExternalCoreKeyboardConstants.ModifierFirstOrder))
            if (keyMap.TryGetValue(key, out var code))
                keyboardEvent!(false, code, ExternalCoreKeyboardConstants.NoCharacter, modifiers);
        foreach (var key in keys.Except(previous).OrderBy(key => isModifier(key) ? ExternalCoreKeyboardConstants.ModifierFirstOrder : ExternalCoreKeyboardConstants.ModifierLastOrder))
            if (keyMap.TryGetValue(key, out var code))
                keyboardEvent!(true, code, characterFor(code, keys), modifiers);
        previous = new HashSet<EmulationKey>(keys);
    }

    internal static IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap()
    {
        var map = new Dictionary<EmulationKey, uint>
        {
            [EmulationKey.Backspace] = ExternalCoreKeyboardConstants.Backspace,
            [EmulationKey.Tab] = ExternalCoreKeyboardConstants.Tab,
            [EmulationKey.Return] = ExternalCoreKeyboardConstants.Return,
            [EmulationKey.Escape] = ExternalCoreKeyboardConstants.Escape,
            [EmulationKey.Space] = ExternalCoreKeyboardConstants.Space,
            [EmulationKey.Comma] = ',', [EmulationKey.Minus] = '-', [EmulationKey.Period] = '.',
            [EmulationKey.Slash] = '/', [EmulationKey.Semicolon] = ';', [EmulationKey.Equals] = '=',
            [EmulationKey.LeftBracket] = '[', [EmulationKey.Backslash] = '\\',
            [EmulationKey.RightBracket] = ']', [EmulationKey.Quote] = '\'', [EmulationKey.Backquote] = '`',
            [EmulationKey.Delete] = ExternalCoreKeyboardConstants.Delete,
            [EmulationKey.Up] = ExternalCoreKeyboardConstants.Up, [EmulationKey.Down] = ExternalCoreKeyboardConstants.Down,
            [EmulationKey.Right] = ExternalCoreKeyboardConstants.Right, [EmulationKey.Left] = ExternalCoreKeyboardConstants.Left,
            [EmulationKey.Insert] = ExternalCoreKeyboardConstants.Insert, [EmulationKey.Home] = ExternalCoreKeyboardConstants.Home,
            [EmulationKey.End] = ExternalCoreKeyboardConstants.End, [EmulationKey.PageUp] = ExternalCoreKeyboardConstants.PageUp,
            [EmulationKey.PageDown] = ExternalCoreKeyboardConstants.PageDown,
            [EmulationKey.CapsLock] = ExternalCoreKeyboardConstants.CapsLock,
            [EmulationKey.RightShift] = ExternalCoreKeyboardConstants.RightShift,
            [EmulationKey.LeftShift] = ExternalCoreKeyboardConstants.LeftShift,
            [EmulationKey.RightControl] = ExternalCoreKeyboardConstants.RightControl,
            [EmulationKey.LeftControl] = ExternalCoreKeyboardConstants.LeftControl,
            [EmulationKey.RightAlt] = ExternalCoreKeyboardConstants.RightAlt,
            [EmulationKey.LeftAlt] = ExternalCoreKeyboardConstants.LeftAlt,
            [EmulationKey.LeftAmiga] = ExternalCoreKeyboardConstants.LeftMeta,
            [EmulationKey.RightAmiga] = ExternalCoreKeyboardConstants.RightMeta,
            [EmulationKey.Help] = ExternalCoreKeyboardConstants.Help,
            [EmulationKey.Break] = ExternalCoreKeyboardConstants.Break,
            [EmulationKey.Undo] = ExternalCoreKeyboardConstants.Undo,
            [EmulationKey.NumpadPeriod] = ExternalCoreKeyboardConstants.KeypadPeriod,
            [EmulationKey.NumpadDivide] = ExternalCoreKeyboardConstants.KeypadDivide,
            [EmulationKey.NumpadMultiply] = ExternalCoreKeyboardConstants.KeypadMultiply,
            [EmulationKey.NumpadMinus] = ExternalCoreKeyboardConstants.KeypadMinus,
            [EmulationKey.NumpadPlus] = ExternalCoreKeyboardConstants.KeypadPlus,
            [EmulationKey.NumpadEnter] = ExternalCoreKeyboardConstants.KeypadEnter
        };
        AddRange(map, EmulationKey.A, ExternalCoreKeyboardConstants.FirstLetter, ExternalCoreKeyboardConstants.LetterCount);
        AddRange(map, EmulationKey.D0, ExternalCoreKeyboardConstants.FirstDigit, ExternalCoreKeyboardConstants.DigitCount);
        AddRange(map, EmulationKey.F1, ExternalCoreKeyboardConstants.FirstFunctionKey, ExternalCoreKeyboardConstants.FunctionKeyCount);
        AddRange(map, EmulationKey.Numpad0, ExternalCoreKeyboardConstants.FirstKeypadDigit, ExternalCoreKeyboardConstants.DigitCount);
        return map;
    }


    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys)
    {
        var shifted = HasShift(keys);
        if (code is >= ExternalCoreKeyboardConstants.FirstLetter and <= ExternalCoreKeyboardConstants.LastLetter)
            return shifted ^ keys.Contains(EmulationKey.CapsLock) ? code - ExternalCoreKeyboardConstants.UppercaseOffset : code;
        if (!shifted) return IsPrintable(code) ? code : ExternalCoreKeyboardConstants.NoCharacter;
        return code switch
        {
            '0' => ')', '1' => '!', '2' => '@', '3' => '#', '4' => '$', '5' => '%',
            '6' => '^', '7' => '&', '8' => '*', '9' => '(', '-' => '_', '=' => '+',
            '[' => '{', ']' => '}', '\\' => '|', ';' => ':', '\'' => '"', ',' => '<',
            '.' => '>', '/' => '?', '`' => '~', _ => IsPrintable(code) ? code : ExternalCoreKeyboardConstants.NoCharacter
        };
    }


    private static bool HasShift(IReadOnlySet<EmulationKey> keys) => keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);


    private static bool IsPrintable(uint code) => code is >= ExternalCoreKeyboardConstants.FirstPrintableCharacter and <= ExternalCoreKeyboardConstants.LastPrintableCharacter;


    internal static void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count)
    {
        for (var offset = ExternalCoreKeyboardConstants.FirstRangeOffset; offset < count; offset++)
            map[(EmulationKey)((int)firstKey + offset)] = firstCode + checked((uint)offset);
    }

}
