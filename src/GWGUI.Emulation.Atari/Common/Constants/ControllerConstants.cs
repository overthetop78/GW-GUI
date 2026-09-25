using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Atari.Common.Constants;

public static class ControllerConstants
{
    internal static readonly IReadOnlyList<string> DirectionActions = [EmulationControllerCommandIds.Up, EmulationControllerCommandIds.Down, EmulationControllerCommandIds.Left, EmulationControllerCommandIds.Right];
    internal static readonly IReadOnlyList<string> SingleFireActions = [EmulationControllerCommandIds.Fire1];
    internal static readonly IReadOnlyList<string> DualFireActions = [EmulationControllerCommandIds.Fire1, EmulationControllerCommandIds.Fire2];
    internal static readonly IReadOnlyList<string> HatariFireActions = [EmulationControllerCommandIds.Fire1, EmulationControllerCommandIds.Turbo];
    internal static readonly IReadOnlyList<string> LynxActions =
        [EmulationControllerCommandIds.Fire1, EmulationControllerCommandIds.Fire2, EmulationControllerCommandIds.Option1, EmulationControllerCommandIds.Option2, EmulationControllerCommandIds.Pause];
    internal static readonly IReadOnlyList<string> KeypadActions =
        [EmulationControllerCommandIds.Start, EmulationControllerCommandIds.Pause, EmulationControllerCommandIds.Reset, EmulationControllerCommandIds.Key0, EmulationControllerCommandIds.Key1, EmulationControllerCommandIds.Key2, EmulationControllerCommandIds.Key3, EmulationControllerCommandIds.Key4, EmulationControllerCommandIds.Key5, EmulationControllerCommandIds.Key6, EmulationControllerCommandIds.Key7, EmulationControllerCommandIds.Key8, EmulationControllerCommandIds.Key9, EmulationControllerCommandIds.Star, EmulationControllerCommandIds.Hash];
    internal static readonly IReadOnlyList<string> JaguarActions =
        [EmulationControllerCommandIds.A, EmulationControllerCommandIds.B, EmulationControllerCommandIds.C, EmulationControllerCommandIds.Option, EmulationControllerCommandIds.Pause, EmulationControllerCommandIds.Key0, EmulationControllerCommandIds.Key1, EmulationControllerCommandIds.Key2, EmulationControllerCommandIds.Key3, EmulationControllerCommandIds.Key4, EmulationControllerCommandIds.Key5, EmulationControllerCommandIds.Key6, EmulationControllerCommandIds.Key7, EmulationControllerCommandIds.Key8, EmulationControllerCommandIds.Key9, EmulationControllerCommandIds.Star, EmulationControllerCommandIds.Hash];
    public const int MinimumDeadZonePercent = 0;
    public const int MaximumDeadZonePercent = 100;
    public const int DefaultDeadZonePercent = 15;
    internal const int MaximumAxisMagnitude = short.MaxValue;
    internal const int PercentageDivisor = 100;
    internal const short NeutralAxis = 0;
    internal const uint TriggerAnalogIndex = 2;
    internal const uint LeftTriggerId = 12;
    internal const uint RightTriggerId = 13;
}

internal static class ControllerPortFunctionsConstants
{
    internal const string Automatic = "Automatic";
    internal const string BoosterGrip = "Booster Grip";
    internal const string Genesis = "Genesis";
    internal const string Joy2B = "Joy 2B+";
}

internal static class KeyboardConstants
{
    internal const uint Backspace = 8;
    internal const uint Tab = 9;
    internal const uint Return = 13;
    internal const uint Escape = 27;
    internal const uint Space = 32;
    internal const uint FirstPrintableCharacter = 32;
    internal const uint LastPrintableCharacter = 126;
    internal const uint Delete = 127;
    internal const uint FirstKeypadDigit = 256;
    internal const uint KeypadPeriod = 266;
    internal const uint KeypadDivide = 267;
    internal const uint KeypadMultiply = 268;
    internal const uint KeypadMinus = 269;
    internal const uint KeypadPlus = 270;
    internal const uint KeypadEnter = 271;
    internal const uint Up = 273;
    internal const uint Down = 274;
    internal const uint Right = 275;
    internal const uint Left = 276;
    internal const uint Insert = 277;
    internal const uint Home = 278;
    internal const uint End = 279;
    internal const uint PageUp = 280;
    internal const uint PageDown = 281;
    internal const uint FirstFunctionKey = 282;
    internal const uint CapsLock = 301;
    internal const uint RightShift = 303;
    internal const uint LeftShift = 304;
    internal const uint RightControl = 305;
    internal const uint LeftControl = 306;
    internal const uint RightAlt = 307;
    internal const uint LeftAlt = 308;
    internal const uint LeftMeta = 311;
    internal const uint RightMeta = 312;
    internal const uint Help = 315;
    internal const uint Break = 318;
    internal const uint Undo = 322;
    internal const uint FirstLetter = 'a';
    internal const uint LastLetter = 'z';
    internal const uint FirstDigit = '0';
    internal const int LetterCount = 26;
    internal const int DigitCount = 10;
    internal const int FunctionKeyCount = 10;
    internal const ushort ShiftModifier = 1;
    internal const ushort ControlModifier = 2;
    internal const ushort AltModifier = 4;
    internal const uint UppercaseOffset = 32;
    internal const uint NoCharacter = 0;
    internal const ushort NoModifiers = 0;
    internal const int FirstRangeOffset = 0;
    internal const int ModifierFirstOrder = 0;
    internal const int ModifierLastOrder = 1;
}

internal static class MouseSettingsConstants
{
    internal const string SpeedOptionKey = MachineOptionConstants.PointerSpeed;
    internal const string MappingOptionPrefix = "gwgui_atari_mouse_";
    internal const int DefaultSpeedPercent = 100;
    internal const int MinimumSpeedPercent = 25;
    internal const int MaximumSpeedPercent = 200;
    internal const int SpeedStepPercent = 25;

    internal static readonly IReadOnlyList<string> Actions = ["Left", "Right"];
}
