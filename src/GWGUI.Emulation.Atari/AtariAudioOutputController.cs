using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal sealed class AtariAudioOutputController : IDisposable
{
    private readonly object _gate = new();
    private Func<IAudioOutput?>? _factory;
    private IAudioOutput? _output;
    private short[] _volumeBuffer = [];
    private int _sampleRate;
    private float _volume = AtariAudioOutputConstants.DefaultVolume;
    private bool _muted;
    private bool _paused;
    private bool _stopped;
    private bool _started;

    internal AtariAudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null)
    {
        _output = output;
        _factory = factory;
    }

    internal bool IsMuted { get { lock (_gate) return _muted; } }
    internal float Volume { get { lock (_gate) return _volume; } }

    internal void Start(int sampleRate)
    {
        lock (_gate)
        {
            _sampleRate = sampleRate;
            _stopped = false;
            EnsureStarted();
        }
    }

    internal void Write(AudioChunk chunk)
    {
        lock (_gate)
        {
            if (_stopped || _paused || _muted) return;
            if (chunk.SampleRate != _sampleRate) Restart(chunk.SampleRate);
            if (!EnsureStarted()) return;
            var samples = AtariAudioOutputFunctions.ApplyVolume(chunk.InterleavedStereo.Span, _volume,
                ref _volumeBuffer);
            try { _output!.Write(samples); }
            catch
            {
                DropOutput();
                if (!EnsureStarted()) return;
                try { _output!.Write(samples); }
                catch { DropOutput(); }
            }
        }
    }

    internal void SetMuted(bool muted)
    {
        lock (_gate)
        {
            _muted = muted;
            if (muted) Flush();
            else EnsureStarted();
        }
    }

    internal void SetVolume(float volume)
    {
        lock (_gate) _volume = AtariAudioOutputFunctions.NormalizeVolume(volume);
    }

    internal void Pause()
    {
        lock (_gate)
        {
            _paused = true;
            Flush();
            StopOutput();
        }
    }

    internal void Resume()
    {
        lock (_gate)
        {
            _paused = false;
            EnsureStarted();
        }
    }

    internal void Reset() { lock (_gate) Flush(); }

    internal void ReplaceFactory(Func<IAudioOutput?>? factory)
    {
        lock (_gate)
        {
            _factory = factory;
            DropOutput();
            EnsureStarted();
        }
    }

    internal void Stop()
    {
        lock (_gate)
        {
            _stopped = true;
            StopOutput();
        }
    }

    private void Restart(int sampleRate)
    {
        _sampleRate = sampleRate;
        StopOutput();
    }

    private bool EnsureStarted()
    {
        if (_stopped || _paused || _muted || _sampleRate <= AtariMachineConstants.InvalidSampleRate) return false;
        _output ??= CreateOutput();
        if (_output is null) return false;
        if (_started) return true;
        try
        {
            _output.Start(_sampleRate);
            _started = true;
            return true;
        }
        catch
        {
            DropOutput();
            return false;
        }
    }

    private IAudioOutput? CreateOutput()
    {
        try { return _factory?.Invoke(); }
        catch { return null; }
    }

    private void Flush()
    {
        try { _output?.Flush(); }
        catch { DropOutput(); }
    }

    private void StopOutput()
    {
        try { _output?.Stop(); }
        catch { }
        _started = false;
    }

    private void DropOutput()
    {
        try { _output?.Dispose(); }
        catch { }
        _output = null;
        _started = false;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _stopped = true;
            StopOutput();
            DropOutput();
        }
    }
}
