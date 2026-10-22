using CommonControllerFunctions = GWGUI.Emulation.Coleco.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;

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
            [EmulationKey.Insert] = ExternalCoreKeyboardConstants.Insert, [EmulationKey.Home] = ExternalCoreKeyboardConstants.Home,
            [EmulationKey.AdamClear] = ExternalCoreKeyboardConstants.End,
            [EmulationKey.AdamMove] = ExternalCoreKeyboardConstants.PageUp,
            [EmulationKey.AdamStore] = ExternalCoreKeyboardConstants.PageDown,
            [EmulationKey.AdamUndo] = KeyboardConstants.Undo,
            [EmulationKey.AdamWildCard] = KeyboardConstants.WildCard,
            [EmulationKey.AdamPrint] = KeyboardConstants.Print,
            [EmulationKey.CapsLock] = ExternalCoreKeyboardConstants.CapsLock,
            [EmulationKey.RightShift] = ExternalCoreKeyboardConstants.RightShift,
            [EmulationKey.LeftShift] = ExternalCoreKeyboardConstants.LeftShift,
            [EmulationKey.RightControl] = ExternalCoreKeyboardConstants.RightControl,
            [EmulationKey.LeftControl] = ExternalCoreKeyboardConstants.LeftControl,
            [EmulationKey.RightAlt] = ExternalCoreKeyboardConstants.RightAlt,
            [EmulationKey.LeftAlt] = ExternalCoreKeyboardConstants.LeftAlt,
            [EmulationKey.NumpadPeriod] = ExternalCoreKeyboardConstants.KeypadPeriod,
            [EmulationKey.NumpadDivide] = ExternalCoreKeyboardConstants.KeypadDivide,
            [EmulationKey.NumpadMultiply] = ExternalCoreKeyboardConstants.KeypadMultiply,
            [EmulationKey.NumpadMinus] = ExternalCoreKeyboardConstants.KeypadMinus,
            [EmulationKey.NumpadPlus] = ExternalCoreKeyboardConstants.KeypadPlus,
            [EmulationKey.NumpadEnter] = ExternalCoreKeyboardConstants.KeypadEnter
        };
        AddRange(map, EmulationKey.A, ExternalCoreKeyboardConstants.FirstLetter, ExternalCoreKeyboardConstants.LetterCount);
        AddRange(map, EmulationKey.D0, ExternalCoreKeyboardConstants.FirstDigit, ExternalCoreKeyboardConstants.DigitCount);
        AddRange(map, EmulationKey.AdamSmart1, ExternalCoreKeyboardConstants.FirstFunctionKey, KeyboardConstants.SmartKeyCount);
        AddRange(map, EmulationKey.Numpad0, ExternalCoreKeyboardConstants.FirstKeypadDigit, ExternalCoreKeyboardConstants.DigitCount);
        return map;
    }

    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => CommonControllerFunctions.Modifiers(keys);

    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys)
        => CommonControllerFunctions.Character(code, keys);

    internal static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.RightShift or EmulationKey.LeftControl or EmulationKey.RightControl or EmulationKey.LeftAlt or EmulationKey.RightAlt;

    private static void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count)
        => CommonControllerFunctions.AddRange(map, firstKey, firstCode, count);
}
