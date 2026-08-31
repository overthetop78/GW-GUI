using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Input;
using GWGUI.App.Services.Input.GameInput;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal sealed class ControllerVisualInput
{
    internal ControllerVisualInput(GameInputLiveState? state)
        : this(Convert(state))
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

        var horizontal = RawAxisSigned(4);
        var vertical = RawAxisSigned(5);
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
        (left ? LeftTrigger : RightTrigger) > .04f ||
        Button(
            left ? GameInputGamepadButtons.LeftTriggerButton : GameInputGamepadButtons.RightTriggerButton,
            left ? 12 : 13);

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
        Direction(position, button, fallbackIndex, 0, 1);

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
            GameInputSwitchPosition.Up => vertical < -.5f,
            GameInputSwitchPosition.Down => vertical > .5f,
            GameInputSwitchPosition.Left => horizontal < -.5f,
            GameInputSwitchPosition.Right => horizontal > .5f,
            _ => false
        };

    private static ControllerVisualState Convert(GameInputLiveState? source)
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
            if (!rawAxes.TryGetValue(index, out var value)) return 0f;
            return source.ControlsUseNormalizedAxes
                ? Math.Clamp(value * 2f - 1f, -1f, 1f)
                : Math.Clamp(value, -1f, 1f);
        }

        float RawUnsigned(int index, float defaultValue = 0f) =>
            rawAxes.TryGetValue(index, out var value)
                ? Math.Clamp(value, 0f, 1f)
                : defaultValue;

        float GamepadAxis(
            Func<GameInputGamepadState, float> selector,
            GameInputGamepadButtons negative,
            GameInputGamepadButtons positive,
            int rawIndex)
        {
            if (gamepad is not { } value) return RawSigned(rawIndex);
            var analog = Math.Clamp(selector(value), -1f, 1f);
            if (Math.Abs(analog) > .0001f) return analog;
            return DigitalAxis(
                value.Buttons.HasFlag(negative),
                value.Buttons.HasFlag(positive));
        }

        float Trigger(GameInputLabel label, bool left)
        {
            if (gamepad is { } value)
            {
                var analog = Math.Clamp(left ? value.LeftTrigger : value.RightTrigger, 0f, 1f);
                if (analog > .0001f) return analog;
                var flag = left
                    ? GameInputGamepadButtons.LeftTriggerButton
                    : GameInputGamepadButtons.RightTriggerButton;
                if (value.Buttons.HasFlag(flag)) return 1f;
            }

            var labeledAxis = source.Controls.FirstOrDefault(control =>
                control.Type == GameInputControlType.Axis && control.Label == label);
            var axisValue = Math.Clamp(labeledAxis?.Value ?? 0f, 0f, 1f);
            if (axisValue > .0001f) return axisValue;
            var labeledControl = left
                ? ControllerVisualControl.GamepadLeftTriggerLabel
                : ControllerVisualControl.GamepadRightTriggerLabel;
            return labeled.Contains(labeledControl) ? 1f : 0f;
        }

        var leftX = GamepadAxis(
            value => value.LeftThumbstickX,
            GameInputGamepadButtons.LeftThumbstickLeft,
            GameInputGamepadButtons.LeftThumbstickRight,
            0);
        var leftY = gamepad is null
            ? RawSigned(1)
            : -GamepadAxis(
                value => value.LeftThumbstickY,
                GameInputGamepadButtons.LeftThumbstickDown,
                GameInputGamepadButtons.LeftThumbstickUp,
                1);
        var rightX = GamepadAxis(
            value => value.RightThumbstickX,
            GameInputGamepadButtons.RightThumbstickLeft,
            GameInputGamepadButtons.RightThumbstickRight,
            2);
        var rightY = gamepad is null
            ? RawSigned(3)
            : -GamepadAxis(
                value => value.RightThumbstickY,
                GameInputGamepadButtons.RightThumbstickDown,
                GameInputGamepadButtons.RightThumbstickUp,
                3);
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
            Wheel = racing?.Wheel ?? RawSigned(0),
            Throttle = racing?.Throttle ?? RawUnsigned(1),
            Brake = racing?.Brake ?? RawUnsigned(2),
            Clutch = racing?.Clutch ?? RawUnsigned(3),
            Handbrake = racing?.Handbrake ?? RawUnsigned(4),
            FlightRoll = flight?.Roll ?? RawSigned(0),
            FlightPitch = flight?.Pitch ?? RawSigned(1),
            FlightYaw = flight?.Yaw ?? RawSigned(2),
            FlightThrottle = flight?.Throttle ?? RawUnsigned(3),
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
        negative == positive ? 0f : negative ? -1f : 1f;

    private static ControllerVisualControl? GamepadControl(GameInputGamepadButtons button) =>
        button switch
        {
            GameInputGamepadButtons.A => ControllerVisualControl.GamepadA,
            GameInputGamepadButtons.B => ControllerVisualControl.GamepadB,
            GameInputGamepadButtons.C => ControllerVisualControl.GamepadC,
            GameInputGamepadButtons.X => ControllerVisualControl.GamepadX,
            GameInputGamepadButtons.Y => ControllerVisualControl.GamepadY,
            GameInputGamepadButtons.Z => ControllerVisualControl.GamepadZ,
            GameInputGamepadButtons.DPadUp => ControllerVisualControl.GamepadDPadUp,
            GameInputGamepadButtons.DPadDown => ControllerVisualControl.GamepadDPadDown,
            GameInputGamepadButtons.DPadLeft => ControllerVisualControl.GamepadDPadLeft,
            GameInputGamepadButtons.DPadRight => ControllerVisualControl.GamepadDPadRight,
            GameInputGamepadButtons.LeftShoulder => ControllerVisualControl.GamepadLeftShoulder,
            GameInputGamepadButtons.RightShoulder => ControllerVisualControl.GamepadRightShoulder,
            GameInputGamepadButtons.LeftThumbstick => ControllerVisualControl.GamepadLeftThumbstick,
            GameInputGamepadButtons.RightThumbstick => ControllerVisualControl.GamepadRightThumbstick,
            GameInputGamepadButtons.LeftTriggerButton => ControllerVisualControl.GamepadLeftTriggerButton,
            GameInputGamepadButtons.RightTriggerButton => ControllerVisualControl.GamepadRightTriggerButton,
            GameInputGamepadButtons.View => ControllerVisualControl.GamepadView,
            GameInputGamepadButtons.Menu => ControllerVisualControl.GamepadMenu,
            GameInputGamepadButtons.PaddleLeft1 => ControllerVisualControl.GamepadPaddleLeft1,
            GameInputGamepadButtons.PaddleLeft2 => ControllerVisualControl.GamepadPaddleLeft2,
            GameInputGamepadButtons.PaddleRight1 => ControllerVisualControl.GamepadPaddleRight1,
            GameInputGamepadButtons.PaddleRight2 => ControllerVisualControl.GamepadPaddleRight2,
            _ => null
        };

    private static ControllerVisualControl RacingControl(GameInputRacingWheelButtons button) =>
        button switch
        {
            GameInputRacingWheelButtons.A => ControllerVisualControl.RacingA,
            GameInputRacingWheelButtons.B => ControllerVisualControl.RacingB,
            GameInputRacingWheelButtons.X => ControllerVisualControl.RacingX,
            GameInputRacingWheelButtons.Y => ControllerVisualControl.RacingY,
            GameInputRacingWheelButtons.DPadUp => ControllerVisualControl.RacingDPadUp,
            GameInputRacingWheelButtons.DPadDown => ControllerVisualControl.RacingDPadDown,
            GameInputRacingWheelButtons.DPadLeft => ControllerVisualControl.RacingDPadLeft,
            GameInputRacingWheelButtons.DPadRight => ControllerVisualControl.RacingDPadRight,
            GameInputRacingWheelButtons.PreviousGear => ControllerVisualControl.RacingPreviousGear,
            GameInputRacingWheelButtons.NextGear => ControllerVisualControl.RacingNextGear,
            GameInputRacingWheelButtons.View => ControllerVisualControl.RacingView,
            GameInputRacingWheelButtons.Menu => ControllerVisualControl.RacingMenu,
            GameInputRacingWheelButtons.LeftThumbstick => ControllerVisualControl.RacingLeftThumbstick,
            GameInputRacingWheelButtons.RightThumbstick => ControllerVisualControl.RacingRightThumbstick,
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };

    private static ControllerVisualControl FlightControl(GameInputFlightStickButtons button) =>
        button switch
        {
            GameInputFlightStickButtons.A => ControllerVisualControl.FlightA,
            GameInputFlightStickButtons.B => ControllerVisualControl.FlightB,
            GameInputFlightStickButtons.X => ControllerVisualControl.FlightX,
            GameInputFlightStickButtons.Y => ControllerVisualControl.FlightY,
            GameInputFlightStickButtons.FirePrimary => ControllerVisualControl.FlightFirePrimary,
            GameInputFlightStickButtons.FireSecondary => ControllerVisualControl.FlightFireSecondary,
            GameInputFlightStickButtons.LeftShoulder => ControllerVisualControl.FlightLeftShoulder,
            GameInputFlightStickButtons.RightShoulder => ControllerVisualControl.FlightRightShoulder,
            GameInputFlightStickButtons.View => ControllerVisualControl.FlightView,
            GameInputFlightStickButtons.Menu => ControllerVisualControl.FlightMenu,
            GameInputFlightStickButtons.HatSwitchUp => ControllerVisualControl.FlightHatUp,
            GameInputFlightStickButtons.HatSwitchDown => ControllerVisualControl.FlightHatDown,
            GameInputFlightStickButtons.HatSwitchLeft => ControllerVisualControl.FlightHatLeft,
            GameInputFlightStickButtons.HatSwitchRight => ControllerVisualControl.FlightHatRight,
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };

    private static ControllerVisualControl? FlightHatControl(GameInputSwitchPosition position) =>
        position switch
        {
            GameInputSwitchPosition.Up => ControllerVisualControl.FlightHatUp,
            GameInputSwitchPosition.Down => ControllerVisualControl.FlightHatDown,
            GameInputSwitchPosition.Left => ControllerVisualControl.FlightHatLeft,
            GameInputSwitchPosition.Right => ControllerVisualControl.FlightHatRight,
            _ => null
        };

    private static ControllerVisualControl ArcadeControl(GameInputArcadeStickButtons button) =>
        button switch
        {
            GameInputArcadeStickButtons.Action1 => ControllerVisualControl.ArcadeAction1,
            GameInputArcadeStickButtons.Action2 => ControllerVisualControl.ArcadeAction2,
            GameInputArcadeStickButtons.Action3 => ControllerVisualControl.ArcadeAction3,
            GameInputArcadeStickButtons.Action4 => ControllerVisualControl.ArcadeAction4,
            GameInputArcadeStickButtons.Action5 => ControllerVisualControl.ArcadeAction5,
            GameInputArcadeStickButtons.Action6 => ControllerVisualControl.ArcadeAction6,
            GameInputArcadeStickButtons.Special1 => ControllerVisualControl.ArcadeSpecial1,
            GameInputArcadeStickButtons.Special2 => ControllerVisualControl.ArcadeSpecial2,
            GameInputArcadeStickButtons.View => ControllerVisualControl.ArcadeView,
            GameInputArcadeStickButtons.Menu => ControllerVisualControl.ArcadeMenu,
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };

    private static ControllerVisualControl? SystemControl(GameInputSystemButtons button) =>
        button switch
        {
            GameInputSystemButtons.Guide => ControllerVisualControl.SystemGuide,
            GameInputSystemButtons.Share => ControllerVisualControl.SystemShare,
            _ => null
        };

    private static ControllerVisualControl? DirectionControl(GameInputSwitchPosition position) =>
        position switch
        {
            GameInputSwitchPosition.Up => ControllerVisualControl.GamepadDPadUp,
            GameInputSwitchPosition.Down => ControllerVisualControl.GamepadDPadDown,
            GameInputSwitchPosition.Left => ControllerVisualControl.GamepadDPadLeft,
            GameInputSwitchPosition.Right => ControllerVisualControl.GamepadDPadRight,
            _ => null
        };

    private static ControllerVisualControl? LabelControl(GameInputLabel label) =>
        label switch
        {
            GameInputLabel.XboxA or GameInputLabel.LetterA => ControllerVisualControl.GamepadA,
            GameInputLabel.XboxB or GameInputLabel.LetterB => ControllerVisualControl.GamepadB,
            GameInputLabel.LetterC => ControllerVisualControl.GamepadC,
            GameInputLabel.XboxX or GameInputLabel.LetterX => ControllerVisualControl.GamepadX,
            GameInputLabel.XboxY or GameInputLabel.LetterY => ControllerVisualControl.GamepadY,
            GameInputLabel.LetterZ => ControllerVisualControl.GamepadZ,
            GameInputLabel.XboxDPadUp or GameInputLabel.IconDPadUp or GameInputLabel.Up => ControllerVisualControl.GamepadDPadUp,
            GameInputLabel.XboxDPadDown or GameInputLabel.IconDPadDown or GameInputLabel.Down => ControllerVisualControl.GamepadDPadDown,
            GameInputLabel.XboxDPadLeft or GameInputLabel.IconDPadLeft or GameInputLabel.Left => ControllerVisualControl.GamepadDPadLeft,
            GameInputLabel.XboxDPadRight or GameInputLabel.IconDPadRight or GameInputLabel.Right => ControllerVisualControl.GamepadDPadRight,
            GameInputLabel.XboxLeftShoulder or GameInputLabel.LB or GameInputLabel.L1 => ControllerVisualControl.GamepadLeftShoulder,
            GameInputLabel.XboxRightShoulder or GameInputLabel.RB or GameInputLabel.R1 => ControllerVisualControl.GamepadRightShoulder,
            GameInputLabel.XboxLeftStickButton or GameInputLabel.LSB or GameInputLabel.L3 => ControllerVisualControl.GamepadLeftThumbstick,
            GameInputLabel.XboxRightStickButton or GameInputLabel.RSB or GameInputLabel.R3 => ControllerVisualControl.GamepadRightThumbstick,
            GameInputLabel.XboxLeftTrigger => ControllerVisualControl.GamepadLeftTriggerLabel,
            GameInputLabel.XboxRightTrigger => ControllerVisualControl.GamepadRightTriggerLabel,
            GameInputLabel.XboxBack or GameInputLabel.XboxView or GameInputLabel.View or GameInputLabel.Back or GameInputLabel.Select => ControllerVisualControl.GamepadView,
            GameInputLabel.XboxStart or GameInputLabel.XboxMenu or GameInputLabel.Menu or GameInputLabel.Start or GameInputLabel.Options => ControllerVisualControl.GamepadMenu,
            GameInputLabel.XboxPaddle1 or GameInputLabel.PaddleLeft1 => ControllerVisualControl.GamepadPaddleLeft1,
            GameInputLabel.XboxPaddle2 or GameInputLabel.PaddleLeft2 => ControllerVisualControl.GamepadPaddleLeft2,
            GameInputLabel.XboxPaddle3 or GameInputLabel.PaddleRight1 => ControllerVisualControl.GamepadPaddleRight1,
            GameInputLabel.XboxPaddle4 or GameInputLabel.PaddleRight2 => ControllerVisualControl.GamepadPaddleRight2,
            GameInputLabel.Share => ControllerVisualControl.SystemShare,
            _ => null
        };

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
