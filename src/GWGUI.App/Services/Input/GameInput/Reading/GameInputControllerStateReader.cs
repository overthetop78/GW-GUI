using GWGUI.App.Constants.Input.GameInput;
using GWGUI.Emulation;

namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputControllerStateReader
{
    internal static EmulationControllerState Read(
        IGameInput? gameInput,
        GameInputDeviceEntry entry,
        IReadOnlyDictionary<string, GameInputSystemButtons> systemButtons,
        Func<Exception, bool> isInteropFailure,
        Action<object?> release)
    {
        if (gameInput is null) return EmulationControllerState.Empty with { DeviceId = entry.Id };
        IGameInputReading? reading = null;
        try
        {
            var kind = PreferredReadingKind(entry.InputKinds);
            var result = gameInput.GetCurrentReading(kind, entry.DevicePointer, out reading);
            if (result < 0 || reading is null)
            {
                GameInputDiagnostics.RecordReadFailure(entry.Id, kind, result);
                return EmulationControllerState.Empty with { DeviceId = entry.Id };
            }
            var readingKind = reading.GetInputKind();
            var gamepad = default(GameInputGamepadState);
            var hasGamepad = kind == GameInputKind.Gamepad && reading.GetGamepadState(out gamepad);
            var hasControllerArrays = kind != GameInputKind.RawDeviceReport &&
                (entry.InputKinds & GameInputKind.Controller) != 0;
            if (!hasControllerArrays)
            {
                GameInputDiagnostics.RecordControllerArraysUnavailable(
                    entry.Id, readingKind, hasGamepad);
                return hasGamepad
                    ? GameInputControllerStateMapper.MapGamepad(entry.Id, gamepad, systemButtons)
                    : EmulationControllerState.Empty with { DeviceId = entry.Id };
            }
            GameInputDiagnostics.RecordControllerReading(
                entry.Id, readingKind, hasGamepad,
                reading.GetControllerAxisCount(), reading.GetControllerButtonCount(),
                reading.GetControllerSwitchCount());
            return hasGamepad
                ? GameInputControllerStateMapper.MapGamepad(entry.Id, gamepad, systemButtons)
                : GameInputControllerStateMapper.MapController(entry.Id, reading, entry.Mapper, systemButtons);
        }
        catch (Exception exception) when (isInteropFailure(exception))
        {
            GameInputDiagnostics.RecordReadInteropFailure(entry.Id, exception);
            return EmulationControllerState.Empty with { DeviceId = entry.Id };
        }
        finally { release(reading); }
    }

    internal static unsafe GameInputLiveState ReadDetailed(
        IGameInput? gameInput,
        GameInputDeviceEntry entry,
        IReadOnlyDictionary<string, byte[]> latestRawReports,
        IReadOnlyDictionary<string, GameInputSystemButtons> systemButtons,
        Func<Exception, bool> isInteropFailure,
        Action<object?> release)
    {
        if (gameInput is null) return GameInputLiveState.Empty(entry.Id);
        IGameInputReading? reading = null;
        try
        {
            var kind = PreferredReadingKind(entry.InputKinds);
            var readingResult = gameInput.GetCurrentReading(kind, entry.DevicePointer, out reading);
            if (readingResult < 0 || reading is null)
            {
                GameInputDiagnostics.RecordDetailedReadFailure(entry.Id, kind, readingResult);
                if (entry.HidDecoder is null) return GameInputLiveState.Empty(entry.Id);
                var latest = latestRawReports.GetValueOrDefault(entry.Id) ?? [];
                return new GameInputLiveState(
                    entry.Id, 0, GameInputKind.RawDeviceReport,
                    latest.Length == 0 ? entry.HidDecoder.NeutralControls() : entry.HidDecoder.Decode(latest),
                    latest, systemButtons.GetValueOrDefault(entry.Id), null, null, null, null, true);
            }
            var readingKind = reading.GetInputKind();
            GameInputDiagnostics.RecordDetailedReading(entry.Id, readingKind);

            var axes = Array.Empty<float>();
            var buttons = Array.Empty<byte>();
            var switches = Array.Empty<int>();
            if (kind != GameInputKind.RawDeviceReport &&
                (entry.InputKinds & GameInputKind.Controller) != 0)
            {
                var axisCount = checked((int)reading.GetControllerAxisCount());
                axes = new float[axisCount];
                fixed (float* pointer = axes)
                    reading.GetControllerAxisState((uint)axisCount, (IntPtr)pointer);

                var buttonCount = checked((int)reading.GetControllerButtonCount());
                buttons = new byte[buttonCount];
                fixed (byte* pointer = buttons)
                    reading.GetControllerButtonState((uint)buttonCount, (IntPtr)pointer);

                var switchCount = checked((int)reading.GetControllerSwitchCount());
                switches = new int[switchCount];
                fixed (int* pointer = switches)
                    reading.GetControllerSwitchState((uint)switchCount, (IntPtr)pointer);
            }

            var labels = entry.Descriptor.Controls.ToDictionary(
                control => (control.Type, control.Index), control => control.Label);
            var controls = new List<GameInputControlValue>(axes.Length + buttons.Length + switches.Length);
            for (var index = 0; index < axes.Length; index++)
                controls.Add(new GameInputControlValue(
                    GameInputControlType.Axis, index,
                    labels.GetValueOrDefault((GameInputControlType.Axis, index), GameInputLabel.None),
                    axes[index]));
            for (var index = 0; index < buttons.Length; index++)
                controls.Add(new GameInputControlValue(
                    GameInputControlType.Button, index,
                    labels.GetValueOrDefault((GameInputControlType.Button, index), GameInputLabel.None),
                    buttons[index] == 0 ? 0f : 1f));
            for (var index = 0; index < switches.Length; index++)
            {
                var position = (GameInputSwitchPosition)switches[index];
                controls.Add(new GameInputControlValue(
                    GameInputControlType.Switch, index,
                    labels.GetValueOrDefault((GameInputControlType.Switch, index), GameInputLabel.None),
                    switches[index], position));
            }

            var rawBytes = ReadRawReport(reading, isInteropFailure, release);
            if (rawBytes.Count == 0 && latestRawReports.TryGetValue(entry.Id, out var latestRaw))
                rawBytes = latestRaw;
            if (entry.HidDecoder is not null)
                controls = entry.HidDecoder.Decode(rawBytes).ToList();
            GameInputArcadeStickState? arcade = null;
            if (entry.Descriptor.StandardCapabilities.HasArcadeStick &&
                reading.GetArcadeStickState(out var arcadeValue)) arcade = arcadeValue;
            GameInputFlightStickState? flight = null;
            if (entry.Descriptor.StandardCapabilities.HasFlightStick &&
                reading.GetFlightStickState(out var flightValue)) flight = flightValue;
            GameInputGamepadState? gamepad = null;
            if (entry.Descriptor.StandardCapabilities.HasGamepad &&
                reading.GetGamepadState(out var gamepadValue)) gamepad = gamepadValue;
            GameInputRacingWheelState? wheel = null;
            if (entry.Descriptor.StandardCapabilities.HasRacingWheel &&
                reading.GetRacingWheelState(out var wheelValue)) wheel = wheelValue;

            return new GameInputLiveState(
                entry.Id, reading.GetTimestamp(), reading.GetInputKind(), controls, rawBytes,
                systemButtons.GetValueOrDefault(entry.Id), arcade, flight, gamepad, wheel,
                entry.HidDecoder is not null);
        }
        catch (Exception exception) when (isInteropFailure(exception))
        {
            GameInputDiagnostics.RecordDetailedReadInteropFailure(entry.Id, exception);
            return GameInputLiveState.Empty(entry.Id);
        }
        finally { release(reading); }
    }

    internal static unsafe IReadOnlyList<byte> ReadRawReport(
        IGameInputReading reading,
        Func<Exception, bool> isInteropFailure,
        Action<object?> release)
    {
        IGameInputRawDeviceReport? report = null;
        try
        {
            reading.GetRawReport(out report);
            if (report is null)
            {
                GameInputDiagnostics.RecordMissingRawReport();
                return [];
            }
            var size = report.GetRawDataSize();
            if (size == 0 || size > GameInputConstants.MaximumRawReportByteCount)
            {
                GameInputDiagnostics.RecordRawReportSize(size);
                return [];
            }
            var bytes = new byte[checked((int)size)];
            fixed (byte* pointer = bytes)
            {
                var copied = report.GetRawData(size, (IntPtr)pointer);
                GameInputDiagnostics.RecordRawReportCopy(size, copied);
                return copied == 0 ? [] : bytes[..checked((int)Math.Min(copied, size))];
            }
        }
        catch (Exception exception) when (isInteropFailure(exception))
        {
            return [];
        }
        finally { release(report); }
    }

    private static GameInputKind PreferredReadingKind(GameInputKind kinds)
    {
        if ((kinds & GameInputKind.Gamepad) != 0) return GameInputKind.Gamepad;
        if ((kinds & GameInputKind.RacingWheel) != 0) return GameInputKind.RacingWheel;
        if ((kinds & GameInputKind.FlightStick) != 0) return GameInputKind.FlightStick;
        if ((kinds & GameInputKind.ArcadeStick) != 0) return GameInputKind.ArcadeStick;
        if ((kinds & GameInputKind.Controller) != 0) return GameInputKind.Controller;
        return GameInputKind.RawDeviceReport;
    }
}
