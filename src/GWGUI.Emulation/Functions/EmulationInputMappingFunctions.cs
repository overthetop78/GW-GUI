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

    public static IReadOnlySet<EmulationKey> MapKeyboard(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, EmulationKey>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return new HashSet<EmulationKey>(keys);
        var result = new HashSet<EmulationKey>();
        foreach (var key in keys)
        {
            var mapped = mappings.FirstOrDefault(item => item.Value == key);
            result.Add(!string.IsNullOrWhiteSpace(mapped.Key) && Enum.TryParse<EmulationKey>(mapped.Key, true, out var target) ? target : key);
        }
        return result;
    }

    public static string? ParseControllerDeviceId(string? source)
    {
        if (string.IsNullOrWhiteSpace(source) ||
            !source.StartsWith(EmulationInputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase)) return null;
        var controlSeparator = source.LastIndexOf(EmulationInputMappingConstants.SourceSeparator);
        return controlSeparator <= EmulationInputMappingConstants.ControllerPrefix.Length ? null : source[EmulationInputMappingConstants.ControllerPrefix.Length..controlSeparator];
    }

    public static EmulationControllerState ResolveController(string? deviceId, IReadOnlyList<EmulationControllerState> controllers, int fallbackIndex)
    {
        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            var match = controllers.FirstOrDefault(controller => string.Equals(controller.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase));
            return match ?? EmulationControllerState.Empty;
        }
        return fallbackIndex >= 0 && fallbackIndex < controllers.Count ? controllers[fallbackIndex] : EmulationControllerState.Empty;
    }

    public static EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
    {
        var deviceId = ParseControllerDeviceId(source);
        return deviceId is null ? fallback : ResolveController(deviceId, controllers, -1);
    }

    public static float ControllerSourceValue(string source, EmulationControllerState controller)
    {
        var name = SourceControlName(source);
        if (name is EmulationInputMappingConstants.LeftTrigger or EmulationInputMappingConstants.L2
            or EmulationInputMappingConstants.RightTrigger or EmulationInputMappingConstants.R2)
        {
            if (controller.Controls.TryGetValue(name, out var pressure)) return Math.Clamp(pressure, 0f, 1f);
            var trigger = name is EmulationInputMappingConstants.LeftTrigger or EmulationInputMappingConstants.L2 ? controller.LeftTrigger : controller.RightTrigger;
            if (trigger > 0) return (float)trigger / short.MaxValue;
        }
        if (ControllerButtons.TryGetValue(name, out var button))
            return (controller.Buttons & (1u << button)) != 0 ? 1f : 0f;
        if (name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase)
            && controller.Controls.TryGetValue(name[..^EmulationInputMappingConstants.PositiveSuffix.Length], out var positive))
            return IsGenericAxisDirection(name) ? Math.Clamp(positive * 2f - 1f, 0f, 1f) : positive;
        if (name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase)
            && controller.Controls.TryGetValue(name[..^EmulationInputMappingConstants.NegativeSuffix.Length], out var negative))
            return IsGenericAxisDirection(name) ? Math.Clamp(1f - negative * 2f, 0f, 1f) : -negative;
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

    public static bool IsControllerSourcePressed(string source, EmulationControllerState controller) => IsControllerSourcePressed(source, controller, ControllerSourceValue(source, controller));

    public static bool IsControllerSourcePressed(string source, EmulationControllerState controller, float value)
    {
        var name = SourceControlName(source);
        if (IsGenericAxisDirection(name))
            return value > EmulationInputMappingConstants.PressedThreshold;
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

    private static bool IsGenericAxisDirection(string name)
    {
        var suffix = name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase)
            ? EmulationInputMappingConstants.PositiveSuffix
            : name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase) ? EmulationInputMappingConstants.NegativeSuffix : string.Empty;
        var numberLength = name.Length - EmulationInputMappingConstants.AxisPrefix.Length - suffix.Length;
        return suffix.Length > 0 && numberLength > 0 && name.StartsWith(EmulationInputMappingConstants.AxisPrefix, StringComparison.OrdinalIgnoreCase)
            && int.TryParse(name.AsSpan(EmulationInputMappingConstants.AxisPrefix.Length, numberLength), out var index) && index >= 0;
    }

    private static string SourceControlName(string source) => source.Split(EmulationInputMappingConstants.SourceSeparator, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;

    public static short MapControllerAxis(IReadOnlyDictionary<string, string> mappings, string positiveCommand, string negativeCommand,
        EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        var positive = mappings.TryGetValue(positiveCommand, out var positiveSource) ? SourcePressure(positiveSource, controller, physical, keys, mouse) : 0f;
        var negative = mappings.TryGetValue(negativeCommand, out var negativeSource) ? SourcePressure(negativeSource, controller, physical, keys, mouse) : 0f;
        var value = positive - negative;
        return (short)Math.Round(value >= 0f ? value * short.MaxValue : -value * short.MinValue);
    }

    public static short MapControllerPressure(IReadOnlyDictionary<string, string> mappings, string command, EmulationControllerState controller,
        IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse) =>
        mappings.TryGetValue(command, out var source) ? (short)Math.Round(SourcePressure(source, controller, physical, keys, mouse) * short.MaxValue) : (short)0;

    private static float SourcePressure(string source, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical,
        IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        if (string.IsNullOrWhiteSpace(source)) return 0f;
        if (source.StartsWith(EmulationInputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
            return Enum.TryParse<EmulationKey>(source[EmulationInputMappingConstants.KeyboardPrefix.Length..], true, out var key) && keys.Contains(key) ? 1f : 0f;
        if (source.StartsWith(EmulationInputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase))
            return mouse.GetValueOrDefault(source[EmulationInputMappingConstants.MousePrefix.Length..]) ? 1f : 0f;
        var selected = ResolveSourceController(source, physical, controller);
        return ControllerSourcePressure(source, selected);
    }

    public static float ControllerSourcePressure(string source, EmulationControllerState controller)
    {
        var pressure = ControllerSourceValue(source, controller);
        var name = SourceControlName(source);
        var rawStick = !controller.Controls.ContainsKey(name) && name is
            EmulationInputMappingConstants.LeftStickLeft or EmulationInputMappingConstants.LeftStickRight
            or EmulationInputMappingConstants.LeftStickUp or EmulationInputMappingConstants.LeftStickDown
            or EmulationInputMappingConstants.RightStickLeft or EmulationInputMappingConstants.RightStickRight
            or EmulationInputMappingConstants.RightStickUp or EmulationInputMappingConstants.RightStickDown;
        if (rawStick || Math.Abs(pressure) > 1f) pressure /= short.MaxValue;
        return Math.Clamp(pressure, 0f, 1f);
    }

    public static string? OppositeControllerAxisSource(string source)
    {
        if (!source.StartsWith(EmulationInputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase)) return null;
        var name = SourceControlName(source);
        string? opposite = name switch
        {
            EmulationInputMappingConstants.LeftStickLeft => EmulationInputMappingConstants.LeftStickRight,
            EmulationInputMappingConstants.LeftStickRight => EmulationInputMappingConstants.LeftStickLeft,
            EmulationInputMappingConstants.LeftStickUp => EmulationInputMappingConstants.LeftStickDown,
            EmulationInputMappingConstants.LeftStickDown => EmulationInputMappingConstants.LeftStickUp,
            EmulationInputMappingConstants.RightStickLeft => EmulationInputMappingConstants.RightStickRight,
            EmulationInputMappingConstants.RightStickRight => EmulationInputMappingConstants.RightStickLeft,
            EmulationInputMappingConstants.RightStickUp => EmulationInputMappingConstants.RightStickDown,
            EmulationInputMappingConstants.RightStickDown => EmulationInputMappingConstants.RightStickUp,
            _ => null
        };
        if (IsGenericAxisDirection(name))
        {
            if (name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase))
                opposite = name[..^EmulationInputMappingConstants.PositiveSuffix.Length] + EmulationInputMappingConstants.NegativeSuffix;
            else if (name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase))
                opposite = name[..^EmulationInputMappingConstants.NegativeSuffix.Length] + EmulationInputMappingConstants.PositiveSuffix;
        }
        return opposite is null ? null : source[..^name.Length] + opposite;
    }
}
