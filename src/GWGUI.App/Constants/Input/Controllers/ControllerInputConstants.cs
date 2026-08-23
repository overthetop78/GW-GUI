namespace GWGUI.App.Constants.Input.Controllers;

public static class ControllerInputConstants
{
    public const short AnalogThreshold = 14000;

    public static readonly string[] LegacyButtonNames =
        ["B", "Y", "Select", "Start", "Up", "Down", "Left", "Right", "A", "X", "L", "R", "L2", "R2", "L3", "R3"];

    public static readonly string[] ModernButtonSources =
    [
        "Controller:ButtonB", "Controller:ButtonY", "Controller:View", "Controller:Menu",
        "Controller:DPadUp", "Controller:DPadDown", "Controller:DPadLeft", "Controller:DPadRight",
        "Controller:ButtonA", "Controller:ButtonX", "Controller:LeftShoulder", "Controller:RightShoulder",
        "Controller:LeftTrigger", "Controller:RightTrigger", "Controller:LeftStickClick", "Controller:RightStickClick",
        "Controller:XboxButton", "Controller:Share",
        "Controller:PaddleLeft1", "Controller:PaddleLeft2",
        "Controller:PaddleRight1", "Controller:PaddleRight2"
    ];
}
