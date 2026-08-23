namespace GWGUI.App.Services.Input.GameInput;

[Flags]
internal enum GameInputKind : uint
{
    Unknown = 0x00000000,
    RawDeviceReport = 0x00000001,
    ControllerAxis = 0x00000002,
    ControllerButton = 0x00000004,
    ControllerSwitch = 0x00000008,
    Controller = ControllerAxis | ControllerButton | ControllerSwitch,
    Keyboard = 0x00000010,
    Mouse = 0x00000020,
    Sensors = 0x00000040,
    ArcadeStick = 0x00010000,
    FlightStick = 0x00020000,
    Gamepad = 0x00040000,
    RacingWheel = 0x00080000,
    // Still reported by the Windows GameInput runtime although it is absent
    // from the current redistributable header.
    UiNavigation = 0x01000000
}

internal enum GameInputEnumerationKind { None, Async, Blocking }

[Flags]
internal enum GameInputFocusPolicy : uint
{
    Default = 0,
    ExclusiveForegroundInput = 0x00000002,
    ExclusiveForegroundGuideButton = 0x00000008,
    ExclusiveForegroundShareButton = 0x00000020,
    EnableBackgroundInput = 0x00000040,
    EnableBackgroundGuideButton = 0x00000080,
    EnableBackgroundShareButton = 0x00000100
}

internal enum GameInputSwitchKind { Unknown = -1, TwoWay, FourWay, EightWay }
internal enum GameInputSwitchPosition { Center, Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft }
internal enum GameInputKeyboardKind { Unknown = -1, Ansi, Iso, Ks, Abnt, Jis }

[Flags]
internal enum GameInputMouseButtons : uint
{
    None = 0, Left = 1, Right = 2, Middle = 4, Button4 = 8, Button5 = 16,
    WheelTiltLeft = 32, WheelTiltRight = 64
}

[Flags]
internal enum GameInputMousePositions : uint { None = 0, Absolute = 1, Relative = 2 }

[Flags]
internal enum GameInputSensorsKind : uint
{
    None = 0, Accelerometer = 1, Gyrometer = 2, Compass = 4, Orientation = 8
}

internal enum GameInputSensorAccuracy : uint { Unknown, Unreliable, Approximate, High }

[Flags]
internal enum GameInputArcadeStickButtons : uint
{
    None = 0, Menu = 1, View = 2, Up = 4, Down = 8, Left = 0x10, Right = 0x20,
    Action1 = 0x40, Action2 = 0x80, Action3 = 0x100, Action4 = 0x200,
    Action5 = 0x400, Action6 = 0x800, Special1 = 0x1000, Special2 = 0x2000
}

[Flags]
internal enum GameInputFlightStickButtons : uint
{
    None = 0, Menu = 1, View = 2, FirePrimary = 4, FireSecondary = 8,
    HatSwitchUp = 0x10, HatSwitchDown = 0x20, HatSwitchLeft = 0x40, HatSwitchRight = 0x80,
    A = 0x100, B = 0x200, X = 0x400, Y = 0x800,
    LeftShoulder = 0x1000, RightShoulder = 0x2000
}

[Flags]
internal enum GameInputGamepadButtons : uint
{
    None = 0, Menu = 0x1, View = 0x2, A = 0x4, B = 0x8, X = 0x10, Y = 0x20,
    DPadUp = 0x40, DPadDown = 0x80, DPadLeft = 0x100, DPadRight = 0x200,
    LeftShoulder = 0x400, RightShoulder = 0x800,
    LeftThumbstick = 0x1000, RightThumbstick = 0x2000, C = 0x4000, Z = 0x8000,
    LeftTriggerButton = 0x10000, RightTriggerButton = 0x20000,
    LeftThumbstickUp = 0x00040000, LeftThumbstickDown = 0x00080000,
    LeftThumbstickLeft = 0x00100000, LeftThumbstickRight = 0x00200000,
    RightThumbstickUp = 0x00400000, RightThumbstickDown = 0x00800000,
    RightThumbstickLeft = 0x01000000, RightThumbstickRight = 0x02000000,
    PaddleLeft1 = 0x04000000, PaddleLeft2 = 0x08000000,
    PaddleRight1 = 0x10000000, PaddleRight2 = 0x20000000
}

internal enum GameInputRawDeviceReportKind : uint { Input, Output }

