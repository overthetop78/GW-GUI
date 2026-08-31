using GWGUI.App.Enums.Input;

namespace GWGUI.App.Contracts.Input;

internal sealed record ControllerVisualState
{
    internal float LeftX { get; init; }
    internal float LeftY { get; init; }
    internal float RightX { get; init; }
    internal float RightY { get; init; }
    internal float LeftTrigger { get; init; }
    internal float RightTrigger { get; init; }
    internal float Wheel { get; init; }
    internal float Throttle { get; init; }
    internal float Brake { get; init; }
    internal float Clutch { get; init; }
    internal float Handbrake { get; init; }
    internal float FlightRoll { get; init; }
    internal float FlightPitch { get; init; }
    internal float FlightYaw { get; init; }
    internal float FlightThrottle { get; init; }
    internal int PatternShifterGear { get; init; }
    internal float ArcadeX { get; init; }
    internal float ArcadeY { get; init; }
    internal bool RawAxesUseNormalizedValues { get; init; }
    internal bool HasGamepadState { get; init; }
    internal bool HasRacingWheelState { get; init; }
    internal bool HasFlightStickState { get; init; }
    internal bool HasArcadeStickState { get; init; }
    internal IReadOnlySet<ControllerVisualControl> PrimarySwitchDirections { get; init; } =
        new HashSet<ControllerVisualControl>();
    internal IReadOnlySet<ControllerVisualControl> StandardControls { get; init; } =
        new HashSet<ControllerVisualControl>();
    internal IReadOnlySet<ControllerVisualControl> LabeledControls { get; init; } =
        new HashSet<ControllerVisualControl>();
    internal IReadOnlySet<int> ActiveRawButtons { get; init; } = new HashSet<int>();
    internal IReadOnlyDictionary<int, float> RawAxisValues { get; init; } =
        new Dictionary<int, float>();
    internal IReadOnlyDictionary<string, float> EmulatedCommandValues { get; init; } =
        new Dictionary<string, float>(StringComparer.Ordinal);

    internal bool IsStandardActive(ControllerVisualControl control) =>
        StandardControls.Contains(control);

    internal bool IsLabeledActive(ControllerVisualControl control) =>
        LabeledControls.Contains(control);

    internal bool IsRawButtonActive(int index) => ActiveRawButtons.Contains(index);

    internal float RawAxisSigned(int index)
    {
        if (!RawAxisValues.TryGetValue(index, out var value)) return 0f;
        return RawAxesUseNormalizedValues
            ? Math.Clamp(value * 2f - 1f, -1f, 1f)
            : Math.Clamp(value, -1f, 1f);
    }

    internal float RawAxisUnsigned(int index, float defaultValue = 0f) =>
        RawAxisValues.TryGetValue(index, out var value)
            ? Math.Clamp(value, 0f, 1f)
            : defaultValue;

    internal float EmulatedCommandValue(string commandId) =>
        EmulatedCommandValues.TryGetValue(commandId, out var value) ? value : 0f;
}
