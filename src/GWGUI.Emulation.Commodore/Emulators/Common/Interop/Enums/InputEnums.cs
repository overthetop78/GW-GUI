namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Enums;

internal enum NativeKeyboardKey : uint
{
    Backspace = 8,
    Tab = 9,
    Return = 13,
    Escape = 27,
    Space = 32,
    Comma = 44,
    Minus = 45,
    Period = 46,
    Slash = 47,
    Semicolon = 59,
    Equals = 61,
    LeftBracket = 91,
    Backslash = 92,
    RightBracket = 93,
    Quote = 39,
    Backquote = 96,
    Delete = 127,
    Up = 273,
    Down = 274,
    Right = 275,
    Left = 276,
    Insert = 277,
    Home = 278,
    End = 279,
    PageUp = 280,
    PageDown = 281,
    CapsLock = 301,
    RightShift = 303,
    LeftShift = 304,
    RightControl = 305,
    LeftControl = 306,
    RightAlt = 307,
    LeftAlt = 308,
    LeftAmiga = 311,
    RightAmiga = 312,
    Help = 315,
    NumpadPeriod = 266,
    NumpadDivide = 267,
    NumpadMultiply = 268,
    NumpadMinus = 269,
    NumpadPlus = 270,
    NumpadEnter = 271,
    FirstLetter = 97,
    LastLetter = 122,
    FirstDigit = 48,
    FirstFunctionKey = 282,
    FirstNumpadDigit = 256
}

[Flags]
internal enum NativeKeyboardModifier : ushort
{
    None = 0,
    Shift = 1,
    Control = 2,
    Alt = 4
}

internal enum NativeMouseInput : uint
{
    DeltaX = 0, DeltaY = 1, LeftButton = 2, RightButton = 3, WheelUp = 4, WheelDown = 5, MiddleButton = 6
}

internal enum NativeAnalogStick : uint { Left = 0, Right = 1 }
internal enum NativeAnalogAxis : uint { X = 0, Y = 1 }
