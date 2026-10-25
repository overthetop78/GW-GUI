using GWGUI.App.Functions.Emulation.Controllers;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Services.Logging;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.App.Controllers.Emulation.Machine;

internal sealed class MachineControllerFeedbackController(IGameInputControllerSource source, Action<Exception>? logError = null) : IDisposable
{
    private readonly HashSet<string> _activeDevices = new(StringComparer.Ordinal);
    private readonly Dictionary<string, MotorValues> _sent = new(StringComparer.Ordinal);
    private readonly HashSet<string> _loggedErrors = new(StringComparer.Ordinal);
    private bool _disposed;

    internal void Update(IEmulationControllerFeedbackSource? feedback, bool active)
    {
        if (_disposed) return;
        try
        {
            var outputs = active ? feedback?.ControllerOutputs : null;
            if (outputs is not { Count: > 0 }) { Reset(); return; }
            var devices = source.GetConnectedDevices().ToDictionary(device => device.Id, StringComparer.Ordinal);
            var next = new Dictionary<string, MotorValues>(StringComparer.Ordinal);
            foreach (var output in outputs)
            {
                var flag = ControllerFeedbackFunctions.PhysicalMotor(output.Motor);
                if (string.IsNullOrWhiteSpace(output.DeviceId) || !float.IsFinite(output.Strength) || output.Strength is <= 0 or > 1
                    || flag == GameInputRumbleMotors.None || !devices.TryGetValue(output.DeviceId, out var device) || !device.RumbleMotors.HasFlag(flag)) continue;
                var values = next.GetValueOrDefault(output.DeviceId);
                next[output.DeviceId] = values.With(output.Motor, output.Strength);
            }
            foreach (var id in _activeDevices.Where(id => !next.ContainsKey(id)).ToArray()) Send(id, default);
            foreach (var item in next)
                if (!_sent.TryGetValue(item.Key, out var previous) || previous != item.Value)
                    if (!Send(item.Key, item.Value)) Send(item.Key, default);
        }
        catch (Exception error)
        {
            try { Log(error); }
            finally { Reset(); }
        }
    }

    internal void Reset()
    {
        foreach (var id in _activeDevices.ToArray()) Send(id, default);
    }

    private bool Send(string id, MotorValues values)
    {
        _activeDevices.Add(id);
        try
        {
            if (!source.SetRumble(id, values.Low, values.High, values.Left, values.Right)) { _sent.Remove(id); return false; }
            if (values == default) { _activeDevices.Remove(id); _sent.Remove(id); }
            else _sent[id] = values;
            return true;
        }
        catch (Exception error)
        {
            _sent.Remove(id);
            Log(error);
            return false;
        }
    }

    private void Log(Exception error)
    {
        if (!_loggedErrors.Add($"{error.GetType().FullName}:{error.HResult}")) return;
        try
        {
            if (logError is not null) logError(error);
            else ErrorLog.WriteWithoutPublishing(error, "Emulation controller feedback");
        }
        catch (Exception) { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        try { Reset(); }
        finally { _disposed = true; }
    }

    private readonly record struct MotorValues(float Low, float High, float Left, float Right)
    {
        internal MotorValues With(EmulationControllerMotor motor, float strength) => motor switch
        {
            EmulationControllerMotor.LowFrequency => this with { Low = Math.Max(Low, strength) },
            EmulationControllerMotor.HighFrequency => this with { High = Math.Max(High, strength) },
            EmulationControllerMotor.LeftTrigger => this with { Left = Math.Max(Left, strength) },
            EmulationControllerMotor.RightTrigger => this with { Right = Math.Max(Right, strength) },
            _ => this
        };
    }
}
