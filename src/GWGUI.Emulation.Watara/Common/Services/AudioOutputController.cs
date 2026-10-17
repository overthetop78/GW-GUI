using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Watara.Common.Services;

internal sealed class AudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null)
    : EmulationAudioOutputController(output, factory)
{

    internal AudioOutputBufferStatus? BufferStatus
    {
        get { lock (_gate) return _started && !_muted && !_paused && !_stopped ? _output?.BufferStatus : null; }
    }

    internal void SetMinimumLatency(int milliseconds)
    {
        lock (_gate) if (_started && milliseconds > 0) _output?.TrySetMinimumLatency(milliseconds);
    }
}
