using GWGUI.Emulation;

using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;

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
            [EmulationKey.Home] = KeyboardConstants.Home,
            [EmulationKey.End] = KeyboardConstants.End, [EmulationKey.PageUp] = KeyboardConstants.PageUp,
            [EmulationKey.CapsLock] = KeyboardConstants.CapsLock,
            [EmulationKey.RightShift] = KeyboardConstants.RightShift,
            [EmulationKey.LeftShift] = KeyboardConstants.LeftShift,
            [EmulationKey.RightControl] = KeyboardConstants.RightControl,
            [EmulationKey.LeftControl] = KeyboardConstants.LeftControl,
            [EmulationKey.Break] = KeyboardConstants.Pause,
            [EmulationKey.AcornF0] = KeyboardConstants.F0,
            [EmulationKey.AcornCopy] = KeyboardConstants.Insert,
            [EmulationKey.AcornShiftLock] = KeyboardConstants.PageDown,
            [EmulationKey.AcornKeypadHash] = KeyboardConstants.Hash,
            [EmulationKey.AcornKeypadDelete] = KeyboardConstants.Clear,
            [EmulationKey.AcornKeypadComma] = KeyboardConstants.KeypadEquals,
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
        (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl) ? KeyboardConstants.ControlModifier : KeyboardConstants.NoModifiers));

    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys)
        => GWGUI.Emulation.Functions.ExternalCoreKeyboardFunctions.Character(code, keys);

    internal static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift or EmulationKey.LeftControl or EmulationKey.RightControl;

    private static bool HasShift(IReadOnlySet<EmulationKey> keys) => keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);

    private static void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count)
        => GWGUI.Emulation.Functions.ExternalCoreKeyboardFunctions.AddRange(map, firstKey, firstCode, count);
}
