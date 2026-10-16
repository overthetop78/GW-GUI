namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Constants;

internal static class KeyboardConstants
{
    internal const uint Backspace = 8, Return = 13, Escape = 27, Space = 32, Delete = 127;
    internal const uint Up = 273, Down = 274, Right = 275, Left = 276, PageUp = 280, PageDown = 281;
    internal const uint FirstFunctionKey = 282, FirstLetter = 97, FirstDigit = 48;
    internal const uint NativeF5 = FirstFunctionKey + 4, NativeF6 = FirstFunctionKey + 5, NativeF7 = FirstFunctionKey + 6, NativeF8 = FirstFunctionKey + 7;
    internal const uint NativeF9 = FirstFunctionKey + 8, NativeF10 = FirstFunctionKey + 9, NativeF11 = FirstFunctionKey + 10, NativeF12 = FirstFunctionKey + 11;
    internal const int LetterCount = 26, DigitCount = 10, CommonFunctionKeyCount = 4;
    internal const uint NativeHelp = 91, NativeShift = 93, NativeInputMethod = 92;
    internal const uint NoCharacter = 0;
    internal const ushort NoModifiers = 0;
}
