using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using System.Globalization;
using System.Text;

namespace GWGUI.App.Views.Controls.Options.ControllerPresentation;

internal static class GameInputDisplayFormatter
{
    internal static string ControlName(GameInputControlValue control)
    {
        var type = LocExtension.Get($"Controllers.ControlType.{control.Type}");
        var label = Label(control.Label);
        return string.IsNullOrEmpty(label) ? $"{type} {control.Index + 1}" : $"{label} · {type} {control.Index + 1}";
    }

    internal static string ControlDefinition(GameInputControlDescriptor control)
    {
        var name = ControlName(new GameInputControlValue(
            control.Type, control.Index, control.Label, 0f));
        if (control.Type != GameInputControlType.Switch) return name;
        var labels = control.SwitchLabels?
            .Select(Label)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.CurrentCulture)
            .ToArray() ?? [];
        var suffix = labels.Length == 0 ? string.Empty : $" · {string.Join(" ", labels)}";
        return $"{name} · {SwitchKind(control.SwitchKind)}{suffix}";
    }

    internal static string ControlValue(GameInputControlValue control) => control.Type switch
    {
        GameInputControlType.Switch => SwitchPosition(control.SwitchPosition),
        GameInputControlType.Button => control.IsPressed ? "1" : "0",
        _ => control.Value.ToString("0.000", CultureInfo.CurrentCulture)
    };

    internal static string Label(GameInputLabel label) => label switch
    {
        GameInputLabel.None or GameInputLabel.Unknown => string.Empty,
        GameInputLabel.XboxGuide => "Xbox ◆",
        GameInputLabel.XboxBack or GameInputLabel.XboxView => "Xbox ◫",
        GameInputLabel.XboxStart or GameInputLabel.XboxMenu => "Xbox ☰",
        GameInputLabel.XboxA => "Xbox A",
        GameInputLabel.XboxB => "Xbox B",
        GameInputLabel.XboxX => "Xbox X",
        GameInputLabel.XboxY => "Xbox Y",
        GameInputLabel.XboxDPadUp => "Xbox ↑",
        GameInputLabel.XboxDPadDown => "Xbox ↓",
        GameInputLabel.XboxDPadLeft => "Xbox ←",
        GameInputLabel.XboxDPadRight => "Xbox →",
        GameInputLabel.XboxLeftShoulder => "Xbox LB",
        GameInputLabel.XboxLeftTrigger => "Xbox LT",
        GameInputLabel.XboxLeftStickButton => "Xbox L3",
        GameInputLabel.XboxRightShoulder => "Xbox RB",
        GameInputLabel.XboxRightTrigger => "Xbox RT",
        GameInputLabel.XboxRightStickButton => "Xbox R3",
        GameInputLabel.XboxPaddle1 => "Xbox P1",
        GameInputLabel.XboxPaddle2 => "Xbox P2",
        GameInputLabel.XboxPaddle3 => "Xbox P3",
        GameInputLabel.XboxPaddle4 => "Xbox P4",
        GameInputLabel.ArrowUp or GameInputLabel.IconDPadUp or GameInputLabel.Up => "↑",
        GameInputLabel.ArrowUpRight => "↗",
        GameInputLabel.ArrowRight or GameInputLabel.IconDPadRight or GameInputLabel.Right => "→",
        GameInputLabel.ArrowDownRight => "↘",
        GameInputLabel.ArrowDown or GameInputLabel.IconDPadDown or GameInputLabel.Down => "↓",
        GameInputLabel.ArrowDownLeft => "↙",
        GameInputLabel.ArrowLeft or GameInputLabel.IconDPadLeft or GameInputLabel.Left => "←",
        GameInputLabel.ArrowUpLeft => "↖",
        GameInputLabel.ArrowUpDown => "↕",
        GameInputLabel.ArrowLeftRight => "↔",
        GameInputLabel.ArrowUpDownLeftRight => "✣",
        GameInputLabel.ArrowClockwise or GameInputLabel.IconDialClockwise => "↻",
        GameInputLabel.ArrowCounterClockwise or GameInputLabel.IconDialCounterClockwise => "↺",
        GameInputLabel.ArrowReturn => "↵",
        GameInputLabel.IconBranding or GameInputLabel.Guide => "◆",
        GameInputLabel.IconHome or GameInputLabel.Home => "⌂",
        GameInputLabel.IconMenu or GameInputLabel.Menu => "☰",
        GameInputLabel.IconCross => "✕",
        GameInputLabel.IconCircle => "○",
        GameInputLabel.IconSquare => "□",
        GameInputLabel.IconTriangle => "△",
        GameInputLabel.IconStar => "★",
        GameInputLabel.IconSliderLeftRight => "↔",
        GameInputLabel.IconSliderUpDown or GameInputLabel.IconWheelUpDown => "↕",
        GameInputLabel.IconPlus => "+",
        GameInputLabel.IconMinus => "−",
        GameInputLabel.IconSuspension => "⏸",
        GameInputLabel.Mode => "MODE",
        GameInputLabel.Select => "SELECT",
        GameInputLabel.View => "◫",
        GameInputLabel.Back => "↩",
        GameInputLabel.Start => "START",
        GameInputLabel.Options => "⋯",
        GameInputLabel.Share => "↥",
        GameInputLabel.LB => "LB",
        GameInputLabel.LT => "LT",
        GameInputLabel.LSB => "LSB",
        GameInputLabel.L1 => "L1",
        GameInputLabel.L2 => "L2",
        GameInputLabel.L3 => "L3",
        GameInputLabel.RB => "RB",
        GameInputLabel.RT => "RT",
        GameInputLabel.RSB => "RSB",
        GameInputLabel.R1 => "R1",
        GameInputLabel.R2 => "R2",
        GameInputLabel.R3 => "R3",
        GameInputLabel.PaddleLeft1 => "L P1",
        GameInputLabel.PaddleLeft2 => "L P2",
        GameInputLabel.PaddleRight1 => "R P1",
        GameInputLabel.PaddleRight2 => "R P2",
        _ when label is >= GameInputLabel.LetterA and <= GameInputLabel.LetterZ =>
            ((char)('A' + (int)label - (int)GameInputLabel.LetterA)).ToString(),
        _ when label is >= GameInputLabel.Number0 and <= GameInputLabel.Number9 =>
            ((char)('0' + (int)label - (int)GameInputLabel.Number0)).ToString(),
        _ => $"{LocExtension.Get("Common.Unknown")} ({(int)label})"
    };

    internal static string SwitchKind(GameInputSwitchKind kind) => kind switch
    {
        GameInputSwitchKind.TwoWay => "↕ · 2",
        GameInputSwitchKind.FourWay => "✣ · 4",
        GameInputSwitchKind.EightWay => "✣ · 8",
        _ => LocExtension.Get("Common.Unknown")
    };

    internal static string SwitchPosition(GameInputSwitchPosition position) => position switch
    {
        GameInputSwitchPosition.Center => "●",
        GameInputSwitchPosition.Up => "↑",
        GameInputSwitchPosition.UpRight => "↗",
        GameInputSwitchPosition.Right => "→",
        GameInputSwitchPosition.DownRight => "↘",
        GameInputSwitchPosition.Down => "↓",
        GameInputSwitchPosition.DownLeft => "↙",
        GameInputSwitchPosition.Left => "←",
        GameInputSwitchPosition.UpLeft => "↖",
        _ => $"{LocExtension.Get("Common.Unknown")} ({(int)position})"
    };

    internal static string Flags<T>(T value) where T : struct, Enum
    {
        var bits = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        if (bits == 0) return LocExtension.Get("Controllers.None");
        var parts = Enum.GetValues<T>()
            .Select(item => (Item: item, Bits: Convert.ToUInt64(item, CultureInfo.InvariantCulture)))
            .Where(item => item.Bits != 0 && IsSingleBit(item.Bits) && (bits & item.Bits) == item.Bits)
            .Select(item => EnumValue(item.Item))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        var known = Enum.GetValues<T>().Select(item => Convert.ToUInt64(item, CultureInfo.InvariantCulture))
            .Where(IsSingleBit).Aggregate(0UL, (current, item) => current | item);
        var unknown = bits & ~known;
        if (unknown != 0) parts.Add($"0x{unknown:X}");
        return parts.Count == 0 ? EnumValue(value) : string.Join(" · ", parts);
    }

    internal static string Family(GameInputDeviceFamily family) => family switch
    {
        GameInputDeviceFamily.XboxOne => "Xbox One / GIP",
        GameInputDeviceFamily.Xbox360 => "Xbox 360 / XInput",
        GameInputDeviceFamily.Hid => "HID",
        GameInputDeviceFamily.I8042 => "i8042",
        GameInputDeviceFamily.Aggregate => LocExtension.Get("Controllers.Enum.Aggregate"),
        GameInputDeviceFamily.Virtual => LocExtension.Get("Controllers.Enum.Virtual"),
        _ => LocExtension.Get("Common.Unknown")
    };

    internal static string Usage(GameInputUsage usage)
    {
        var name = (usage.Page, usage.Id) switch
        {
            (0x01, 0x04) => LocExtension.Get("Controllers.Enum.Joystick"),
            (0x01, 0x05) => LocExtension.Get("Controllers.Enum.Gamepad"),
            (0x01, 0x08) => LocExtension.Get("Controllers.Enum.MultiAxisController"),
            (0x01, 0x02) => LocExtension.Get("Emulation.Tab.Mouse"),
            (0x01, 0x06) => LocExtension.Get("Emulation.Tab.Keyboard"),
            _ => LocExtension.Get("Controllers.Enum.HidUsage")
        };
        return $"{name} · {usage.Page:X4}:{usage.Id:X4}";
    }

    internal static string FeedbackMotor(GameInputForceFeedbackMotorDescriptor motor)
    {
        var effects = motor.SupportedEffects.Count == 0
            ? LocExtension.Get("Controllers.None")
            : string.Join(", ", motor.SupportedEffects.Select(EnumValue));
        var power = motor.PoweredOn ? LocExtension.Get("Controllers.Yes") : LocExtension.Get("Controllers.No");
        return $"#{motor.Index + 1}: ⏻ {power} · {Flags(motor.SupportedAxes)} · {effects}";
    }


    internal static string EnumValue<T>(T value) where T : struct, Enum => value switch
    {
        GameInputKind.RawDeviceReport => LocExtension.Get("Controllers.RawReports"),
        GameInputKind.ControllerAxis => LocExtension.Get("Controllers.ControlType.Axis"),
        GameInputKind.ControllerButton => LocExtension.Get("Controllers.ControlType.Button"),
        GameInputKind.ControllerSwitch => LocExtension.Get("Controllers.ControlType.Switch"),
        GameInputKind.Keyboard => LocExtension.Get("Emulation.Tab.Keyboard"),
        GameInputKind.Mouse => LocExtension.Get("Emulation.Tab.Mouse"),
        GameInputKind.Sensors => LocExtension.Get("Controllers.Enum.Sensors"),
        GameInputKind.ArcadeStick => LocExtension.Get("Controllers.Model.ArcadeStick"),
        GameInputKind.FlightStick => LocExtension.Get("Controllers.Model.FlightStick"),
        GameInputKind.Gamepad => LocExtension.Get("Controllers.Enum.Gamepad"),
        GameInputKind.RacingWheel => LocExtension.Get("Controllers.Model.RacingWheel"),
        GameInputKind.UiNavigation => LocExtension.Get("Controllers.Enum.UiNavigation"),

        GameInputRumbleMotors.LowFrequency => LocExtension.Get("Controllers.Enum.LowFrequencyMotor"),
        GameInputRumbleMotors.HighFrequency => LocExtension.Get("Controllers.Enum.HighFrequencyMotor"),
        GameInputRumbleMotors.LeftTrigger => LocExtension.Get("Controllers.LeftTrigger"),
        GameInputRumbleMotors.RightTrigger => LocExtension.Get("Controllers.RightTrigger"),

        GameInputSystemButtons.Guide => "◆",
        GameInputSystemButtons.Share => LocExtension.Get("Controllers.Enum.Share"),

        GameInputDeviceStatus.Connected => $"✓ {LocExtension.Get("Controllers.Device")}",
        GameInputDeviceStatus.HapticInfoReady => $"✓ {LocExtension.Get("Controllers.Haptics")}",

        GameInputFeedbackAxes.LinearX => $"{LocExtension.Get("Controllers.Enum.Linear")} X",
        GameInputFeedbackAxes.LinearY => $"{LocExtension.Get("Controllers.Enum.Linear")} Y",
        GameInputFeedbackAxes.LinearZ => $"{LocExtension.Get("Controllers.Enum.Linear")} Z",
        GameInputFeedbackAxes.AngularX => $"{LocExtension.Get("Controllers.Enum.Angular")} X",
        GameInputFeedbackAxes.AngularY => $"{LocExtension.Get("Controllers.Enum.Angular")} Y",
        GameInputFeedbackAxes.AngularZ => $"{LocExtension.Get("Controllers.Enum.Angular")} Z",
        GameInputFeedbackAxes.Normal => LocExtension.Get("Controllers.Enum.Normal"),

        GameInputForceFeedbackEffectKind.Constant => LocExtension.Get("Controllers.Enum.Effect.Constant"),
        GameInputForceFeedbackEffectKind.Ramp => LocExtension.Get("Controllers.Enum.Effect.Ramp"),
        GameInputForceFeedbackEffectKind.SineWave => LocExtension.Get("Controllers.Enum.Effect.SineWave"),
        GameInputForceFeedbackEffectKind.SquareWave => LocExtension.Get("Controllers.Enum.Effect.SquareWave"),
        GameInputForceFeedbackEffectKind.TriangleWave => LocExtension.Get("Controllers.Enum.Effect.TriangleWave"),
        GameInputForceFeedbackEffectKind.SawtoothUpWave => LocExtension.Get("Controllers.Enum.Effect.SawtoothUp"),
        GameInputForceFeedbackEffectKind.SawtoothDownWave => LocExtension.Get("Controllers.Enum.Effect.SawtoothDown"),
        GameInputForceFeedbackEffectKind.Spring => LocExtension.Get("Controllers.Enum.Effect.Spring"),
        GameInputForceFeedbackEffectKind.Friction => LocExtension.Get("Controllers.Enum.Effect.Friction"),
        GameInputForceFeedbackEffectKind.Damper => LocExtension.Get("Controllers.Enum.Effect.Damper"),
        GameInputForceFeedbackEffectKind.Inertia => LocExtension.Get("Controllers.Enum.Effect.Inertia"),

        GameInputArcadeStickButtons.Menu => "☰",
        GameInputArcadeStickButtons.View => "◫",
        GameInputArcadeStickButtons.Up => "↑",
        GameInputArcadeStickButtons.Down => "↓",
        GameInputArcadeStickButtons.Left => "←",
        GameInputArcadeStickButtons.Right => "→",
        GameInputArcadeStickButtons.Action1 => $"{LocExtension.Get("Controllers.Control")} 1",
        GameInputArcadeStickButtons.Action2 => $"{LocExtension.Get("Controllers.Control")} 2",
        GameInputArcadeStickButtons.Action3 => $"{LocExtension.Get("Controllers.Control")} 3",
        GameInputArcadeStickButtons.Action4 => $"{LocExtension.Get("Controllers.Control")} 4",
        GameInputArcadeStickButtons.Action5 => $"{LocExtension.Get("Controllers.Control")} 5",
        GameInputArcadeStickButtons.Action6 => $"{LocExtension.Get("Controllers.Control")} 6",
        GameInputArcadeStickButtons.Special1 => $"{LocExtension.Get("Controllers.ExtraControls")} 1",
        GameInputArcadeStickButtons.Special2 => $"{LocExtension.Get("Controllers.ExtraControls")} 2",

        GameInputFlightStickButtons.Menu => "☰",
        GameInputFlightStickButtons.View => "◫",
        GameInputFlightStickButtons.FirePrimary => "🔥 1",
        GameInputFlightStickButtons.FireSecondary => "🔥 2",
        GameInputFlightStickButtons.HatSwitchUp => "Hat ↑",
        GameInputFlightStickButtons.HatSwitchDown => "Hat ↓",
        GameInputFlightStickButtons.HatSwitchLeft => "Hat ←",
        GameInputFlightStickButtons.HatSwitchRight => "Hat →",
        GameInputFlightStickButtons.A => "A",
        GameInputFlightStickButtons.B => "B",
        GameInputFlightStickButtons.X => "X",
        GameInputFlightStickButtons.Y => "Y",
        GameInputFlightStickButtons.LeftShoulder => "LB",
        GameInputFlightStickButtons.RightShoulder => "RB",

        GameInputRacingWheelButtons.Menu => "☰",
        GameInputRacingWheelButtons.View => "◫",
        GameInputRacingWheelButtons.PreviousGear => $"{LocExtension.Get("Controllers.Gear")} −",
        GameInputRacingWheelButtons.NextGear => $"{LocExtension.Get("Controllers.Gear")} +",
        GameInputRacingWheelButtons.DPadUp => "D-pad ↑",
        GameInputRacingWheelButtons.DPadDown => "D-pad ↓",
        GameInputRacingWheelButtons.DPadLeft => "D-pad ←",
        GameInputRacingWheelButtons.DPadRight => "D-pad →",
        GameInputRacingWheelButtons.A => "A",
        GameInputRacingWheelButtons.B => "B",
        GameInputRacingWheelButtons.X => "X",
        GameInputRacingWheelButtons.Y => "Y",
        GameInputRacingWheelButtons.LeftThumbstick => "L3",
        GameInputRacingWheelButtons.RightThumbstick => "R3",

        GameInputFlightStickAxes.Roll => LocExtension.Get("Controllers.Roll"),
        GameInputFlightStickAxes.Pitch => LocExtension.Get("Controllers.Pitch"),
        GameInputFlightStickAxes.Yaw => LocExtension.Get("Controllers.Yaw"),
        GameInputFlightStickAxes.Throttle => LocExtension.Get("Controllers.Throttle"),

        GameInputGamepadAxes.LeftTrigger => LocExtension.Get("Controllers.LeftTrigger"),
        GameInputGamepadAxes.RightTrigger => LocExtension.Get("Controllers.RightTrigger"),
        GameInputGamepadAxes.LeftThumbstickX => LocExtension.Get("Controllers.LeftStickX"),
        GameInputGamepadAxes.LeftThumbstickY => LocExtension.Get("Controllers.LeftStickY"),
        GameInputGamepadAxes.RightThumbstickX => LocExtension.Get("Controllers.RightStickX"),
        GameInputGamepadAxes.RightThumbstickY => LocExtension.Get("Controllers.RightStickY"),

        GameInputRacingWheelAxes.Steering => LocExtension.Get("Controllers.Steering"),
        GameInputRacingWheelAxes.Throttle => LocExtension.Get("Controllers.Throttle"),
        GameInputRacingWheelAxes.Brake => LocExtension.Get("Controllers.Brake"),
        GameInputRacingWheelAxes.Clutch => LocExtension.Get("Controllers.Clutch"),
        GameInputRacingWheelAxes.Handbrake => LocExtension.Get("Controllers.Handbrake"),
        GameInputRacingWheelAxes.PatternShifter => LocExtension.Get("Controllers.Gear"),

        GameInputRawDeviceReportKind.Input => LocExtension.Get("Controllers.Enum.InputReport"),
        GameInputRawDeviceReportKind.Output => LocExtension.Get("Controllers.Enum.OutputReport"),
        GameInputFeedbackEffectState.Stopped => "■",
        GameInputFeedbackEffectState.Running => "▶",
        GameInputFeedbackEffectState.Paused => "⏸",
        GameInputElementKind.Axis => LocExtension.Get("Controllers.ControlType.Axis"),
        GameInputElementKind.Button => LocExtension.Get("Controllers.ControlType.Button"),
        GameInputElementKind.Switch => LocExtension.Get("Controllers.ControlType.Switch"),

        GameInputGamepadButtons.Menu => "☰",
        GameInputGamepadButtons.View => "◫",
        GameInputGamepadButtons.A => "A",
        GameInputGamepadButtons.B => "B",
        GameInputGamepadButtons.X => "X",
        GameInputGamepadButtons.Y => "Y",
        GameInputGamepadButtons.C => "C",
        GameInputGamepadButtons.Z => "Z",
        GameInputGamepadButtons.DPadUp => "D-pad ↑",
        GameInputGamepadButtons.DPadDown => "D-pad ↓",
        GameInputGamepadButtons.DPadLeft => "D-pad ←",
        GameInputGamepadButtons.DPadRight => "D-pad →",
        GameInputGamepadButtons.LeftShoulder => "LB",
        GameInputGamepadButtons.RightShoulder => "RB",
        GameInputGamepadButtons.LeftThumbstick => "L3",
        GameInputGamepadButtons.RightThumbstick => "R3",
        GameInputGamepadButtons.LeftTriggerButton => "LT",
        GameInputGamepadButtons.RightTriggerButton => "RT",
        GameInputGamepadButtons.LeftThumbstickUp => "LS ↑",
        GameInputGamepadButtons.LeftThumbstickDown => "LS ↓",
        GameInputGamepadButtons.LeftThumbstickLeft => "LS ←",
        GameInputGamepadButtons.LeftThumbstickRight => "LS →",
        GameInputGamepadButtons.RightThumbstickUp => "RS ↑",
        GameInputGamepadButtons.RightThumbstickDown => "RS ↓",
        GameInputGamepadButtons.RightThumbstickLeft => "RS ←",
        GameInputGamepadButtons.RightThumbstickRight => "RS →",
        GameInputGamepadButtons.PaddleLeft1 => "L P1",
        GameInputGamepadButtons.PaddleLeft2 => "L P2",
        GameInputGamepadButtons.PaddleRight1 => "R P1",
        GameInputGamepadButtons.PaddleRight2 => "R P2",

        _ => Humanize(value.ToString())
    };

    internal static string Humanize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var result = new StringBuilder(value.Length + 8);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (index > 0 && char.IsUpper(character) &&
                (char.IsLower(value[index - 1]) || index + 1 < value.Length && char.IsLower(value[index + 1])))
                result.Append(' ');
            result.Append(character);
        }
        return result.ToString();
    }

    private static bool IsSingleBit(ulong value) => value != 0 && (value & (value - 1)) == 0;
}
