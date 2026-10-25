namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Constants;

internal static class ControllerActionConstants
{
    internal const string Up = "Up", Down = "Down", Left = "Left", Right = "Right";
    internal const string FireLeft = "FireLeft", FireRight = "FireRight", Purple = "Purple", Blue = "Blue";
    internal const string Keypad0 = "Keypad0", Keypad1 = "Keypad1", Keypad2 = "Keypad2", Keypad3 = "Keypad3";
    internal const string Keypad4 = "Keypad4", Keypad5 = "Keypad5", Keypad6 = "Keypad6", Keypad7 = "Keypad7";
    internal const string Keypad8 = "Keypad8", Keypad9 = "Keypad9", Star = "Star", Pound = "Pound";
    internal const string SpinnerLeft = "SpinnerLeft", SpinnerRight = "SpinnerRight";
    internal static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Up, Down, Left, Right, FireLeft, FireRight, Purple, Blue,
        Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9,
        Star, Pound, SpinnerLeft, SpinnerRight
    };
}
