using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Emulation.Services;

/// <summary>Stockage synchronisé des moteurs, repris du récepteur natif existant.</summary>
internal sealed class EmulationControllerFeedbackAccumulator : IEmulationControllerFeedbackAccumulator
{
    private const uint StrongEffect = 0;
    private const uint WeakEffect = 1;
    private readonly object _gate = new();
    private readonly ushort[,] _values;
    private bool _disposed;

    internal EmulationControllerFeedbackAccumulator(int maximumPortCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumPortCount);
        _values = new ushort[maximumPortCount, 2];
    }

    public bool SetState(uint port, uint effect, ushort strength)
    {
        if (port >= _values.GetLength(0) || effect > WeakEffect) return false;
        lock (_gate)
        {
            if (_disposed) return false;
            _values[port, effect] = strength;
            return true;
        }
    }

    public IReadOnlyList<ControllerFeedbackState> Snapshot()
    {
        lock (_gate)
        {
            if (_disposed) return [];
            var result = new List<ControllerFeedbackState>();
            for (var port = 0; port < _values.GetLength(0); port++)
            {
                if (_values[port, StrongEffect] != 0)
                    result.Add(new(port, EmulationControllerFeedbackCommands.Strong, _values[port, StrongEffect] / (float)ushort.MaxValue));
                if (_values[port, WeakEffect] != 0)
                    result.Add(new(port, EmulationControllerFeedbackCommands.Weak, _values[port, WeakEffect] / (float)ushort.MaxValue));
            }
            return result;
        }
    }

    public void ResetPort(int port)
    {
        if (port < 0 || port >= _values.GetLength(0)) return;
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
