namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Constants;

internal static class InputMappingConstants
{
    internal const int ConsumedRelativeValue = 0;
    internal const string KeyboardPrefix = "Keyboard:";
    internal const string MousePrefix = "Mouse:";
    internal const string ControllerPrefix = "Controller:";
    internal const string ResourcePrefix = "Emulation.Sharp.Input.";
    internal const string KeyboardResourcePrefix = "Emulation.Sharp.Keyboard.";
    internal const int Unassigned = -1;
    internal const float PressedThreshold = 0.5f;
    internal const int DefaultMouseSpeedPercent = 100;
    internal const int MouseSpeedStepPercent = 10;
    internal const int PercentScale = 100;
    internal const int MinimumMouseSpeedPercent = 10;
    internal const int MaximumMouseSpeedPercent = 1000;
    internal const string MouseLeft = "Left";
    internal const string MouseRight = "Right";
    internal const string MouseLeftResource = "Emulation.Mouse.Button.Left";
    internal const string MouseRightResource = "Emulation.Mouse.Button.Right";
}
