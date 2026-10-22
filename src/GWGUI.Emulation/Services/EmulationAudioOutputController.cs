using GWGUI.Emulation;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Services;

/// <summary>Gestion commune de la sortie audio, du volume et de la récupération après une erreur de sortie.</summary>
/// <remarks>Une sortie reçue est empruntée ; une sortie créée par la fabrique appartient au contrôleur.</remarks>
internal sealed class EmulationAudioOutputController : IEmulationAudioOutputController
{
    private readonly object _gate = new();
    private Func<IAudioOutput?>? _factory;
    private IAudioOutput? _output;
    private bool _ownsOutput;
    private short[] _volumeBuffer = [];
    private int _sampleRate;
    private EmulationAudioChannelLayout _channelLayout = EmulationAudioChannelLayout.Stereo;
    private int _minimumLatencyMilliseconds;
    private float _volume = 1f;
    private bool _muted;
    private bool _paused;
    private bool _stopped;
    private bool _started;
    private bool _unavailable;

    /// <summary>Associe une sortie empruntée et, éventuellement, une fabrique de sorties possédées.</summary>
    internal EmulationAudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null)
    {
        _output = output;
        _factory = factory;
    }

    /// <summary>Indique si les échantillons doivent être ignorés pour couper le son.</summary>
    public bool IsMuted { get { lock (_gate) return _muted; } }
    /// <summary>Volume appliqué aux échantillons avant leur écriture.</summary>
    public float Volume { get { lock (_gate) return _volume; } }

    /// <summary>Expose le tampon de la sortie en tenant compte de l’état effectif du contrôleur.</summary>
    /// <remarks>L’instantané reste consultable quand la sortie est inactive. Une erreur détache la sortie
    /// selon sa propriété et marque le contrôleur indisponible jusqu’à sa prochaine reprise.</remarks>
    public AudioOutputBufferStatus? BufferStatus
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

    /// <summary>Applique et mémorise la latence minimale demandée, en millisecondes.</summary>
    /// <param name="latencyMilliseconds">Valeur positive ou nulle ; zéro retire la demande minimale.</param>
    /// <returns>True si la valeur est mémorisée, false si elle est négative ou si la sortie la refuse.</returns>
    /// <remarks>Sans sortie physique, la valeur est conservée et sera appliquée avant son démarrage.
    /// Une erreur suit la même règle de détachement et de récupération que les autres opérations audio.</remarks>
    public bool TrySetMinimumLatency(int latencyMilliseconds)
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

    /// <summary>Définit la fréquence et démarre la sortie si son état le permet.</summary>
    public void Start(int sampleRate) => Start(sampleRate, EmulationAudioChannelLayout.Stereo);

    /// <summary>Démarre la sortie avec la disposition des canaux fournie par le cœur.</summary>
    /// <param name="sampleRate">Fréquence des frames audio en hertz.</param>
    /// <param name="channelLayout">Disposition des échantillons entrelacés dans chaque frame.</param>
    /// <remarks>Un changement de format arrête la lecture précédente avant de redémarrer la même sortie.
    /// Le contrôleur conserve ses règles de pause, coupure, propriété et récupération.</remarks>
    public void Start(int sampleRate, EmulationAudioChannelLayout channelLayout)
    {
        lock (_gate)
        {
            if (_started && (sampleRate != _sampleRate || channelLayout != _channelLayout)) StopOutput();
            _sampleRate = sampleRate;
            _channelLayout = channelLayout;
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
            if (chunk.SampleRate != _sampleRate || chunk.ChannelLayout != _channelLayout) Restart(chunk.SampleRate, chunk.ChannelLayout);
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
            _paused = false;
            StopOutput();
        }
    }

    private void Restart(int sampleRate, EmulationAudioChannelLayout channelLayout)
    {
        _sampleRate = sampleRate;
        _channelLayout = channelLayout;
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
            if (_minimumLatencyMilliseconds > 0) _output.TrySetMinimumLatency(_minimumLatencyMilliseconds);
            if (_output is IMultichannelAudioOutput multichannel) multichannel.Start(_sampleRate, _channelLayout);
            else if (_channelLayout == EmulationAudioChannelLayout.Stereo) _output.Start(_sampleRate);
            else throw new NotSupportedException(nameof(IMultichannelAudioOutput));
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

    private void DropOutput()
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
}
