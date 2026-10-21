using GWGUI.Emulation;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Services;

/// <summary>Gestion commune de la sortie audio, du volume et de la récupération après une erreur de sortie.</summary>
/// <remarks>Une sortie reçue est empruntée ; une sortie créée par la fabrique appartient au contrôleur.</remarks>
public class EmulationAudioOutputController : IEmulationAudioOutputController
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

    /// <summary>Associe une sortie empruntée et, éventuellement, une fabrique de sorties possédées.</summary>
    public EmulationAudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null)
    {
        _output = output;
        _factory = factory;
    }

    /// <summary>Indique si les échantillons doivent être ignorés pour couper le son.</summary>
    public bool IsMuted { get { lock (_gate) return _muted; } }
    /// <summary>Volume appliqué aux échantillons avant leur écriture.</summary>
    public float Volume { get { lock (_gate) return _volume; } }

    /// <summary>Définit la fréquence et démarre la sortie si son état le permet.</summary>
    public void Start(int sampleRate)
    {
        lock (_gate)
        {
            _sampleRate = sampleRate;
            _stopped = false;
            _unavailable = false;
            EnsureStarted();
        }
    }

    /// <summary>Écrit les échantillons avec le volume courant et tente de récupérer une sortie défaillante.</summary>
    public void Write(AudioChunk chunk)
    {
        lock (_gate)
        {
            if (_stopped || _paused || _muted) return;
            if (chunk.SampleRate != _sampleRate) Restart(chunk.SampleRate);
            if (!EnsureStarted()) return;
            var samples = EmulationSdk.AudioProcessing.ApplyVolume(chunk.InterleavedSamples.Span, _volume,
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

    /// <summary>Coupe ou rétablit le son ; couper le son vide la sortie.</summary>
    public void SetMuted(bool muted)
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

    /// <summary>Applique le bornage de volume commun au SDK.</summary>
    public void SetVolume(float volume)
    {
        lock (_gate) _volume = EmulationSdk.AudioProcessing.NormalizeVolume(volume);
    }

    /// <summary>Suspend, vide et arrête la sortie sans détruire le contrôleur.</summary>
    public void Pause()
    {
        lock (_gate)
        {
            _paused = true;
            Flush();
            StopOutput();
        }
    }

    /// <summary>Réactive la sortie après une pause et autorise une nouvelle tentative de démarrage.</summary>
    public void Resume()
    {
        lock (_gate)
        {
            _paused = false;
            _unavailable = false;
            EnsureStarted();
        }
    }

    /// <summary>Vide les échantillons en attente lors d’une réinitialisation de la machine.</summary>
    public void Reset() { lock (_gate) Flush(); }

    /// <summary>Remplace la fabrique et détache la sortie courante en respectant sa propriété.</summary>
    public void ReplaceFactory(Func<IAudioOutput?>? factory)
    {
        lock (_gate)
        {
            _factory = factory;
            _unavailable = false;
            DropOutput();
            EnsureStarted();
        }
    }

    /// <summary>Arrête la sortie et empêche de nouvelles écritures jusqu’au prochain démarrage.</summary>
    public void Stop()
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

    /// <summary>Arrête la sortie et libère uniquement les sorties créées par ce contrôleur.</summary>
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
