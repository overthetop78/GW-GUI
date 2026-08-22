using GWGUI.App.Constants.Input.Bindings;
using GWGUI.App.Constants.Input.Controllers;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Input.Controllers;

public static class ControllerInputMap
{
    public static bool IsModernSourcePressed(string source, EmulationControllerState controller)
    {
        var segments = source.Split(':', StringSplitOptions.RemoveEmptyEntries);
        source = segments[^1];
        var button = source switch
        {
            "ButtonB" => 0, "ButtonY" => 1, "View" => 2, "Menu" => 3,
            "DPadUp" => 4, "DPadDown" => 5, "DPadLeft" => 6, "DPadRight" => 7,
            "ButtonA" => 8, "ButtonX" => 9, "LeftShoulder" => 10, "RightShoulder" => 11,
            "LeftTrigger" => 12, "RightTrigger" => 13,
            "LeftStickClick" => 14, "RightStickClick" => 15, "XboxButton" => 16,
            _ => -1
        };
        if (button >= 0) return (controller.Buttons & (1u << button)) != 0;
        return source switch
        {
            "LeftStickLeft" => controller.LeftX < -ControllerInputConstants.AnalogThreshold,
            "LeftStickRight" => controller.LeftX > ControllerInputConstants.AnalogThreshold,
            "LeftStickUp" => controller.LeftY < -ControllerInputConstants.AnalogThreshold,
            "LeftStickDown" => controller.LeftY > ControllerInputConstants.AnalogThreshold,
            "RightStickLeft" => controller.RightX < -ControllerInputConstants.AnalogThreshold,
            "RightStickRight" => controller.RightX > ControllerInputConstants.AnalogThreshold,
            "RightStickUp" => controller.RightY < -ControllerInputConstants.AnalogThreshold,
            "RightStickDown" => controller.RightY > ControllerInputConstants.AnalogThreshold,
            _ => false
        };
    }

    public static EmulationControllerState ControllerForSource(string source,
        IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
    {
        if (!source.StartsWith(InputBindingSyntaxConstants.XInputPrefix, StringComparison.OrdinalIgnoreCase)) return fallback;
        var segments = source.Split(':', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 4 && int.TryParse(segments[2], out var port) && port >= 0 && port < controllers.Count
            ? controllers[port] : fallback;
    }

    public static int ParseXInputPort(string? deviceId, int fallback) =>
        deviceId?.StartsWith("xinput:", StringComparison.OrdinalIgnoreCase) == true
        && int.TryParse(deviceId[7..], out var port) && port is >= 0 and < 4 ? port : fallback;
}
