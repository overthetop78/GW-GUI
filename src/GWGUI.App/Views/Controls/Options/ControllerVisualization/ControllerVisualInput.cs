using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Services.Input.GameInput;
using static GWGUI.App.Views.Controls.Options.ControllerVisualization.ControllerVisualControlMappings;
using static GWGUI.App.Constants.Input.ControllerVisualization.ControllerVisualInputConstants;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal sealed class ControllerVisualInput
{
    internal ControllerVisualInput(GameInputLiveState? state)
        : this(ControllerVisualStateConverter.Convert(state))
    {
    }

    internal ControllerVisualInput(ControllerVisualState? state)
    {
        State = state ?? new ControllerVisualState();
    }

    internal ControllerVisualState State { get; }

    internal float LeftX => State.LeftX;
    internal float LeftY => State.LeftY;
    internal float RightX => State.RightX;
    internal float RightY => State.RightY;
    internal float LeftTrigger => State.LeftTrigger;
    internal float RightTrigger => State.RightTrigger;
    internal float Wheel => State.Wheel;
    internal float Throttle => State.Throttle;
    internal float Brake => State.Brake;
    internal float Clutch => State.Clutch;
    internal float Handbrake => State.Handbrake;
    internal float FlightRoll => State.FlightRoll;
    internal float FlightPitch => State.FlightPitch;
    internal float FlightYaw => State.FlightYaw;
    internal float FlightThrottle => State.FlightThrottle;
    internal int PatternShifterGear => State.PatternShifterGear;
    internal float ArcadeX => State.ArcadeX;
    internal float ArcadeY => State.ArcadeY;

    internal bool RacingButton(GameInputRacingWheelButtons button, int fallbackIndex) =>
        State.HasRacingWheelState
            ? State.IsStandardActive(RacingControl(button))
            : RawButton(fallbackIndex);

    internal bool FlightButton(GameInputFlightStickButtons button, int fallbackIndex) =>
        State.HasFlightStickState
            ? State.IsStandardActive(FlightControl(button))
            : RawButton(fallbackIndex);

    internal bool FlightHat(GameInputSwitchPosition position)
    {
        var control = FlightHatControl(position);
        if (State.HasFlightStickState)
            return control is not null && State.IsStandardActive(control.Value);
        if (MatchesPrimarySwitch(position)) return true;

        var horizontal = RawAxisSigned(FlightHatHorizontalAxisIndex);
        var vertical = RawAxisSigned(FlightHatVerticalAxisIndex);
        return MatchesAxes(position, horizontal, vertical);
    }

    internal bool ArcadeButton(GameInputArcadeStickButtons button, int fallbackIndex) =>
        State.HasArcadeStickState
            ? State.IsStandardActive(ArcadeControl(button))
            : RawButton(fallbackIndex);

    internal bool Button(GameInputGamepadButtons button, int fallbackIndex)
    {
        var control = GamepadControl(button);
        if (control is not null && State.IsStandardActive(control.Value)) return true;
        if (control is not null && State.IsLabeledActive(control.Value)) return true;
        return RawButton(fallbackIndex);
    }

    internal bool TriggerPressed(bool left) =>
        (left ? LeftTrigger : RightTrigger) > TriggerPressedThreshold ||
        Button(
            left ? GameInputGamepadButtons.LeftTriggerButton : GameInputGamepadButtons.RightTriggerButton,
            left ? LeftTriggerButtonIndex : RightTriggerButtonIndex);

    internal bool SystemButton(GameInputSystemButtons button)
    {
        var control = SystemControl(button);
        return control is not null && State.IsStandardActive(control.Value);
    }

    internal bool LabelButton(params GameInputLabel[] labels) =>
        labels.Any(label =>
        {
            var control = LabelControl(label);
            return control is not null && State.IsLabeledActive(control.Value);
        });

    internal bool RawButton(int index) => State.IsRawButtonActive(index);

    internal bool Direction(
        GameInputSwitchPosition position,
        GameInputGamepadButtons button,
        int fallbackIndex) =>
        Direction(position, button, fallbackIndex, GamepadLeftHorizontalAxisIndex, GamepadLeftVerticalAxisIndex);

    internal bool Direction(
        GameInputSwitchPosition position,
        GameInputGamepadButtons button,
        int fallbackIndex,
        int horizontalAxis,
        int verticalAxis)
    {
        var control = GamepadControl(button);
        if (State.HasGamepadState)
            return control is not null && State.IsStandardActive(control.Value);
        if (Button(button, fallbackIndex)) return true;
        if (MatchesPrimarySwitch(position)) return true;
        return MatchesAxes(position, RawAxisSigned(horizontalAxis), RawAxisSigned(verticalAxis));
    }

    internal float RawAxisUnsigned(int index, float defaultValue = 0f) =>
        State.RawAxisUnsigned(index, defaultValue);

    internal float RawAxisSigned(int index) => State.RawAxisSigned(index);

    private bool MatchesPrimarySwitch(GameInputSwitchPosition position)
    {
        var control = DirectionControl(position);
        return control is not null && State.PrimarySwitchDirections.Contains(control.Value);
    }

    private static bool MatchesAxes(
        GameInputSwitchPosition position,
        float horizontal,
        float vertical) =>
        position switch
        {
            GameInputSwitchPosition.Up => vertical < -DirectionPressedThreshold,
            GameInputSwitchPosition.Down => vertical > DirectionPressedThreshold,
            GameInputSwitchPosition.Left => horizontal < -DirectionPressedThreshold,
            GameInputSwitchPosition.Right => horizontal > DirectionPressedThreshold,
            _ => false
        };

}

internal readonly record struct ControllerVisualSnapshot(
    float LeftX,
    float LeftY,
    float RightX,
    float RightY,
    float LeftTrigger,
    float RightTrigger,
    float Wheel,
    float Throttle,
    float Brake,
    float Clutch,
    bool PrimaryPressed,
    bool DPadUpPressed);
