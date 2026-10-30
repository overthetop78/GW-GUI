using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Services.Input.GameInput;
using static GWGUI.App.Views.Controls.Options.ControllerVisualization.ControllerVisualControlMappings;
using static GWGUI.App.Constants.Input.ControllerVisualization.ControllerVisualInputConstants;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static class ControllerVisualStateConverter
{
    internal static ControllerVisualState Convert(GameInputLiveState? source)
    {
        if (source is null) return new ControllerVisualState();

        var rawAxes = source.Controls.Where(control => control.Type == GameInputControlType.Axis).GroupBy(control => control.Index).ToDictionary(group => group.Key, group => group.Last().Value);
        var rawButtons = source.Controls.Where(control => control.Type == GameInputControlType.Button && control.IsPressed).Select(control => control.Index).ToHashSet();
        var labeled = source.Controls.Where(control => control.Type == GameInputControlType.Button && control.IsPressed).Select(control => LabelControl(control.Label)).Where(control => control is not null).Select(control => control!.Value) .ToHashSet();
        var standard = new HashSet<ControllerVisualControl>();

        var gamepad = source.Gamepad;
        if (gamepad is { } gamepadValue)
            AddGamepadControls(standard, gamepadValue.Buttons);

        var racing = source.RacingWheel;
        if (racing is { } racingValue)
            AddRacingControls(standard, racingValue.Buttons);

        var flight = source.FlightStick;
        if (flight is { } flightValue)
        {
            AddFlightControls(standard, flightValue.Buttons);
            AddDirectionControls(standard, flightValue.HatSwitch, ControllerVisualControl.FlightHatUp, ControllerVisualControl.FlightHatDown, ControllerVisualControl.FlightHatLeft, ControllerVisualControl.FlightHatRight);
        }

        var arcade = source.ArcadeStick;
        if (arcade is { } arcadeValue)
            AddArcadeControls(standard, arcadeValue.Buttons);

        if (source.SystemButtons.HasFlag(GameInputSystemButtons.Guide))
            standard.Add(ControllerVisualControl.SystemGuide);
        if (source.SystemButtons.HasFlag(GameInputSystemButtons.Share))
            standard.Add(ControllerVisualControl.SystemShare);

        var primarySwitchDirections = new HashSet<ControllerVisualControl>();
        var primarySwitch = source.Controls.FirstOrDefault(control => control.Type == GameInputControlType.Switch);
        if (primarySwitch is not null)
            AddDirectionControls( primarySwitchDirections, primarySwitch.SwitchPosition, ControllerVisualControl.GamepadDPadUp, ControllerVisualControl.GamepadDPadDown, ControllerVisualControl.GamepadDPadLeft, ControllerVisualControl.GamepadDPadRight);

        float RawSigned(int index)
        {
            if (!rawAxes.TryGetValue(index, out var value)) return NeutralAxisValue;
            return source.ControlsUseNormalizedAxes ? Math.Clamp(value * NormalizedAxisScale - MaximumAxisValue, MinimumSignedAxisValue, MaximumAxisValue) : Math.Clamp(value, MinimumSignedAxisValue, MaximumAxisValue);
        }

        float RawUnsigned(int index, float defaultValue = NeutralAxisValue) => rawAxes.TryGetValue(index, out var value) ? Math.Clamp(value, NeutralAxisValue, MaximumAxisValue) : defaultValue;

        float GamepadAxis(Func<GameInputGamepadState, float> selector, GameInputGamepadButtons negative, GameInputGamepadButtons positive, int rawIndex)
        {
            if (gamepad is not { } value) return RawSigned(rawIndex);
            var analog = Math.Clamp(selector(value), MinimumSignedAxisValue, MaximumAxisValue);
            if (Math.Abs(analog) > AnalogInputThreshold) return analog;
            return DigitalAxis(value.Buttons.HasFlag(negative), value.Buttons.HasFlag(positive));
        }

        float Trigger(GameInputLabel label, bool left)
        {
            if (gamepad is { } value)
            {
                var analog = Math.Clamp(left ? value.LeftTrigger : value.RightTrigger, NeutralAxisValue, MaximumAxisValue);
                if (analog > AnalogInputThreshold) return analog;
                var flag = left ? GameInputGamepadButtons.LeftTriggerButton : GameInputGamepadButtons.RightTriggerButton;
                if (value.Buttons.HasFlag(flag)) return MaximumAxisValue;
            }

            var labeledAxis = source.Controls.FirstOrDefault(control => control.Type == GameInputControlType.Axis && control.Label == label);
            var axisValue = Math.Clamp(labeledAxis?.Value ?? NeutralAxisValue, NeutralAxisValue, MaximumAxisValue);
            if (axisValue > AnalogInputThreshold) return axisValue;
            var labeledControl = left ? ControllerVisualControl.GamepadLeftTriggerLabel : ControllerVisualControl.GamepadRightTriggerLabel;
            return labeled.Contains(labeledControl) ? MaximumAxisValue : NeutralAxisValue;
        }

        var leftX = GamepadAxis(value => value.LeftThumbstickX, GameInputGamepadButtons.LeftThumbstickLeft, GameInputGamepadButtons.LeftThumbstickRight, GamepadLeftHorizontalAxisIndex);
        var leftY = gamepad is null ? RawSigned(GamepadLeftVerticalAxisIndex) : -GamepadAxis(value => value.LeftThumbstickY, GameInputGamepadButtons.LeftThumbstickDown, GameInputGamepadButtons.LeftThumbstickUp, GamepadLeftVerticalAxisIndex);
        var rightX = GamepadAxis(value => value.RightThumbstickX, GameInputGamepadButtons.RightThumbstickLeft, GameInputGamepadButtons.RightThumbstickRight, GamepadRightHorizontalAxisIndex);
        var rightY = gamepad is null ? RawSigned(GamepadRightVerticalAxisIndex) : -GamepadAxis(value => value.RightThumbstickY, GameInputGamepadButtons.RightThumbstickDown, GameInputGamepadButtons.RightThumbstickUp, GamepadRightVerticalAxisIndex);
        var arcadeX = arcade is { } currentArcade ? DigitalAxis(currentArcade.Buttons.HasFlag(GameInputArcadeStickButtons.Left), currentArcade.Buttons.HasFlag(GameInputArcadeStickButtons.Right)) : leftX;
        var arcadeY = arcade is { } currentArcadeY ? DigitalAxis(currentArcadeY.Buttons.HasFlag(GameInputArcadeStickButtons.Up), currentArcadeY.Buttons.HasFlag(GameInputArcadeStickButtons.Down)) : leftY;

        return new ControllerVisualState
        {
            LeftX = leftX,
            LeftY = leftY,
            RightX = rightX,
            RightY = rightY,
            LeftTrigger = Trigger(GameInputLabel.XboxLeftTrigger, left: true),
            RightTrigger = Trigger(GameInputLabel.XboxRightTrigger, left: false),
            Wheel = racing?.Wheel ?? RawSigned(RacingWheelAxisIndex),
            Throttle = racing?.Throttle ?? RawUnsigned(RacingThrottleAxisIndex),
            Brake = racing?.Brake ?? RawUnsigned(RacingBrakeAxisIndex),
            Clutch = racing?.Clutch ?? RawUnsigned(RacingClutchAxisIndex),
            Handbrake = racing?.Handbrake ?? RawUnsigned(RacingHandbrakeAxisIndex),
            FlightRoll = flight?.Roll ?? RawSigned(FlightRollAxisIndex),
            FlightPitch = flight?.Pitch ?? RawSigned(FlightPitchAxisIndex),
            FlightYaw = flight?.Yaw ?? RawSigned(FlightYawAxisIndex),
            FlightThrottle = flight?.Throttle ?? RawUnsigned(FlightThrottleAxisIndex),
            PatternShifterGear = racing?.PatternShifterGear ?? 0,
            ArcadeX = arcadeX,
            ArcadeY = arcadeY,
            RawAxesUseNormalizedValues = source.ControlsUseNormalizedAxes,
            HasGamepadState = gamepad is not null,
            HasRacingWheelState = racing is not null,
            HasFlightStickState = flight is not null,
            HasArcadeStickState = arcade is not null,
            PrimarySwitchDirections = primarySwitchDirections,
            StandardControls = standard,
            LabeledControls = labeled,
            ActiveRawButtons = rawButtons,
            RawAxisValues = rawAxes
        };
    }

    private static void AddGamepadControls(ISet<ControllerVisualControl> target, GameInputGamepadButtons buttons) =>
        AddControls(target, buttons, ControllerVisualControlMappings.GamepadMappings);

    private static void AddRacingControls(ISet<ControllerVisualControl> target, GameInputRacingWheelButtons buttons) =>
        AddControls(target, buttons, ControllerVisualControlMappings.RacingMappings);

    private static void AddFlightControls(ISet<ControllerVisualControl> target, GameInputFlightStickButtons buttons) =>
        AddControls(target, buttons, ControllerVisualControlMappings.FlightMappings);

    private static void AddArcadeControls(ISet<ControllerVisualControl> target, GameInputArcadeStickButtons buttons) =>
        AddControls(target, buttons, ControllerVisualControlMappings.ArcadeMappings);

    private static void AddDirectionControls(ISet<ControllerVisualControl> target, GameInputSwitchPosition position, ControllerVisualControl up, ControllerVisualControl down, ControllerVisualControl left, ControllerVisualControl right)
    {
        if (position is GameInputSwitchPosition.Up or GameInputSwitchPosition.UpLeft or GameInputSwitchPosition.UpRight)
            target.Add(up);
        if (position is GameInputSwitchPosition.Down or GameInputSwitchPosition.DownLeft or GameInputSwitchPosition.DownRight)
            target.Add(down);
        if (position is GameInputSwitchPosition.Left or GameInputSwitchPosition.UpLeft or GameInputSwitchPosition.DownLeft)
            target.Add(left);
        if (position is GameInputSwitchPosition.Right or GameInputSwitchPosition.UpRight or GameInputSwitchPosition.DownRight)
            target.Add(right);
    }

    private static float DigitalAxis(bool negative, bool positive) => negative == positive ? NeutralAxisValue : negative ? MinimumSignedAxisValue : MaximumAxisValue;

    private static void AddControls<TButtons>(ISet<ControllerVisualControl> target, TButtons buttons,
        IReadOnlyList<(TButtons Button, ControllerVisualControl Control)> mappings)
        where TButtons : struct, Enum
    {
        foreach (var mapping in mappings)
            Add(target, buttons, mapping.Button, mapping.Control);
    }

    private static void Add<TButtons>(ISet<ControllerVisualControl> target, TButtons buttons, TButtons flag, ControllerVisualControl control)
        where TButtons : struct, Enum
    {
        var value = System.Convert.ToUInt64(buttons);
        var mask = System.Convert.ToUInt64(flag);
        if ((value & mask) != 0) target.Add(control);
    }
}
