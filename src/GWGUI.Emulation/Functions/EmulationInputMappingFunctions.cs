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

    public static float ControllerSourceValue(string source, EmulationControllerState controller)
    {
        var name = SourceControlName(source);
        if (ControllerButtons.TryGetValue(name, out var button))
            return (controller.Buttons & (1u << button)) != 0 ? 1f : 0f;
        if (name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase)
            && controller.Controls.TryGetValue(
                name[..^EmulationInputMappingConstants.PositiveSuffix.Length], out var positive))
            return positive;
        if (name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase)
            && controller.Controls.TryGetValue(
                name[..^EmulationInputMappingConstants.NegativeSuffix.Length], out var negative))
            return -negative;
        if (controller.Controls.TryGetValue(name, out var value)) return value;
        return name switch
        {
            EmulationInputMappingConstants.LeftStickLeft => -controller.LeftX,
            EmulationInputMappingConstants.LeftStickRight => controller.LeftX,
            EmulationInputMappingConstants.LeftStickUp => -controller.LeftY,
            EmulationInputMappingConstants.LeftStickDown => controller.LeftY,
            EmulationInputMappingConstants.RightStickLeft => -controller.RightX,
            EmulationInputMappingConstants.RightStickRight => controller.RightX,
            EmulationInputMappingConstants.RightStickUp => -controller.RightY,
            EmulationInputMappingConstants.RightStickDown => controller.RightY,
            _ => 0f
        };
    }

    public static bool IsControllerSourcePressed(string source, EmulationControllerState controller) =>
        IsControllerSourcePressed(source, controller, ControllerSourceValue(source, controller));

    public static bool IsControllerSourcePressed(
        string source,
        EmulationControllerState controller,
        float value)
    {
        var name = SourceControlName(source);
        if (ControllerButtons.ContainsKey(name))
            return value > EmulationInputMappingConstants.PressedThreshold;
        if (name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase))
            return value > EmulationInputMappingConstants.PositiveThreshold;
        if (name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase))
            return value > -EmulationInputMappingConstants.NegativeThreshold;
        if (controller.Controls.ContainsKey(name))
            return value > EmulationInputMappingConstants.PressedThreshold;
        return value > EmulationInputMappingConstants.AnalogThreshold;
    }

    private static string SourceControlName(string source) =>
        source.Split(EmulationInputMappingConstants.SourceSeparator,
            StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
}
