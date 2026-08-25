namespace GWGUI.Emulation.Functions;

public static class EmulationInputMappingFunctions
{
    private static readonly IReadOnlyDictionary<string, int> ControllerButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [EmulationInputMappingConstants.B] = EmulationInputMappingConstants.BIndex,
            [EmulationInputMappingConstants.ButtonB] = EmulationInputMappingConstants.BIndex,
            [EmulationInputMappingConstants.Y] = EmulationInputMappingConstants.YIndex,
            [EmulationInputMappingConstants.ButtonY] = EmulationInputMappingConstants.YIndex,
            [EmulationInputMappingConstants.Select] = EmulationInputMappingConstants.SelectIndex,
            [EmulationInputMappingConstants.View] = EmulationInputMappingConstants.SelectIndex,
            [EmulationInputMappingConstants.Start] = EmulationInputMappingConstants.StartIndex,
            [EmulationInputMappingConstants.Menu] = EmulationInputMappingConstants.StartIndex,
            [EmulationInputMappingConstants.Up] = EmulationInputMappingConstants.UpIndex,
            [EmulationInputMappingConstants.DPadUp] = EmulationInputMappingConstants.UpIndex,
            [EmulationInputMappingConstants.Down] = EmulationInputMappingConstants.DownIndex,
            [EmulationInputMappingConstants.DPadDown] = EmulationInputMappingConstants.DownIndex,
            [EmulationInputMappingConstants.Left] = EmulationInputMappingConstants.LeftIndex,
            [EmulationInputMappingConstants.DPadLeft] = EmulationInputMappingConstants.LeftIndex,
            [EmulationInputMappingConstants.Right] = EmulationInputMappingConstants.RightIndex,
            [EmulationInputMappingConstants.DPadRight] = EmulationInputMappingConstants.RightIndex,
            [EmulationInputMappingConstants.A] = EmulationInputMappingConstants.AIndex,
            [EmulationInputMappingConstants.ButtonA] = EmulationInputMappingConstants.AIndex,
            [EmulationInputMappingConstants.X] = EmulationInputMappingConstants.XIndex,
            [EmulationInputMappingConstants.ButtonX] = EmulationInputMappingConstants.XIndex,
            [EmulationInputMappingConstants.L] = EmulationInputMappingConstants.LIndex,
            [EmulationInputMappingConstants.LeftShoulder] = EmulationInputMappingConstants.LIndex,
            [EmulationInputMappingConstants.R] = EmulationInputMappingConstants.RIndex,
            [EmulationInputMappingConstants.RightShoulder] = EmulationInputMappingConstants.RIndex,
            [EmulationInputMappingConstants.L2] = EmulationInputMappingConstants.L2Index,
            [EmulationInputMappingConstants.LeftTrigger] = EmulationInputMappingConstants.L2Index,
            [EmulationInputMappingConstants.R2] = EmulationInputMappingConstants.R2Index,
            [EmulationInputMappingConstants.RightTrigger] = EmulationInputMappingConstants.R2Index,
            [EmulationInputMappingConstants.L3] = EmulationInputMappingConstants.L3Index,
            [EmulationInputMappingConstants.LeftStickClick] = EmulationInputMappingConstants.L3Index,
            [EmulationInputMappingConstants.R3] = EmulationInputMappingConstants.R3Index,
            [EmulationInputMappingConstants.RightStickClick] = EmulationInputMappingConstants.R3Index,
            [EmulationInputMappingConstants.XboxButton] = EmulationInputMappingConstants.GuideIndex,
            [EmulationInputMappingConstants.Guide] = EmulationInputMappingConstants.GuideIndex,
            [EmulationInputMappingConstants.Share] = EmulationInputMappingConstants.ShareIndex,
            [EmulationInputMappingConstants.PaddleLeft1] = EmulationInputMappingConstants.PaddleLeft1Index,
            [EmulationInputMappingConstants.PaddleLeft2] = EmulationInputMappingConstants.PaddleLeft2Index,
            [EmulationInputMappingConstants.PaddleRight1] = EmulationInputMappingConstants.PaddleRight1Index,
            [EmulationInputMappingConstants.PaddleRight2] = EmulationInputMappingConstants.PaddleRight2Index
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

    public static string? ParseControllerDeviceId(string? source)
    {
        if (string.IsNullOrWhiteSpace(source) ||
            !source.StartsWith(EmulationInputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase)) return null;
        var controlSeparator = source.LastIndexOf(EmulationInputMappingConstants.SourceSeparator);
        return controlSeparator <= EmulationInputMappingConstants.ControllerPrefix.Length ? null
            : source[EmulationInputMappingConstants.ControllerPrefix.Length..controlSeparator];
    }

    public static EmulationControllerState ResolveController(string? deviceId,
        IReadOnlyList<EmulationControllerState> controllers, int fallbackIndex)
    {
        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            var match = controllers.FirstOrDefault(controller =>
                string.Equals(controller.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase));
            if (match is not null) return match;
        }
        return fallbackIndex >= 0 && fallbackIndex < controllers.Count
            ? controllers[fallbackIndex] : EmulationControllerState.Empty;
    }

    public static bool IsControllerSourcePressed(string source, EmulationControllerState controller)
    {
        var name = source.Split(EmulationInputMappingConstants.SourceSeparator,
            StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        if (ControllerButtons.TryGetValue(name, out var button))
            return (controller.Buttons & (1u << button)) != 0;
        if (name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase) &&
            controller.Controls.TryGetValue(name[..^EmulationInputMappingConstants.PositiveSuffix.Length],
                out var positive)) return positive > EmulationInputMappingConstants.PositiveThreshold;
        if (name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase) &&
            controller.Controls.TryGetValue(name[..^EmulationInputMappingConstants.NegativeSuffix.Length],
                out var negative)) return negative < EmulationInputMappingConstants.NegativeThreshold;
        if (controller.Controls.TryGetValue(name, out var value))
            return value > EmulationInputMappingConstants.PressedThreshold;
        return name switch
        {
            EmulationInputMappingConstants.LeftStickLeft => controller.LeftX < -EmulationInputMappingConstants.AnalogThreshold,
            EmulationInputMappingConstants.LeftStickRight => controller.LeftX > EmulationInputMappingConstants.AnalogThreshold,
            EmulationInputMappingConstants.LeftStickUp => controller.LeftY < -EmulationInputMappingConstants.AnalogThreshold,
            EmulationInputMappingConstants.LeftStickDown => controller.LeftY > EmulationInputMappingConstants.AnalogThreshold,
            EmulationInputMappingConstants.RightStickLeft => controller.RightX < -EmulationInputMappingConstants.AnalogThreshold,
            EmulationInputMappingConstants.RightStickRight => controller.RightX > EmulationInputMappingConstants.AnalogThreshold,
            EmulationInputMappingConstants.RightStickUp => controller.RightY < -EmulationInputMappingConstants.AnalogThreshold,
            EmulationInputMappingConstants.RightStickDown => controller.RightY > EmulationInputMappingConstants.AnalogThreshold,
            _ => false
        };
    }
}
