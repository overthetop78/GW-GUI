using GWGUI.App.Enums.Input;
using GWGUI.App.Services.Input.GameInput;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static class ControllerVisualControlMappings
{
    internal static ControllerVisualControl? GamepadControl(GameInputGamepadButtons button) =>
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

    internal static ControllerVisualControl RacingControl(GameInputRacingWheelButtons button) =>
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

    internal static ControllerVisualControl FlightControl(GameInputFlightStickButtons button) =>
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

    internal static ControllerVisualControl? FlightHatControl(GameInputSwitchPosition position) =>
        position switch
        {
            GameInputSwitchPosition.Up => ControllerVisualControl.FlightHatUp,
            GameInputSwitchPosition.Down => ControllerVisualControl.FlightHatDown,
            GameInputSwitchPosition.Left => ControllerVisualControl.FlightHatLeft,
            GameInputSwitchPosition.Right => ControllerVisualControl.FlightHatRight,
            _ => null
        };

    internal static ControllerVisualControl ArcadeControl(GameInputArcadeStickButtons button) =>
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

    internal static ControllerVisualControl? SystemControl(GameInputSystemButtons button) =>
        button switch
        {
            GameInputSystemButtons.Guide => ControllerVisualControl.SystemGuide,
            GameInputSystemButtons.Share => ControllerVisualControl.SystemShare,
            _ => null
        };

    internal static ControllerVisualControl? DirectionControl(GameInputSwitchPosition position) =>
        position switch
        {
            GameInputSwitchPosition.Up => ControllerVisualControl.GamepadDPadUp,
            GameInputSwitchPosition.Down => ControllerVisualControl.GamepadDPadDown,
            GameInputSwitchPosition.Left => ControllerVisualControl.GamepadDPadLeft,
            GameInputSwitchPosition.Right => ControllerVisualControl.GamepadDPadRight,
            _ => null
        };

    internal static ControllerVisualControl? LabelControl(GameInputLabel label) =>
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

}
