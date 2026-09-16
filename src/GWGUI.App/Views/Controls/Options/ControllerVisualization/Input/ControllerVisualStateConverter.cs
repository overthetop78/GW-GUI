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

        var rawAxes = source.Controls
            .Where(control => control.Type == GameInputControlType.Axis)
            .GroupBy(control => control.Index)
            .ToDictionary(group => group.Key, group => group.Last().Value);
        var rawButtons = source.Controls
            .Where(control => control.Type == GameInputControlType.Button && control.IsPressed)
            .Select(control => control.Index)
            .ToHashSet();
        var labeled = source.Controls
            .Where(control => control.Type == GameInputControlType.Button && control.IsPressed)
            .Select(control => LabelControl(control.Label))
            .Where(control => control is not null)
            .Select(control => control!.Value)
            .ToHashSet();
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
            AddDirectionControls(
                standard,
                flightValue.HatSwitch,
                ControllerVisualControl.FlightHatUp,
                ControllerVisualControl.FlightHatDown,
                ControllerVisualControl.FlightHatLeft,
                ControllerVisualControl.FlightHatRight);
        }

        var arcade = source.ArcadeStick;
        if (arcade is { } arcadeValue)
            AddArcadeControls(standard, arcadeValue.Buttons);

        if (source.SystemButtons.HasFlag(GameInputSystemButtons.Guide))
            standard.Add(ControllerVisualControl.SystemGuide);
        if (source.SystemButtons.HasFlag(GameInputSystemButtons.Share))
            standard.Add(ControllerVisualControl.SystemShare);

        var primarySwitchDirections = new HashSet<ControllerVisualControl>();
        var primarySwitch = source.Controls.FirstOrDefault(
            control => control.Type == GameInputControlType.Switch);
        if (primarySwitch is not null)
            AddDirectionControls(
                primarySwitchDirections,
                primarySwitch.SwitchPosition,
                ControllerVisualControl.GamepadDPadUp,
                ControllerVisualControl.GamepadDPadDown,
                ControllerVisualControl.GamepadDPadLeft,
                ControllerVisualControl.GamepadDPadRight);

        float RawSigned(int index)
        {
            if (!rawAxes.TryGetValue(index, out var value)) return NeutralAxisValue;
            return source.ControlsUseNormalizedAxes
                ? Math.Clamp(
                    value * NormalizedAxisScale - MaximumAxisValue,
                    MinimumSignedAxisValue,
                    MaximumAxisValue)
                : Math.Clamp(value, MinimumSignedAxisValue, MaximumAxisValue);
        }

        float RawUnsigned(int index, float defaultValue = NeutralAxisValue) =>
            rawAxes.TryGetValue(index, out var value)
                ? Math.Clamp(value, NeutralAxisValue, MaximumAxisValue)
                : defaultValue;

        float GamepadAxis(
            Func<GameInputGamepadState, float> selector,
            GameInputGamepadButtons negative,
            GameInputGamepadButtons positive,
            int rawIndex)
        {
            if (gamepad is not { } value) return RawSigned(rawIndex);
            var analog = Math.Clamp(selector(value), MinimumSignedAxisValue, MaximumAxisValue);
            if (Math.Abs(analog) > AnalogInputThreshold) return analog;
            return DigitalAxis(
                value.Buttons.HasFlag(negative),
                value.Buttons.HasFlag(positive));
        }

        float Trigger(GameInputLabel label, bool left)
        {
            if (gamepad is { } value)
            {
                var analog = Math.Clamp(
                    left ? value.LeftTrigger : value.RightTrigger,
                    NeutralAxisValue,
                    MaximumAxisValue);
                if (analog > AnalogInputThreshold) return analog;
                var flag = left
                    ? GameInputGamepadButtons.LeftTriggerButton
                    : GameInputGamepadButtons.RightTriggerButton;
                if (value.Buttons.HasFlag(flag)) return MaximumAxisValue;
            }

            var labeledAxis = source.Controls.FirstOrDefault(control =>
                control.Type == GameInputControlType.Axis && control.Label == label);
            var axisValue = Math.Clamp(
                labeledAxis?.Value ?? NeutralAxisValue,
                NeutralAxisValue,
                MaximumAxisValue);
            if (axisValue > AnalogInputThreshold) return axisValue;
            var labeledControl = left
                ? ControllerVisualControl.GamepadLeftTriggerLabel
                : ControllerVisualControl.GamepadRightTriggerLabel;
            return labeled.Contains(labeledControl) ? MaximumAxisValue : NeutralAxisValue;
        }

        var leftX = GamepadAxis(
            value => value.LeftThumbstickX,
            GameInputGamepadButtons.LeftThumbstickLeft,
            GameInputGamepadButtons.LeftThumbstickRight,
            GamepadLeftHorizontalAxisIndex);
        var leftY = gamepad is null
            ? RawSigned(GamepadLeftVerticalAxisIndex)
            : -GamepadAxis(
                value => value.LeftThumbstickY,
                GameInputGamepadButtons.LeftThumbstickDown,
                GameInputGamepadButtons.LeftThumbstickUp,
                GamepadLeftVerticalAxisIndex);
        var rightX = GamepadAxis(
            value => value.RightThumbstickX,
            GameInputGamepadButtons.RightThumbstickLeft,
            GameInputGamepadButtons.RightThumbstickRight,
            GamepadRightHorizontalAxisIndex);
        var rightY = gamepad is null
            ? RawSigned(GamepadRightVerticalAxisIndex)
            : -GamepadAxis(
                value => value.RightThumbstickY,
                GameInputGamepadButtons.RightThumbstickDown,
                GameInputGamepadButtons.RightThumbstickUp,
                GamepadRightVerticalAxisIndex);
        var arcadeX = arcade is { } currentArcade
            ? DigitalAxis(
                currentArcade.Buttons.HasFlag(GameInputArcadeStickButtons.Left),
                currentArcade.Buttons.HasFlag(GameInputArcadeStickButtons.Right))
            : leftX;
        var arcadeY = arcade is { } currentArcadeY
            ? DigitalAxis(
                currentArcadeY.Buttons.HasFlag(GameInputArcadeStickButtons.Up),
                currentArcadeY.Buttons.HasFlag(GameInputArcadeStickButtons.Down))
            : leftY;

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

    private static void AddGamepadControls(
        ISet<ControllerVisualControl> target,
        GameInputGamepadButtons buttons)
    {
        Add(target, buttons, GameInputGamepadButtons.A, ControllerVisualControl.GamepadA);
        Add(target, buttons, GameInputGamepadButtons.B, ControllerVisualControl.GamepadB);
        Add(target, buttons, GameInputGamepadButtons.C, ControllerVisualControl.GamepadC);
        Add(target, buttons, GameInputGamepadButtons.X, ControllerVisualControl.GamepadX);
        Add(target, buttons, GameInputGamepadButtons.Y, ControllerVisualControl.GamepadY);
        Add(target, buttons, GameInputGamepadButtons.Z, ControllerVisualControl.GamepadZ);
        Add(target, buttons, GameInputGamepadButtons.DPadUp, ControllerVisualControl.GamepadDPadUp);
        Add(target, buttons, GameInputGamepadButtons.DPadDown, ControllerVisualControl.GamepadDPadDown);
        Add(target, buttons, GameInputGamepadButtons.DPadLeft, ControllerVisualControl.GamepadDPadLeft);
        Add(target, buttons, GameInputGamepadButtons.DPadRight, ControllerVisualControl.GamepadDPadRight);
        Add(target, buttons, GameInputGamepadButtons.LeftShoulder, ControllerVisualControl.GamepadLeftShoulder);
        Add(target, buttons, GameInputGamepadButtons.RightShoulder, ControllerVisualControl.GamepadRightShoulder);
        Add(target, buttons, GameInputGamepadButtons.LeftThumbstick, ControllerVisualControl.GamepadLeftThumbstick);
        Add(target, buttons, GameInputGamepadButtons.RightThumbstick, ControllerVisualControl.GamepadRightThumbstick);
        Add(target, buttons, GameInputGamepadButtons.LeftTriggerButton, ControllerVisualControl.GamepadLeftTriggerButton);
        Add(target, buttons, GameInputGamepadButtons.RightTriggerButton, ControllerVisualControl.GamepadRightTriggerButton);
        Add(target, buttons, GameInputGamepadButtons.View, ControllerVisualControl.GamepadView);
        Add(target, buttons, GameInputGamepadButtons.Menu, ControllerVisualControl.GamepadMenu);
        Add(target, buttons, GameInputGamepadButtons.PaddleLeft1, ControllerVisualControl.GamepadPaddleLeft1);
        Add(target, buttons, GameInputGamepadButtons.PaddleLeft2, ControllerVisualControl.GamepadPaddleLeft2);
        Add(target, buttons, GameInputGamepadButtons.PaddleRight1, ControllerVisualControl.GamepadPaddleRight1);
        Add(target, buttons, GameInputGamepadButtons.PaddleRight2, ControllerVisualControl.GamepadPaddleRight2);
    }

    private static void AddRacingControls(
        ISet<ControllerVisualControl> target,
        GameInputRacingWheelButtons buttons)
    {
        Add(target, buttons, GameInputRacingWheelButtons.A, ControllerVisualControl.RacingA);
        Add(target, buttons, GameInputRacingWheelButtons.B, ControllerVisualControl.RacingB);
        Add(target, buttons, GameInputRacingWheelButtons.X, ControllerVisualControl.RacingX);
        Add(target, buttons, GameInputRacingWheelButtons.Y, ControllerVisualControl.RacingY);
        Add(target, buttons, GameInputRacingWheelButtons.DPadUp, ControllerVisualControl.RacingDPadUp);
        Add(target, buttons, GameInputRacingWheelButtons.DPadDown, ControllerVisualControl.RacingDPadDown);
        Add(target, buttons, GameInputRacingWheelButtons.DPadLeft, ControllerVisualControl.RacingDPadLeft);
        Add(target, buttons, GameInputRacingWheelButtons.DPadRight, ControllerVisualControl.RacingDPadRight);
        Add(target, buttons, GameInputRacingWheelButtons.PreviousGear, ControllerVisualControl.RacingPreviousGear);
        Add(target, buttons, GameInputRacingWheelButtons.NextGear, ControllerVisualControl.RacingNextGear);
        Add(target, buttons, GameInputRacingWheelButtons.View, ControllerVisualControl.RacingView);
        Add(target, buttons, GameInputRacingWheelButtons.Menu, ControllerVisualControl.RacingMenu);
        Add(target, buttons, GameInputRacingWheelButtons.LeftThumbstick, ControllerVisualControl.RacingLeftThumbstick);
        Add(target, buttons, GameInputRacingWheelButtons.RightThumbstick, ControllerVisualControl.RacingRightThumbstick);
    }

    private static void AddFlightControls(
        ISet<ControllerVisualControl> target,
        GameInputFlightStickButtons buttons)
    {
        Add(target, buttons, GameInputFlightStickButtons.A, ControllerVisualControl.FlightA);
        Add(target, buttons, GameInputFlightStickButtons.B, ControllerVisualControl.FlightB);
        Add(target, buttons, GameInputFlightStickButtons.X, ControllerVisualControl.FlightX);
        Add(target, buttons, GameInputFlightStickButtons.Y, ControllerVisualControl.FlightY);
        Add(target, buttons, GameInputFlightStickButtons.FirePrimary, ControllerVisualControl.FlightFirePrimary);
        Add(target, buttons, GameInputFlightStickButtons.FireSecondary, ControllerVisualControl.FlightFireSecondary);
        Add(target, buttons, GameInputFlightStickButtons.LeftShoulder, ControllerVisualControl.FlightLeftShoulder);
        Add(target, buttons, GameInputFlightStickButtons.RightShoulder, ControllerVisualControl.FlightRightShoulder);
        Add(target, buttons, GameInputFlightStickButtons.View, ControllerVisualControl.FlightView);
        Add(target, buttons, GameInputFlightStickButtons.Menu, ControllerVisualControl.FlightMenu);
        Add(target, buttons, GameInputFlightStickButtons.HatSwitchUp, ControllerVisualControl.FlightHatUp);
        Add(target, buttons, GameInputFlightStickButtons.HatSwitchDown, ControllerVisualControl.FlightHatDown);
        Add(target, buttons, GameInputFlightStickButtons.HatSwitchLeft, ControllerVisualControl.FlightHatLeft);
        Add(target, buttons, GameInputFlightStickButtons.HatSwitchRight, ControllerVisualControl.FlightHatRight);
    }

    private static void AddArcadeControls(
        ISet<ControllerVisualControl> target,
        GameInputArcadeStickButtons buttons)
    {
        Add(target, buttons, GameInputArcadeStickButtons.Action1, ControllerVisualControl.ArcadeAction1);
        Add(target, buttons, GameInputArcadeStickButtons.Action2, ControllerVisualControl.ArcadeAction2);
        Add(target, buttons, GameInputArcadeStickButtons.Action3, ControllerVisualControl.ArcadeAction3);
        Add(target, buttons, GameInputArcadeStickButtons.Action4, ControllerVisualControl.ArcadeAction4);
        Add(target, buttons, GameInputArcadeStickButtons.Action5, ControllerVisualControl.ArcadeAction5);
        Add(target, buttons, GameInputArcadeStickButtons.Action6, ControllerVisualControl.ArcadeAction6);
        Add(target, buttons, GameInputArcadeStickButtons.Special1, ControllerVisualControl.ArcadeSpecial1);
        Add(target, buttons, GameInputArcadeStickButtons.Special2, ControllerVisualControl.ArcadeSpecial2);
        Add(target, buttons, GameInputArcadeStickButtons.View, ControllerVisualControl.ArcadeView);
        Add(target, buttons, GameInputArcadeStickButtons.Menu, ControllerVisualControl.ArcadeMenu);
    }

    private static void AddDirectionControls(
        ISet<ControllerVisualControl> target,
        GameInputSwitchPosition position,
        ControllerVisualControl up,
        ControllerVisualControl down,
        ControllerVisualControl left,
        ControllerVisualControl right)
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

    private static float DigitalAxis(bool negative, bool positive) =>
        negative == positive
            ? NeutralAxisValue
            : negative
                ? MinimumSignedAxisValue
                : MaximumAxisValue;

    private static void Add<TButtons>(
        ISet<ControllerVisualControl> target,
        TButtons buttons,
        TButtons flag,
        ControllerVisualControl control)
        where TButtons : struct, Enum
    {
        var value = System.Convert.ToUInt64(buttons);
        var mask = System.Convert.ToUInt64(flag);
        if ((value & mask) != 0) target.Add(control);
    }
}
