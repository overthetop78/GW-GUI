namespace GWGUI.Emulation.Constants;


/// <summary>Codes, modificateurs et bornes du clavier natif partagé par les modules.</summary>
/// <remarks>Les codes et plages appartiennent au contrat de données. Les indices de tri et calculs
/// employés par les fonctions du SDK conservent leur visibilité interne.</remarks>
public static class ExternalCoreKeyboardConstants
{
    public const uint Backspace = 8;
    public const uint Tab = 9;
    public const uint Return = 13;
    public const uint Escape = 27;
    public const uint Space = 32;
    public const uint FirstPrintableCharacter = 32;
    public const uint LastPrintableCharacter = 126;
    public const uint Delete = 127;
    public const uint FirstKeypadDigit = 256;
    public const uint KeypadPeriod = 266;
    public const uint KeypadDivide = 267;
    public const uint KeypadMultiply = 268;
    public const uint KeypadMinus = 269;
    public const uint KeypadPlus = 270;
    public const uint KeypadEnter = 271;
    public const uint Up = 273;
    public const uint Down = 274;
    public const uint Right = 275;
    public const uint Left = 276;
    public const uint Insert = 277;
    public const uint Home = 278;
    public const uint End = 279;
    public const uint PageUp = 280;
    public const uint PageDown = 281;
    public const uint FirstFunctionKey = 282;
    public const uint CapsLock = 301;
    public const uint RightShift = 303;
    public const uint LeftShift = 304;
    public const uint RightControl = 305;
    public const uint LeftControl = 306;
    public const uint RightAlt = 307;
    public const uint LeftAlt = 308;
    public const uint LeftMeta = 311;
    public const uint RightMeta = 312;
    public const uint Help = 315;
    public const uint Break = 318;
    public const uint Undo = 322;
    public const uint FirstLetter = 'a';
    public const uint LastLetter = 'z';
    public const uint FirstDigit = '0';
    public const int LetterCount = 26;
    public const int DigitCount = 10;
    public const int FunctionKeyCount = 10;
    public const ushort ShiftModifier = 1;
    public const ushort ControlModifier = 2;
    public const ushort AltModifier = 4;
    internal const uint UppercaseOffset = 32;
    public const uint NoCharacter = 0;
    public const ushort NoModifiers = 0;
    internal const int FirstRangeOffset = 0;
    internal const int ModifierFirstOrder = 0;
    internal const int ModifierLastOrder = 1;
}
