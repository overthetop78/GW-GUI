using GWGUI.Emulation.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Constants;

internal static class InputConstants
{
    internal const uint JoypadDevice = 1;
    internal const uint AnalogDevice = 5;
    internal const uint MouseDevice = 2;
    internal const uint KeyboardDevice = 3;
    internal const uint PrimaryPort = 0;
    internal const uint MouseXId = 0;
    internal const uint MouseYId = 1;
    internal const uint MouseLeftId = 2;
    internal const uint MouseRightId = 3;
    internal const uint MouseWheelUpId = 4;
    internal const uint MouseWheelDownId = 5;
    internal const uint MouseMiddleId = 6;
    internal const uint JoypadMaskId = 256;
    internal const uint LeftAnalogIndex = 0;
    internal const uint RightAnalogIndex = 1;
    internal const uint AnalogXId = 0;
    internal const uint AnalogYId = 1;
    internal const uint MaximumJoypadButtonCount = 32;
    internal const uint JoypadSelectId = 2;
    internal const uint JoypadStartId = 3;
    internal const uint JoypadLeftShoulderId = 10;
    internal const uint JoypadRightTriggerId = 13;
    internal const short ActiveState = 1;
    internal const short InactiveState = 0;
    internal const int ConsumedRelativeValue = 0;
}

internal static class InputSettingsConstants
{
    internal static readonly IReadOnlyList<EmulationKey> FunctionKeys =
        [EmulationKey.F1, EmulationKey.F2, EmulationKey.F3, EmulationKey.F4, EmulationKey.F5,
         EmulationKey.F6, EmulationKey.F7, EmulationKey.F8, EmulationKey.F9, EmulationKey.F10];

    internal static readonly IReadOnlyList<EmulationKey> ComputerSpecialKeys =
        [EmulationKey.Help, EmulationKey.Undo, EmulationKey.Break];

    internal static readonly IReadOnlyList<EmulationKey> Atari800SpecialKeys =
        [EmulationKey.AtariOption, EmulationKey.AtariSelect, EmulationKey.AtariStart,
         EmulationKey.Help, EmulationKey.Break];

    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> DefaultKeys =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.Help] = EmulationKey.Insert,
            [EmulationKey.Undo] = EmulationKey.Home,
            [EmulationKey.Break] = EmulationKey.End,
            [EmulationKey.AtariOption] = EmulationKey.F2,
            [EmulationKey.AtariSelect] = EmulationKey.F3,
            [EmulationKey.AtariStart] = EmulationKey.F1
        };
}

internal static class InputSettingsFunctionsConstants
{
    internal const string Left = EmulationControllerCommandIds.Left;
    internal const string ResourceMouseButtonLeft = "Emulation.Mouse.Button.Left";
    internal const string MouseLeft = "Mouse:Left";
    internal const string Right = EmulationControllerCommandIds.Right;
    internal const string ResourceMouseButtonRight = "Emulation.Mouse.Button.Right";
    internal const string MouseRight = "Mouse:Right";
    internal const string Fire1 = EmulationControllerCommandIds.Fire1;
    internal const string Fire2 = EmulationControllerCommandIds.Fire2;
    internal const string Turbo = EmulationControllerCommandIds.Turbo;
    internal const string ResourceControllerNone = "Emulation.Controller.None";
    internal const string ResourceControllerAutomatic = "Emulation.Controller.Automatic";
    internal const string ResourceAtariControllerJoystick = "Emulation.Atari.Controller.Joystick";
    internal const string ResourceAtariControllerAtari5200 = "Emulation.Atari.Controller.Atari5200";
    internal const string ResourceControllerAnalogJoystick = "Emulation.Controller.AnalogJoystick";
    internal const string ResourceAtariControllerPaddleControllers = "Emulation.Atari.Controller.PaddleControllers";
    internal const string ResourceAtariControllerXg1LightGun = "Emulation.Atari.Controller.Xg1LightGun";
    internal const string ResourceAtariControllerNumericKeypad = "Emulation.Atari.Controller.NumericKeypad";
    internal const string ResourceAtariControllerDriving = "Emulation.Atari.Controller.Driving";
    internal const string ResourceAtariControllerProLine = "Emulation.Atari.Controller.ProLine";
    internal const string ResourceAtariControllerBoosterGrip = "Emulation.Atari.Controller.BoosterGrip";
    internal const string ResourceAtariControllerGenesis = "Emulation.Atari.Controller.Genesis";
    internal const string ResourceAtariControllerJoy2BPlus = "Emulation.Atari.Controller.Joy2BPlus";
    internal const string ResourceAtariControllerLynx = "Emulation.Atari.Controller.Lynx";
    internal const string ResourceAtariControllerJaguar = "Emulation.Atari.Controller.Jaguar";
    internal const string ResourceControllerActionTurboFire = "Emulation.Controller.Action.TurboFire";
    internal const string Up = EmulationControllerCommandIds.Up;
    internal const string Down = EmulationControllerCommandIds.Down;
    internal const string Option1 = EmulationControllerCommandIds.Option1;
    internal const string Option12 = "Option 1";
    internal const string Option2 = EmulationControllerCommandIds.Option2;
    internal const string Option22 = "Option 2";
    internal const string ResourceKeyHelp = "Emulation.Key.Help";
    internal const string ResourceKeyUndo = "Emulation.Key.Undo";
    internal const string ResourceKeyBreak = "Emulation.Key.Break";
}

internal static class InputSnapshotFunctionsConstants
{
    internal const string Fire1 = "Fire1";
    internal const string Fire2 = "Fire2";
    internal const string Turbo = "Turbo";
    internal const string Up = "Up";
    internal const string Down = "Down";
    internal const string Left = "Left";
    internal const string Right = "Right";
    internal const string A = "A";
    internal const string B = "B";
    internal const string C = "C";
    internal const string Pause = "Pause";
    internal const string Option = "Option";
    internal const string Option1 = "Option1";
    internal const string Option2 = "Option2";
    internal const string Start = "Start";
    internal const string Reset = "Reset";
    internal const string Key0 = "Key0";
    internal const string Key1 = "Key1";
    internal const string Key2 = "Key2";
    internal const string Key3 = "Key3";
    internal const string Key4 = "Key4";
    internal const string Key5 = "Key5";
    internal const string Key6 = "Key6";
    internal const string Star = "Star";
    internal const string Hash = "Hash";
    internal const string Keyboard = "Keyboard:";
    internal const string Mouse = "Mouse:";
    internal const string Middle = "Middle";
    internal const string XButton1 = "XButton1";
    internal const string XButton2 = "XButton2";
    internal const string WheelUp = "WheelUp";
    internal const string WheelDown = "WheelDown";
    internal const string WheelLeft = "WheelLeft";
    internal const string WheelRight = "WheelRight";
    internal const string Key7 = "Key7";
}
