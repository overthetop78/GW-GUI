using GWGUI.Emulation;

using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;

internal static class KeyboardFunctions
{
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
            [EmulationKey.Home] = ExternalCoreKeyboardConstants.Home,
            [EmulationKey.End] = ExternalCoreKeyboardConstants.End, [EmulationKey.PageUp] = ExternalCoreKeyboardConstants.PageUp,
            [EmulationKey.CapsLock] = ExternalCoreKeyboardConstants.CapsLock,
            [EmulationKey.RightShift] = ExternalCoreKeyboardConstants.RightShift,
            [EmulationKey.LeftShift] = ExternalCoreKeyboardConstants.LeftShift,
            [EmulationKey.RightControl] = ExternalCoreKeyboardConstants.RightControl,
            [EmulationKey.LeftControl] = ExternalCoreKeyboardConstants.LeftControl,
            [EmulationKey.Break] = KeyboardConstants.Pause,
            [EmulationKey.AcornF0] = KeyboardConstants.F0,
            [EmulationKey.AcornCopy] = ExternalCoreKeyboardConstants.Insert,
            [EmulationKey.AcornShiftLock] = ExternalCoreKeyboardConstants.PageDown,
            [EmulationKey.AcornKeypadHash] = KeyboardConstants.Hash,
            [EmulationKey.AcornKeypadDelete] = KeyboardConstants.Clear,
            [EmulationKey.AcornKeypadComma] = KeyboardConstants.KeypadEquals,
            [EmulationKey.NumpadPeriod] = ExternalCoreKeyboardConstants.KeypadPeriod,
            [EmulationKey.NumpadDivide] = ExternalCoreKeyboardConstants.KeypadDivide,
            [EmulationKey.NumpadMultiply] = ExternalCoreKeyboardConstants.KeypadMultiply,
            [EmulationKey.NumpadMinus] = ExternalCoreKeyboardConstants.KeypadMinus,
            [EmulationKey.NumpadPlus] = ExternalCoreKeyboardConstants.KeypadPlus,
            [EmulationKey.NumpadEnter] = ExternalCoreKeyboardConstants.KeypadEnter
        };
        AddRange(map, EmulationKey.A, ExternalCoreKeyboardConstants.FirstLetter, ExternalCoreKeyboardConstants.LetterCount);
        AddRange(map, EmulationKey.D0, ExternalCoreKeyboardConstants.FirstDigit, ExternalCoreKeyboardConstants.DigitCount);
        AddRange(map, EmulationKey.F1, ExternalCoreKeyboardConstants.FirstFunctionKey, KeyboardConstants.FunctionKeyCount);
        AddRange(map, EmulationKey.Numpad0, ExternalCoreKeyboardConstants.FirstKeypadDigit, ExternalCoreKeyboardConstants.DigitCount);
        return map;
    }

    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => (ushort)(
        (HasShift(keys) ? ExternalCoreKeyboardConstants.ShiftModifier : ExternalCoreKeyboardConstants.NoModifiers) |
        (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl) ? ExternalCoreKeyboardConstants.ControlModifier : ExternalCoreKeyboardConstants.NoModifiers));

    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys)
        => GWGUI.Emulation.Functions.ExternalCoreKeyboardFunctions.Character(code, keys);

    internal static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift or EmulationKey.LeftControl or EmulationKey.RightControl;

    private static bool HasShift(IReadOnlySet<EmulationKey> keys) => keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift);

    private static void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count)
        => GWGUI.Emulation.Functions.ExternalCoreKeyboardFunctions.AddRange(map, firstKey, firstCode, count);
}
