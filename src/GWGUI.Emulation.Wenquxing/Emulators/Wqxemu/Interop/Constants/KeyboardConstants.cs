namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Constants;

internal static class KeyboardConstants
{
    internal const uint Backspace = 8, Return = 13, Escape = 27, Space = 32, Delete = 127;
    // WQXEmu's Libretro bridge uses these private key codes instead of the standard RETROK navigation values.
    internal const uint Left = 0x250, Up = 0x251, Right = 0x252, Down = 0x253, PageUp = 0x254, PageDown = 0x255;
    internal const uint FirstFunctionKey = 282, FirstLetter = 97, FirstDigit = 48;
    internal const uint NativeF5 = FirstFunctionKey + 4, NativeF6 = FirstFunctionKey + 5, NativeF7 = FirstFunctionKey + 6, NativeF8 = FirstFunctionKey + 7;
    internal const uint NativeF9 = FirstFunctionKey + 8, NativeF10 = FirstFunctionKey + 9, NativeF11 = FirstFunctionKey + 10, NativeF12 = FirstFunctionKey + 11;
    internal const int LetterCount = 26, DigitCount = 10, CommonFunctionKeyCount = 4;
    internal const uint NativeHelp = 91, NativeShift = 93, NativeInputMethod = 92;
    internal const uint NoCharacter = 0;
    internal const ushort NoModifiers = 0;
}