[Flags]
internal enum GameInputRacingWheelButtons : uint
{
    None = 0, Menu = 1, View = 2, PreviousGear = 4, NextGear = 8,
    DPadUp = 0x10, DPadDown = 0x20, DPadLeft = 0x40, DPadRight = 0x80,
    A = 0x100, B = 0x200, X = 0x400, Y = 0x800,
    LeftThumbstick = 0x1000, RightThumbstick = 0x2000
}

[Flags]
internal enum GameInputSystemButtons : uint { None = 0, Guide = 1, Share = 2 }

[Flags]
internal enum GameInputFlightStickAxes : uint { None = 0, Roll = 0x10, Pitch = 0x20, Yaw = 0x40, Throttle = 0x80 }

[Flags]
internal enum GameInputGamepadAxes : uint
{
    None = 0, LeftTrigger = 1, RightTrigger = 2, LeftThumbstickX = 4,
    LeftThumbstickY = 8, RightThumbstickX = 0x10, RightThumbstickY = 0x20
}

[Flags]
internal enum GameInputRacingWheelAxes : uint
{
    None = 0, Steering = 0x100, Throttle = 0x200, Brake = 0x400,
    Clutch = 0x800, Handbrake = 0x1000, PatternShifter = 0x2000
}

[Flags]
internal enum GameInputDeviceStatus : uint
{
    None = 0, Connected = 1, HapticInfoReady = 0x00200000, Any = 0xFFFFFFFF
}

internal enum GameInputDeviceFamily { Virtual = -1, Unknown, XboxOne, Xbox360, Hid, I8042, Aggregate }

internal enum GameInputLabel
{
    Unknown = -1, None = 0, XboxGuide = 1, XboxBack = 2, XboxStart = 3,
    XboxMenu = 4, XboxView = 5, XboxA = 7, XboxB = 8, XboxX = 9, XboxY = 10,
    XboxDPadUp = 11, XboxDPadDown = 12, XboxDPadLeft = 13, XboxDPadRight = 14,
    XboxLeftShoulder = 15, XboxLeftTrigger = 16, XboxLeftStickButton = 17,
    XboxRightShoulder = 18, XboxRightTrigger = 19, XboxRightStickButton = 20,
    XboxPaddle1 = 21, XboxPaddle2 = 22, XboxPaddle3 = 23, XboxPaddle4 = 24,
    LetterA = 25, LetterB, LetterC, LetterD, LetterE, LetterF, LetterG, LetterH,
    LetterI, LetterJ, LetterK, LetterL, LetterM, LetterN, LetterO, LetterP,
    LetterQ, LetterR, LetterS, LetterT, LetterU, LetterV, LetterW, LetterX,
    LetterY, LetterZ, Number0, Number1, Number2, Number3, Number4, Number5,
    Number6, Number7, Number8, Number9, ArrowUp, ArrowUpRight, ArrowRight,
    ArrowDownRight, ArrowDown, ArrowDownLeft, ArrowLeft, ArrowUpLeft,
    ArrowUpDown, ArrowLeftRight, ArrowUpDownLeftRight, ArrowClockwise,
    ArrowCounterClockwise, ArrowReturn, IconBranding, IconHome, IconMenu,
    IconCross, IconCircle, IconSquare, IconTriangle, IconStar, IconDPadUp,
    IconDPadDown, IconDPadLeft, IconDPadRight, IconDialClockwise,
    IconDialCounterClockwise, IconSliderLeftRight, IconSliderUpDown,
    IconWheelUpDown, IconPlus, IconMinus, IconSuspension, Home, Guide, Mode,
    Select, Menu, View, Back, Start, Options, Share, Up, Down, Left, Right,
    LB, LT, LSB, L1, L2, L3, RB, RT, RSB, R1, R2, R3,
    PaddleLeft1, PaddleLeft2, PaddleRight1, PaddleRight2
}

[Flags]
internal enum GameInputFeedbackAxes : uint
{
    None = 0, LinearX = 1, LinearY = 2, LinearZ = 4, AngularX = 8,
    AngularY = 0x10, AngularZ = 0x20, Normal = 0x40
}

internal enum GameInputFeedbackEffectState { Stopped, Running, Paused }
internal enum GameInputForceFeedbackEffectKind
{
    Constant, Ramp, SineWave, SquareWave, TriangleWave, SawtoothUpWave,
    SawtoothDownWave, Spring, Friction, Damper, Inertia
}

[Flags]
internal enum GameInputRumbleMotors : uint
{
    None = 0, LowFrequency = 1, HighFrequency = 2, LeftTrigger = 4, RightTrigger = 8
}

internal enum GameInputElementKind { None, Axis, Button, Switch }
