namespace GWGUI.Emulation;

public static class EmulationInputMappingFunctions
{
    private const short AnalogThreshold = 14000;

    private static readonly IReadOnlyDictionary<string, int> ControllerButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["B"] = 0, ["ButtonB"] = 0,
            ["Y"] = 1, ["ButtonY"] = 1,
            ["Select"] = 2, ["View"] = 2,
            ["Start"] = 3, ["Menu"] = 3,
            ["Up"] = 4, ["DPadUp"] = 4,
            ["Down"] = 5, ["DPadDown"] = 5,
            ["Left"] = 6, ["DPadLeft"] = 6,
            ["Right"] = 7, ["DPadRight"] = 7,
            ["A"] = 8, ["ButtonA"] = 8,
            ["X"] = 9, ["ButtonX"] = 9,
            ["L"] = 10, ["LeftShoulder"] = 10,
            ["R"] = 11, ["RightShoulder"] = 11,
            ["L2"] = 12, ["LeftTrigger"] = 12,
            ["R2"] = 13, ["RightTrigger"] = 13,
            ["L3"] = 14, ["LeftStickClick"] = 14,
            ["R3"] = 15, ["RightStickClick"] = 15,
            ["XboxButton"] = 16
        };

    public static IReadOnlySet<EmulationKey> MapKeyboard(IReadOnlySet<EmulationKey> keys,
        IReadOnlyDictionary<string, EmulationKey>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return new HashSet<EmulationKey>(keys);
        var result = new HashSet<EmulationKey>();
        foreach (var key in keys)
        {
            var mapped = mappings.FirstOrDefault(item => item.Value == key);
            result.Add(!string.IsNullOrWhiteSpace(mapped.Key)
                && Enum.TryParse<EmulationKey>(mapped.Key, true, out var target) ? target : key);
        }
        return result;
    }

    public static int ParseControllerPort(string? deviceId, int fallback) =>
        deviceId?.StartsWith("xinput:", StringComparison.OrdinalIgnoreCase) == true
        && int.TryParse(deviceId[7..], out var port) && port is >= 0 and < 4 ? port : fallback;

    public static bool IsControllerSourcePressed(string source, EmulationControllerState controller)
    {
        var name = source.Split(':', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        if (ControllerButtons.TryGetValue(name, out var button))
            return (controller.Buttons & (1u << button)) != 0;
        return name switch
        {
            "LeftStickLeft" => controller.LeftX < -AnalogThreshold,
            "LeftStickRight" => controller.LeftX > AnalogThreshold,
            "LeftStickUp" => controller.LeftY < -AnalogThreshold,
            "LeftStickDown" => controller.LeftY > AnalogThreshold,
            "RightStickLeft" => controller.RightX < -AnalogThreshold,
            "RightStickRight" => controller.RightX > AnalogThreshold,
            "RightStickUp" => controller.RightY < -AnalogThreshold,
            "RightStickDown" => controller.RightY > AnalogThreshold,
            _ => false
        };
    }
}
