namespace GWGUI.App.Controls;

using GWGUI.Emulation;

internal static class AtariInputSettingsConstants
{
    internal const string KeyboardTabResource = "Emulation.KeyboardTab";
    internal const string MouseTabResource = "Emulation.MouseTab";
    internal const string ControllersTabResource = "Emulation.ControllersTab";
    internal const string AtariKeyResource = "Emulation.EmulatedKey";
    internal const string SearchKeyResource = "Emulation.SearchBinding";
    internal const string MouseSpeedResource = "Emulation.MouseSpeed";
    internal const string CaptureMouseResource = "Emulation.CaptureMouse";
    internal const string ReleaseMouseResource = "Emulation.ReleaseMouseKey";
    internal const string DetectControllersResource = "Emulation.DetectControllers";
    internal const string NoControllerResource = "Emulation.NoControllersDetected";
    internal const string ControllerTypeResource = "Emulation.ControllerType";
    internal const string ControllerDeviceResource = "Emulation.ControllerDevice";
    internal const string DeadZoneResource = "Emulation.AnalogMouseDeadzone";
    internal const string MouseSpeedOptionKey = "gwgui_atari_mouse_speed";
    internal const string MouseMappingOptionPrefix = "gwgui_atari_mouse_";
    internal const int DefaultMouseSpeedPercent = 100;
    internal const int MinimumMouseSpeedPercent = 25;
    internal const int MaximumMouseSpeedPercent = 200;
    internal const int MouseSpeedStepPercent = 25;
    internal const int InclusiveEndpointCount = 1;
    internal const int FirstPort = 0;
    internal const int NoControllerCount = 0;
    internal const string XInputDevicePrefix = "XInput ";
    internal static readonly IReadOnlyList<string> StandardControllerActions =
        ["Up", "Down", "Left", "Right", "Fire1", "Fire2", "Turbo"];
    internal static readonly IReadOnlyList<string> KeypadControllerActions =
        ["Start", "Pause", "Reset", "Key0", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7", "Key8", "Key9", "Star", "Hash"];
    internal static readonly IReadOnlyList<string> JaguarControllerActions =
        ["A", "B", "C", "Option", "Pause", "Key0", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7", "Key8", "Key9", "Star", "Hash"];
    internal static readonly IReadOnlyList<string> MouseActions = ["Left", "Right"];
    internal static readonly IReadOnlyList<EmulationKey> FunctionKeys =
        [EmulationKey.F1, EmulationKey.F2, EmulationKey.F3, EmulationKey.F4, EmulationKey.F5,
         EmulationKey.F6, EmulationKey.F7, EmulationKey.F8, EmulationKey.F9, EmulationKey.F10];
    internal static readonly IReadOnlyList<EmulationKey> ComputerSpecialKeys =
        [EmulationKey.Help, EmulationKey.AtariUndo, EmulationKey.AtariBreak];
    internal static readonly IReadOnlyList<EmulationKey> Atari800SpecialKeys =
        [EmulationKey.AtariOption, EmulationKey.AtariSelect, EmulationKey.AtariStart, EmulationKey.Help, EmulationKey.AtariBreak];
    internal static readonly IReadOnlyDictionary<EmulationKey, EmulationKey> SpecialKeyDefaults =
        new Dictionary<EmulationKey, EmulationKey>
        {
            [EmulationKey.Help] = EmulationKey.Insert,
            [EmulationKey.AtariUndo] = EmulationKey.Home,
            [EmulationKey.AtariBreak] = EmulationKey.End,
            [EmulationKey.AtariOption] = EmulationKey.F2,
            [EmulationKey.AtariSelect] = EmulationKey.F3,
            [EmulationKey.AtariStart] = EmulationKey.F1
        };
}
