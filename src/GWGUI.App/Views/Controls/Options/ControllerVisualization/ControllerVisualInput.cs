using GWGUI.App.Services.Input.GameInput;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal sealed class ControllerVisualInput(GameInputLiveState? state)
{
    internal GameInputLiveState? State { get; } = state;

    internal float LeftX => GamepadAxis(
        gamepad => gamepad.LeftThumbstickX,
        GameInputGamepadButtons.LeftThumbstickLeft,
        GameInputGamepadButtons.LeftThumbstickRight,
        0);
    internal float LeftY => State?.Gamepad is null
        ? RawAxisSigned(1)
        : -GamepadAxis(
            gamepad => gamepad.LeftThumbstickY,
            GameInputGamepadButtons.LeftThumbstickDown,
            GameInputGamepadButtons.LeftThumbstickUp,
            1);
    internal float RightX => GamepadAxis(
        gamepad => gamepad.RightThumbstickX,
        GameInputGamepadButtons.RightThumbstickLeft,
        GameInputGamepadButtons.RightThumbstickRight,
        2);
    internal float RightY => State?.Gamepad is null
        ? RawAxisSigned(3)
        : -GamepadAxis(
            gamepad => gamepad.RightThumbstickY,
            GameInputGamepadButtons.RightThumbstickDown,
            GameInputGamepadButtons.RightThumbstickUp,
            3);
    internal float LeftTrigger => Trigger(GameInputLabel.XboxLeftTrigger, left: true);
    internal float RightTrigger => Trigger(GameInputLabel.XboxRightTrigger, left: false);
    internal float Wheel => State?.RacingWheel?.Wheel ?? RawAxisSigned(0);
    internal float Throttle => State?.RacingWheel?.Throttle ?? RawAxisUnsigned(1);
    internal float Brake => State?.RacingWheel?.Brake ?? RawAxisUnsigned(2);
    internal float Clutch => State?.RacingWheel?.Clutch ?? RawAxisUnsigned(3);
    internal float Handbrake => State?.RacingWheel?.Handbrake ?? RawAxisUnsigned(4);
    internal float FlightRoll => State?.FlightStick?.Roll ?? RawAxisSigned(0);
    internal float FlightPitch => State?.FlightStick?.Pitch ?? RawAxisSigned(1);
    internal float FlightYaw => State?.FlightStick?.Yaw ?? RawAxisSigned(2);
    internal float FlightThrottle => State?.FlightStick?.Throttle ?? RawAxisUnsigned(3);
    internal int PatternShifterGear => State?.RacingWheel?.PatternShifterGear ?? 0;
    internal float ArcadeX => State?.ArcadeStick is { } arcade
        ? DigitalAxis(arcade.Buttons.HasFlag(GameInputArcadeStickButtons.Left),
            arcade.Buttons.HasFlag(GameInputArcadeStickButtons.Right))
        : LeftX;
    internal float ArcadeY => State?.ArcadeStick is { } arcade
        ? DigitalAxis(arcade.Buttons.HasFlag(GameInputArcadeStickButtons.Up),
            arcade.Buttons.HasFlag(GameInputArcadeStickButtons.Down))
        : LeftY;

    internal bool RacingButton(GameInputRacingWheelButtons button, int fallbackIndex) =>
        State?.RacingWheel is { } wheel
            ? wheel.Buttons.HasFlag(button)
            : RawButton(fallbackIndex);

    internal bool FlightButton(GameInputFlightStickButtons button, int fallbackIndex) =>
        State?.FlightStick is { } flight
            ? flight.Buttons.HasFlag(button)
            : RawButton(fallbackIndex);

    internal bool FlightHat(GameInputSwitchPosition position)
    {
        if (State?.FlightStick is { } flight)
        {
            var flag = position switch
            {
                GameInputSwitchPosition.Up => GameInputFlightStickButtons.HatSwitchUp,
                GameInputSwitchPosition.Down => GameInputFlightStickButtons.HatSwitchDown,
                GameInputSwitchPosition.Left => GameInputFlightStickButtons.HatSwitchLeft,
                GameInputSwitchPosition.Right => GameInputFlightStickButtons.HatSwitchRight,
                _ => GameInputFlightStickButtons.None
            };
            return MatchesDirection(flight.HatSwitch, position) ||
                (flag != GameInputFlightStickButtons.None && flight.Buttons.HasFlag(flag));
        }
        var current = State?.Controls.FirstOrDefault(
            control => control.Type == GameInputControlType.Switch)?.SwitchPosition;
        if (current is not null) return MatchesDirection(current.Value, position);
        var horizontal = RawAxisSigned(4);
        var vertical = RawAxisSigned(5);
        return position switch
        {
            GameInputSwitchPosition.Up => vertical < -.5f,
            GameInputSwitchPosition.Down => vertical > .5f,
            GameInputSwitchPosition.Left => horizontal < -.5f,
            GameInputSwitchPosition.Right => horizontal > .5f,
            _ => false
        };
    }

    internal bool ArcadeButton(GameInputArcadeStickButtons button, int fallbackIndex) =>
        State?.ArcadeStick is { } arcade
            ? arcade.Buttons.HasFlag(button)
            : RawButton(fallbackIndex);


    internal bool Button(GameInputGamepadButtons button, int fallbackIndex)
    {
        if (State?.Gamepad is { } gamepad && (gamepad.Buttons & button) != 0) return true;
        GameInputLabel[] labels = button switch
        {
            GameInputGamepadButtons.A => [GameInputLabel.XboxA, GameInputLabel.LetterA],
            GameInputGamepadButtons.B => [GameInputLabel.XboxB, GameInputLabel.LetterB],
            GameInputGamepadButtons.C => [GameInputLabel.LetterC],
            GameInputGamepadButtons.X => [GameInputLabel.XboxX, GameInputLabel.LetterX],
            GameInputGamepadButtons.Y => [GameInputLabel.XboxY, GameInputLabel.LetterY],
            GameInputGamepadButtons.Z => [GameInputLabel.LetterZ],
            GameInputGamepadButtons.DPadUp => [GameInputLabel.XboxDPadUp, GameInputLabel.IconDPadUp, GameInputLabel.Up],
            GameInputGamepadButtons.DPadDown => [GameInputLabel.XboxDPadDown, GameInputLabel.IconDPadDown, GameInputLabel.Down],
            GameInputGamepadButtons.DPadLeft => [GameInputLabel.XboxDPadLeft, GameInputLabel.IconDPadLeft, GameInputLabel.Left],
            GameInputGamepadButtons.DPadRight => [GameInputLabel.XboxDPadRight, GameInputLabel.IconDPadRight, GameInputLabel.Right],
            GameInputGamepadButtons.LeftShoulder => [GameInputLabel.XboxLeftShoulder, GameInputLabel.LB, GameInputLabel.L1],
            GameInputGamepadButtons.RightShoulder => [GameInputLabel.XboxRightShoulder, GameInputLabel.RB, GameInputLabel.R1],
            GameInputGamepadButtons.LeftThumbstick => [GameInputLabel.XboxLeftStickButton, GameInputLabel.LSB, GameInputLabel.L3],
            GameInputGamepadButtons.RightThumbstick => [GameInputLabel.XboxRightStickButton, GameInputLabel.RSB, GameInputLabel.R3],
            GameInputGamepadButtons.View => [GameInputLabel.XboxBack, GameInputLabel.XboxView, GameInputLabel.View, GameInputLabel.Back, GameInputLabel.Select],
            GameInputGamepadButtons.Menu => [GameInputLabel.XboxStart, GameInputLabel.XboxMenu, GameInputLabel.Menu, GameInputLabel.Start, GameInputLabel.Options],
            GameInputGamepadButtons.PaddleLeft1 => [GameInputLabel.XboxPaddle1, GameInputLabel.PaddleLeft1],
            GameInputGamepadButtons.PaddleLeft2 => [GameInputLabel.XboxPaddle2, GameInputLabel.PaddleLeft2],
            GameInputGamepadButtons.PaddleRight1 => [GameInputLabel.XboxPaddle3, GameInputLabel.PaddleRight1],
            GameInputGamepadButtons.PaddleRight2 => [GameInputLabel.XboxPaddle4, GameInputLabel.PaddleRight2],
            _ => Array.Empty<GameInputLabel>()
        };
        if (labels.Length != 0 && LabelButton(labels)) return true;
        return RawButton(fallbackIndex);
    }

    internal bool TriggerPressed(bool left) =>
        (left ? LeftTrigger : RightTrigger) > .04f ||
        Button(left ? GameInputGamepadButtons.LeftTriggerButton : GameInputGamepadButtons.RightTriggerButton,
            left ? 12 : 13);

    internal bool SystemButton(GameInputSystemButtons button) =>
        State?.SystemButtons.HasFlag(button) == true;

    internal bool LabelButton(params GameInputLabel[] labels) => State?.Controls.Any(control =>
        control.Type == GameInputControlType.Button &&
        labels.Contains(control.Label) && control.IsPressed) == true;

    internal bool RawButton(int index) => State?.Controls.FirstOrDefault(control =>
        control.Type == GameInputControlType.Button && control.Index == index)?.IsPressed == true;

    internal bool Direction(GameInputSwitchPosition position, GameInputGamepadButtons button, int fallbackIndex)
        => Direction(position, button, fallbackIndex, 0, 1);

    internal bool Direction(
        GameInputSwitchPosition position,
        GameInputGamepadButtons button,
        int fallbackIndex,
        int horizontalAxis,
        int verticalAxis)
    {
        if (State?.Gamepad is { } gamepad)
            return (gamepad.Buttons & button) != 0;
        if (Button(button, fallbackIndex)) return true;
        var current = State?.Controls.FirstOrDefault(control => control.Type == GameInputControlType.Switch)?.SwitchPosition;
        if (current is not null) return MatchesDirection(current.Value, position);

        var horizontal = RawAxisSigned(horizontalAxis);
        var vertical = RawAxisSigned(verticalAxis);
        return position switch
        {
            GameInputSwitchPosition.Up => vertical < -.5f,
            GameInputSwitchPosition.Down => vertical > .5f,
            GameInputSwitchPosition.Left => horizontal < -.5f,
            GameInputSwitchPosition.Right => horizontal > .5f,
            _ => false
        };
    }

    internal float RawAxisUnsigned(int index, float defaultValue = 0f) => Math.Clamp(State?.Controls.FirstOrDefault(control =>
        control.Type == GameInputControlType.Axis && control.Index == index)?.Value ?? defaultValue, 0f, 1f);

    internal float RawAxisSigned(int index)
    {
        var value = State?.Controls.FirstOrDefault(control =>
            control.Type == GameInputControlType.Axis && control.Index == index)?.Value;
        if (value is null) return 0f;
        return State!.ControlsUseNormalizedAxes
            ? Math.Clamp(value.Value * 2f - 1f, -1f, 1f)
            : Math.Clamp(value.Value, -1f, 1f);
    }

    private static float DigitalAxis(bool negative, bool positive) =>
        negative == positive ? 0f : negative ? -1f : 1f;

    private static bool MatchesDirection(GameInputSwitchPosition current, GameInputSwitchPosition requested) =>
        requested switch
        {
            GameInputSwitchPosition.Up => current is GameInputSwitchPosition.Up or GameInputSwitchPosition.UpLeft or GameInputSwitchPosition.UpRight,
            GameInputSwitchPosition.Down => current is GameInputSwitchPosition.Down or GameInputSwitchPosition.DownLeft or GameInputSwitchPosition.DownRight,
            GameInputSwitchPosition.Left => current is GameInputSwitchPosition.Left or GameInputSwitchPosition.UpLeft or GameInputSwitchPosition.DownLeft,
            GameInputSwitchPosition.Right => current is GameInputSwitchPosition.Right or GameInputSwitchPosition.UpRight or GameInputSwitchPosition.DownRight,
            _ => false
        };

    private float GamepadAxis(
        Func<GameInputGamepadState, float> selector,
        GameInputGamepadButtons negative,
        GameInputGamepadButtons positive,
        int rawIndex)
    {
        if (State?.Gamepad is not { } gamepad) return RawAxisSigned(rawIndex);
        var analog = Math.Clamp(selector(gamepad), -1f, 1f);
        if (Math.Abs(analog) > .0001f) return analog;
        return DigitalAxis(gamepad.Buttons.HasFlag(negative), gamepad.Buttons.HasFlag(positive));
    }

    private float Trigger(GameInputLabel label, bool left)
    {
        if (State?.Gamepad is { } gamepad)
        {
            var analog = Math.Clamp(left ? gamepad.LeftTrigger : gamepad.RightTrigger, 0f, 1f);
            if (analog > .0001f) return analog;
            var flag = left ? GameInputGamepadButtons.LeftTriggerButton : GameInputGamepadButtons.RightTriggerButton;
            if (gamepad.Buttons.HasFlag(flag)) return 1f;
        }
        var value = Math.Clamp(State?.Controls.FirstOrDefault(control =>
            control.Type == GameInputControlType.Axis && control.Label == label)?.Value ?? 0f, 0f, 1f);
        if (value > .0001f) return value;
        return LabelButton(label) ? 1f : 0f;
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
