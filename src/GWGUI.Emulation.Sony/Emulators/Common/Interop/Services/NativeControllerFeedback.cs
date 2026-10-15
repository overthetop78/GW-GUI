using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed class NativeControllerFeedback : IDisposable
{
    private const uint StrongEffect = 0;
    private const uint WeakEffect = 1;
    private readonly object _gate = new();
    private readonly ushort[,] _values = new ushort[ControllerPortConstants.MaximumControllerPortCount, 2];
    private readonly ExternalCoreApi.SetRumbleState _setState;
    private bool _disposed;

    internal NativeControllerFeedback() => _setState = SetState;

    internal bool Register(nint destination)
    {
        if (destination == nint.Zero) return false;
        lock (_gate)
        {
            if (_disposed) return false;
            Marshal.StructureToPtr(new ExternalCoreApi.RumbleInterface { SetState = Marshal.GetFunctionPointerForDelegate(_setState) }, destination, false);
            return true;
        }
    }

    internal bool SetState(uint port, uint effect, ushort strength)
    {
        if (port >= ControllerPortConstants.MaximumControllerPortCount || effect > WeakEffect) return false;
        lock (_gate)
        {
            if (_disposed) return false;
            _values[port, effect] = strength;
            return true;
        }
    }

    internal IReadOnlyList<ControllerFeedbackState> Snapshot()
    {
        lock (_gate)
        {
            if (_disposed) return [];
            var result = new List<ControllerFeedbackState>();
            for (var port = 0; port < ControllerPortConstants.MaximumControllerPortCount; port++)
            {
                if (_values[port, StrongEffect] != 0)
                    result.Add(new(port, EmulationControllerFeedbackCommands.Strong, _values[port, StrongEffect] / (float)ushort.MaxValue));
                if (_values[port, WeakEffect] != 0)
                    result.Add(new(port, EmulationControllerFeedbackCommands.Weak, _values[port, WeakEffect] / (float)ushort.MaxValue));
            }
            return result;
        }
    }

    internal void ResetPort(int port)
    {
        if (port is < 0 or >= ControllerPortConstants.MaximumControllerPortCount) return;
        lock (_gate)
        {
            _values[port, StrongEffect] = 0;
            _values[port, WeakEffect] = 0;
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
            Array.Clear(_values);
        }
    }
}
