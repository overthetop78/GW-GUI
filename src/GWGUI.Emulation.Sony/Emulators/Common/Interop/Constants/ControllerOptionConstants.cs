namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class ControllerOptionConstants
{
    internal static IReadOnlyDictionary<string, (string Disabled, string Port1, string Port2, string Both)> MultitapModes { get; } =
        new Dictionary<string, (string, string, string, string)>(StringComparer.Ordinal)
        {
            [Emulators.SwanStation.Constants.OptionConstants.ControllerPortsMultitapModeKey] = ("Disabled", "Port1Only", "Port2Only", "BothPorts"),
            [Emulators.PcsxRearmed.Constants.OptionConstants.MultitapKey] = ("disabled", "port 1", "port 2", "ports 1 and 2"),
            [Emulators.Pcee2.Constants.OptionConstants.MultitapKey] = ("disabled", "port1", "port2", "both")
        };
    internal static IReadOnlyDictionary<string, string> LabelsBySuffix { get; } = new Dictionary<string, string>
    {
        ["_ForceAnalog"] = "Emulation.Option.Sony.Controller.ForceAnalog",
        ["_AnalogDPadInDigitalMode"] = "Emulation.Option.Sony.Controller.AnalogDirections",
        ["_AxisScale"] = "Emulation.Option.Sony.Controller.AxisScale",
        ["_VibrationBias"] = "Emulation.Option.Sony.Controller.VibrationBias",
        [XScaleSuffix] = "Emulation.Option.Sony.Controller.XScale",
        [YScaleSuffix] = "Emulation.Option.Sony.Controller.YScale",
        [SteeringDeadzoneSuffix] = "Emulation.Option.Sony.Controller.SteeringDeadzone",
        [TwistResponseSuffix] = "Emulation.Option.Sony.Controller.TwistResponse"
    };
    internal const string Vibration = "vibration";
    internal const string Axis = "axis";
    internal const string XScaleSuffix = "_XScale";
    internal const string YScaleSuffix = "_YScale";
    internal const string SteeringDeadzoneSuffix = "_SteeringDeadzone";
    internal const string TwistResponseSuffix = "_TwistResponse";
    internal const string AnalogCalibrationHelp = "Emulation.Option.Sony.Controller.AnalogCalibration.Short";
    internal static bool IsAnalogCalibration(string key) => key == Emulators.BeetlePsx.Constants.OptionConstants.AnalogCalibrationKey || key == Emulators.BeetlePsxHw.Constants.OptionConstants.AnalogCalibrationKey;
    internal const string Multitap = "multitap";
    internal const string Deadzone = "deadzone";
    internal const string PortOneSuffix = "_p1";
    internal const string PortTwoSuffix = "_p2";
    internal const string NeGcon = "negcon";
    internal const string GunCon = "guncon";
    internal const string KonamiGun = "konamigun";
    internal const string LightGun = "lightgun";
    internal const string Gun = "gun";
    internal const string Crosshair = "crosshair";
    internal const string Mouse = "mouse";
    internal const string Analog = "analog";
    internal const string Rumble = "rumble";
    internal const string SwanPortPrefix = "swanstation_Controller";
    internal const string PlayerOne = "crosshair1";
    internal const string PlayerTwo = "crosshair2";
}
