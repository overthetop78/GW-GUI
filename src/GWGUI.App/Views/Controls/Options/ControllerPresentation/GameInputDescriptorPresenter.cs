using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using System.Globalization;

namespace GWGUI.App.Views.Controls.Options.ControllerPresentation;

internal sealed record ControllerDetailRow(string Label, string Value);
internal sealed record ControllerAnalogRow(string Label, string Value);

internal static class GameInputDescriptorPresenter
{
    internal static IReadOnlyList<ControllerDetailRow> Capabilities(GameInputDeviceDescriptor device)
    {
        var standard = device.StandardCapabilities;
        var rows = new List<ControllerDetailRow>
        {
            new(LocExtension.Get("Controllers.Device"), GameInputDisplayFormatter.Flags(device.Status)),
            new(LocExtension.Get("Controllers.InputKinds"), GameInputDisplayFormatter.Flags(device.SupportedInput)),
            new(LocExtension.Get("Controllers.Controls"), ControlCounts(device)),
            new(LocExtension.Get("Controllers.Rumble"), Rumble(device)),
            new(LocExtension.Get("Controllers.ForceFeedback"), ForceFeedback(device)),
            new(LocExtension.Get("Controllers.Haptics"), Bool(device.HasHaptics)),
            new(LocExtension.Get("Controllers.SystemButtons"), GameInputDisplayFormatter.Flags(device.SystemButtons)),
            new(LocExtension.Get("Controllers.RawReports"), Reports(device))
        };
        if (standard.HasGamepad)
            rows.Add(new(LocExtension.Get("Controllers.GamepadLayout"), GameInputDisplayFormatter.Flags(standard.GamepadLayout)));
        if (standard.HasRacingWheel)
            rows.Add(new(LocExtension.Get("Controllers.WheelCapabilities"),
                LocExtension.Get("Controllers.WheelDetails", standard.RacingWheelMaxAngle,
                    Bool(standard.RacingWheelHasClutch), Bool(standard.RacingWheelHasHandbrake),
                    Bool(standard.RacingWheelHasPatternShifter))));
        var extras = ExtraControls(standard);
        if (!string.IsNullOrEmpty(extras)) rows.Add(new(LocExtension.Get("Controllers.ExtraControls"), extras));
        if (!string.IsNullOrWhiteSpace(device.HapticAudioEndpointId))
            rows.Add(new(LocExtension.Get("Controllers.HapticEndpoint"), device.HapticAudioEndpointId));
        if (device.HapticLocations.Count != 0)
            rows.Add(new(LocExtension.Get("Controllers.HapticLocations"), string.Join(" · ", device.HapticLocations)));
        return rows;
    }

    internal static string Identity(GameInputDeviceDescriptor device)
    {
        var lines = new List<string>
        {
            $"{LocExtension.Get("Controllers.GameInputName")}: {device.GameInputDisplayName}",
            $"VID:PID: {device.VidPid}",
            $"{LocExtension.Get("Controllers.Revision")}: {device.RevisionNumber}",
            $"{LocExtension.Get("Controllers.HardwareVersion")}: {device.HardwareVersion}",
            $"{LocExtension.Get("Controllers.FirmwareVersion")}: {device.FirmwareVersion}",
            $"{LocExtension.Get("Controllers.Family")}: {GameInputDisplayFormatter.Family(device.Family)}",
            $"{LocExtension.Get("Controllers.Usage")}: {GameInputDisplayFormatter.Usage(device.Usage)}",
            $"DeviceId: {TrimPrefix(device.Id)}",
            $"DeviceRootId: {device.DeviceRootId}",
            $"ContainerId: {device.ContainerId}",
            $"PnP: {device.PnpPath}",
            $"{LocExtension.Get("Controllers.Manufacturer")}: {device.Manufacturer}"
        };
        if (device.WindowsIdentityChain.Count != 0)
        {
            lines.Add(string.Empty);
            lines.Add(LocExtension.Get("Controllers.WindowsIdentity"));
            lines.AddRange(device.WindowsIdentityChain);
        }
        return string.Join(Environment.NewLine, lines);
    }

