namespace GWGUI.App.Constants.Input.GameInput;

internal static class GameInputControllerMappingConstants
{
    internal const float TriggerPressedThreshold = 0.12f;
    internal const string ButtonControlPrefix = "Button";
    internal const string AxisControlPrefix = "Axis";
    internal const string SwitchControlPrefix = "Switch";
    internal const int CardinalDirectionCount = 4;
    internal const int SwitchDirectionCount = 8;
    internal const int FirstSwitchDirection = 1;
}

internal enum EmulationControllerButtonBit
{
    B,
    Y,
    View,
    Menu,
    DPadUp,
    DPadDown,
    DPadLeft,
    DPadRight,
    A,
    X,
    LeftShoulder,
    RightShoulder,
    LeftTrigger,
    RightTrigger,
    LeftThumbstick,
    RightThumbstick,
    Guide,
    Share,
    PaddleLeft1,
    PaddleLeft2,
    PaddleRight1,
    PaddleRight2
}
