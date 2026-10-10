namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Constants;

internal static class InputMappingConstants
{
    internal const string KeyboardPrefix = "Keyboard:";
    internal const string MousePrefix = "Mouse:";
    internal const string ControllerPrefix = "Controller:";
    internal const string ResourcePrefix = "Emulation.Epoch.Input.";
    internal const string KeypadPrefix = "Keypad";
    internal const string GunPrefix = "Gun";
    internal const int Unassigned = -1;
    internal const int AxisCount = 4;
    internal const int LeftX = 0;
    internal const int LeftY = 1;
    internal const int RightX = 2;
    internal const int RightY = 3;
    internal const float AxisMaximum = short.MaxValue;
    internal const float PercentMaximum = 100f;
    internal const float PressedThreshold = 0.5f;
}
