using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Snk.Common.Services;

internal sealed class AudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null)
    : EmulationAudioOutputController(output, factory)
{
    private int _minimumLatencyMilliseconds;

    protected override void BeforeStart(IAudioOutput output)
    {
        if (_minimumLatencyMilliseconds > 0) output.TrySetMinimumLatency(_minimumLatencyMilliseconds);
    }
    internal AudioOutputBufferStatus? BufferStatus
    {
        get
        {
            lock (_gate)
            {
                try { return _output?.BufferStatus is { } status ? status with { IsActive = status.IsActive && _started && !_stopped && !_paused && !_muted && !_unavailable } : null; }
                catch { DropOutput(); _unavailable = true; return null; }
            }
        }
    }

    internal bool TrySetMinimumLatency(int latencyMilliseconds)
    {
        lock (_gate)
        {
            if (latencyMilliseconds < 0) return false;
            if (_minimumLatencyMilliseconds == latencyMilliseconds) return true;
            try
            {
                if (_output is not null && !_output.TrySetMinimumLatency(latencyMilliseconds)) return false;
                _minimumLatencyMilliseconds = latencyMilliseconds;
                return true;
            }
            catch { DropOutput(); _unavailable = true; return false; }
        }
    }
}