    internal static IReadOnlyList<ControllerAnalogRow> Analog(GameInputLiveState state)
    {
        var rows = new List<ControllerAnalogRow>();
        if (state.Gamepad is { } gamepad)
        {
            Add(rows, "Controllers.LeftStickX", gamepad.LeftThumbstickX);
            Add(rows, "Controllers.LeftStickY", gamepad.LeftThumbstickY);
            Add(rows, "Controllers.RightStickX", gamepad.RightThumbstickX);
            Add(rows, "Controllers.RightStickY", gamepad.RightThumbstickY);
            Add(rows, "Controllers.LeftTrigger", gamepad.LeftTrigger);
            Add(rows, "Controllers.RightTrigger", gamepad.RightTrigger);
        }
        if (state.RacingWheel is { } wheel)
        {
            Add(rows, "Controllers.Steering", wheel.Wheel);
            Add(rows, "Controllers.Throttle", wheel.Throttle);
            Add(rows, "Controllers.Brake", wheel.Brake);
            Add(rows, "Controllers.Clutch", wheel.Clutch);
            Add(rows, "Controllers.Handbrake", wheel.Handbrake);
            rows.Add(new(LocExtension.Get("Controllers.Gear"), wheel.PatternShifterGear.ToString(CultureInfo.CurrentCulture)));
        }
        if (state.FlightStick is { } flight)
        {
            Add(rows, "Controllers.Roll", flight.Roll);
            Add(rows, "Controllers.Pitch", flight.Pitch);
            Add(rows, "Controllers.Yaw", flight.Yaw);
            Add(rows, "Controllers.Throttle", flight.Throttle);
        }
        if (rows.Count == 0)
            foreach (var control in state.Controls.Where(control => control.Type == GameInputControlType.Axis))
                rows.Add(new(GameInputDisplayFormatter.ControlName(control),
                    control.Value.ToString("0.000", CultureInfo.CurrentCulture)));
        return rows;
    }

    private static string ControlCounts(GameInputDeviceDescriptor device)
    {
        var axes = device.Controls.Count(control => control.Type == GameInputControlType.Axis);
        var buttons = device.Controls.Count(control => control.Type == GameInputControlType.Button);
        var switches = device.Controls.Count(control => control.Type == GameInputControlType.Switch);
        var summary = LocExtension.Get("Controllers.ControlCounts", axes, buttons, switches);
        if (device.Controls.Count == 0) return summary;
        var definitions = device.Controls.Select(GameInputDisplayFormatter.ControlDefinition);
        return summary + Environment.NewLine + string.Join(Environment.NewLine, definitions);
    }

    private static string Rumble(GameInputDeviceDescriptor device)
    {
        if (device.RumbleMotors == GameInputRumbleMotors.None)
            return LocExtension.Get("Controllers.None");
        var count = System.Numerics.BitOperations.PopCount((uint)device.RumbleMotors);
        return $"{count} · {GameInputDisplayFormatter.Flags(device.RumbleMotors)}";
    }

    private static string ForceFeedback(GameInputDeviceDescriptor device) => device.ForceFeedbackMotors.Count == 0
        ? LocExtension.Get("Controllers.None")
        : $"{device.ForceFeedbackMotors.Count} · " +
          string.Join(Environment.NewLine, device.ForceFeedbackMotors.Select(GameInputDisplayFormatter.FeedbackMotor));

    private static string Reports(GameInputDeviceDescriptor device)
    {
        var summary = LocExtension.Get("Controllers.ReportCounts", device.InputReports.Count, device.OutputReports.Count);
        var details = device.InputReports.Select((report, index) =>
                $"{LocExtension.Get("Controllers.Enum.InputReport")} #{index + 1}: ID={report.Id}, {report.Size} B")
            .Concat(device.OutputReports.Select((report, index) =>
                $"{LocExtension.Get("Controllers.Enum.OutputReport")} #{index + 1}: ID={report.Id}, {report.Size} B"));
        var text = string.Join(" · ", details);
        return string.IsNullOrEmpty(text) ? summary : $"{summary} · {text}";
    }

    private static string ExtraControls(GameInputStandardCapabilities standard)
    {
        var parts = new List<string>();
        foreach (var (kind, indexes) in standard.ExtraAxisIndexes)
            if (indexes.Count != 0) parts.Add($"{GameInputDisplayFormatter.EnumValue(kind)} {LocExtension.Get("Controllers.Enum.Axes")}: {string.Join(", ", indexes.Select(value => value + 1))}");
        foreach (var (kind, indexes) in standard.ExtraButtonIndexes)
            if (indexes.Count != 0) parts.Add($"{GameInputDisplayFormatter.EnumValue(kind)} {LocExtension.Get("Controllers.Enum.Buttons")}: {string.Join(", ", indexes.Select(value => value + 1))}");
        return string.Join(" · ", parts);
    }

    private static void Add(ICollection<ControllerAnalogRow> rows, string key, float value) =>
        rows.Add(new(LocExtension.Get(key), value.ToString("0.000", CultureInfo.CurrentCulture)));
    private static string Bool(bool value) => LocExtension.Get(value ? "Controllers.Yes" : "Controllers.No");
    private static string TrimPrefix(string value) => value.StartsWith("gameinput:", StringComparison.OrdinalIgnoreCase)
        ? value["gameinput:".Length..] : value;
}
