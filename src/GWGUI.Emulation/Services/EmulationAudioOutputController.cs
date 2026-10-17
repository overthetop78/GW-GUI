using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Services;

internal class EmulationAudioOutputController : IDisposable
{
    protected readonly object _gate = new();
    private Func<IAudioOutput?>? _factory;
    protected IAudioOutput? _output;
    private bool _ownsOutput;
    private short[] _volumeBuffer = [];
    private int _sampleRate;
    private float _volume = 1f;
    protected bool _muted;
    protected bool _paused;
    protected bool _stopped;
    protected bool _started;
    protected bool _unavailable;

    internal EmulationAudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null)
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
            _unavailable = false;
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
            var samples = EmulationAudioFunctions.ApplyVolume(chunk.InterleavedSamples.Span, _volume,
                ref _volumeBuffer);
            try { _output!.Write(samples); }
            catch
            {
                DropOutput();
                if (!EnsureStarted()) return;
                try { _output!.Write(samples); }
                catch
                {
                    DropOutput();
                    _unavailable = true;
                }
            }
        }
    }

    internal void SetMuted(bool muted)
    {
        lock (_gate)
        {
            _muted = muted;
            if (muted) Flush();
            else
            {
                _unavailable = false;
                EnsureStarted();
            }
        }
    }

    internal void SetVolume(float volume)
    {
        lock (_gate) _volume = EmulationAudioFunctions.NormalizeVolume(volume);
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
            _unavailable = false;
            EnsureStarted();
        }
    }

    internal void Reset() { lock (_gate) Flush(); }

    internal void ReplaceFactory(Func<IAudioOutput?>? factory)
    {
        lock (_gate)
        {
            _factory = factory;
            _unavailable = false;
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
        if (_stopped || _paused || _muted || _unavailable
            || _sampleRate <= 0) return false;
        _output ??= CreateOutput();
        if (_output is null)
        {
            _unavailable = true;
            return false;
        }
        if (_started) return true;
        try
        {
            BeforeStart(_output);
            _output.Start(_sampleRate);
            _started = true;
            return true;
        }
        catch
        {
            DropOutput();
            _unavailable = true;
            return false;
        }
    }

    private IAudioOutput? CreateOutput()
    {
        try
        {
            var output = _factory?.Invoke();
            _ownsOutput = output is not null;
            return output;
        }
        catch { return null; }
    }

    private void Flush()
    {
        try { _output?.Flush(); }
        catch { DropOutput(); }
    }

    private void StopOutput()
    {
        try { if (_started) _output?.Stop(); }
        catch { }
        finally { _started = false; }
    }

    protected void DropOutput()
    {
        var output = _output;
        var ownsOutput = _ownsOutput;
        _output = null;
        _ownsOutput = false;
        try
        {
            try { if (_started) output?.Stop(); }
            finally { if (ownsOutput) output?.Dispose(); }
        }
        catch { }
        finally { _started = false; }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _stopped = true;
            try { StopOutput(); }
            finally { DropOutput(); }
        }
    }

    protected virtual void BeforeStart(IAudioOutput output) { }
}
