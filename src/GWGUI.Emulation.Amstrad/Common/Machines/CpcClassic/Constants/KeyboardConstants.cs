using GWGUI.Emulation;

namespace GWGUI.Emulation.Amstrad.Common.Machines.CpcClassic.Constants;

internal static class KeyboardConstants
{
    internal const string Escape = "ESC";
    internal const string Tab = "Tab";
    internal const string Delete = "DEL";
    internal const string Clear = "CLR";
    internal const string CapsLock = "CAPS LOCK";
    internal const string Up = "Up";
    internal const string Down = "Down";
    internal const string Left = "Left";
    internal const string Right = "Right";
    internal const string Copy = "COPY";
    internal const string F0 = "F0";
    internal const string F1 = "F1";
    internal const string F2 = "F2";
    internal const string F3 = "F3";
    internal const string F4 = "F4";
    internal const string F5 = "F5";
    internal const string F6 = "F6";
    internal const string F7 = "F7";
    internal const string F8 = "F8";
    internal const string F9 = "F9";
    internal const string FPeriod = "F.";
    internal const string Enter = "ENTER";

    internal static readonly IReadOnlyList<EmulationKey> SpecialKeys =
    [
        EmulationKey.Escape, EmulationKey.Tab, EmulationKey.Backspace, EmulationKey.Delete,
        EmulationKey.CapsLock, EmulationKey.Up, EmulationKey.Down, EmulationKey.Left,
        EmulationKey.Right, EmulationKey.RightAlt, EmulationKey.Numpad0, EmulationKey.Numpad1,
        EmulationKey.Numpad2, EmulationKey.Numpad3, EmulationKey.Numpad4, EmulationKey.Numpad5,
        EmulationKey.Numpad6, EmulationKey.Numpad7, EmulationKey.Numpad8, EmulationKey.Numpad9,
        EmulationKey.NumpadPeriod, EmulationKey.NumpadEnter
    ];
}
