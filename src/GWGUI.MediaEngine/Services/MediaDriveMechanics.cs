using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces;
using C = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;

namespace GWGUI.MediaEngine.Services;

/// <summary>Suit les commandes réelles du lecteur sans posséder de thread, de flux ou de périphérique audio.</summary>
public sealed partial class MediaDriveMechanics : IMediaDriveMechanics
{
    private readonly object _sync = new();
    private readonly string _device;
    private readonly Queue<MediaDriveSoundEvent> _events = new();
    private readonly List<(MediaDriveSoundEvent Sound, long Offset)> _seekVoices = [];
    private MediaDriveState _state;
    private string? _profile;
    private double _volumePercent;
    private bool _soundEnabled;
    private bool _audibleMotor;
    private long _motorOffset;
    private long _eventOffset;
    private int _activeOperations;
    private int _activeAccesses;
    private bool _stopAfterAccess;

    /// <summary>Notifie le propriétaire audio après une nouvelle activité mécanique.</summary>
    public event Action<IMediaDriveMechanics>? AudioAvailable;

    /// <summary>Crée la mécanique d’une catégorie de lecteur avec son profil et son volume.</summary>
    public MediaDriveMechanics(string device, string? profile = null, double volumePercent = C.DefaultVolumePercent, bool soundEnabled = true)
    {
        if (RecordedMechanicalSoundFunctions.BaseProfile(device) is null) throw new ArgumentOutOfRangeException(nameof(device));
        _device = device;
        Configure(profile, volumePercent, soundEnabled);
    }

    /// <inheritdoc/>
    public MediaDriveState State { get { lock (_sync) return _state; } }

    /// <inheritdoc/>
    public bool HasPendingAudio { get { lock (_sync) return _soundEnabled && (_events.Count > 0 || _seekVoices.Count > 0 || _audibleMotor || _state.MotorRunning); } }

    /// <inheritdoc/>
    public bool IsInUse { get { lock (_sync) return _activeOperations > 0 || _activeAccesses > 0 || _state.Reading || _state.Writing; } }

    /// <inheritdoc/>
    public void BeginOperation()
    {
        lock (_sync) { _activeOperations++; _stopAfterAccess = false; }
    }

    /// <inheritdoc/>
    public void EndOperation()
    {
        lock (_sync)
        {
            if (_activeOperations > 0) _activeOperations--;
            if (_activeOperations == 0)
            {
                if (_activeAccesses == 0) SetMotorCore(false);
                else _stopAfterAccess = true;
            }
        }
        NotifyAudio();
    }

    /// <inheritdoc/>
    public void Configure(string? profile, double volumePercent, bool soundEnabled)
    {
        if (!double.IsFinite(volumePercent) || volumePercent is < 0 or > C.MaximumVolumePercent) throw new ArgumentOutOfRangeException(nameof(volumePercent));
        lock (_sync)
        {
            _profile = profile;
            _volumePercent = volumePercent;
            if (!_soundEnabled && soundEnabled && _state.MotorRunning)
            {
                _audibleMotor = true;
                _motorOffset = 0;
            }
            _soundEnabled = soundEnabled;
            if (!soundEnabled) ClearAudio();
        }
        NotifyAudio();
    }

    /// <inheritdoc/>
    public void Insert()
    {
        lock (_sync)
        {
            if (_state.MediaInserted) return;
            _state = _state with { MediaInserted = true };
            Enqueue(new(C.Insert));
        }
        NotifyAudio();
    }

    /// <inheritdoc/>
    public void Eject()
    {
        lock (_sync)
        {
            SetMotorCore(false);
            if (_state.MediaInserted)
            {
                _state = _state with { MediaInserted = false };
                Enqueue(new(C.Eject));
            }
        }
        NotifyAudio();
    }

    /// <inheritdoc/>
    public void SetMotor(bool running)
    {
        lock (_sync) { _stopAfterAccess = false; SetMotorCore(running); }
        NotifyAudio();
    }

    private void SetMotorCore(bool running)
    {
        if (_state.MotorRunning == running) return;
        _state = _state with { MotorRunning = running, Reading = false, Writing = false };
        if (running)
        {
            _audibleMotor = _soundEnabled;
            _motorOffset = 0;
        }
        else Enqueue(new(C.Stop));
    }

    /// <inheritdoc/>
    public void Seek(MediaDrivePosition position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position.Cylinder);
        ArgumentOutOfRangeException.ThrowIfNegative(position.Head);
        lock (_sync) SeekCore(position);
        NotifyAudio();
    }

    private void SeekCore(MediaDrivePosition position)
    {
        var displacement = position.Cylinder - _state.Position.Cylinder;
        if (displacement != 0)
            Enqueue(new(C.Seek, Math.Abs(displacement), Math.Sign(displacement), position.Cylinder));
        else if (position.LogicalBlock >= 0 && _state.Position.LogicalBlock >= 0
            && position.LogicalBlock != _state.Position.LogicalBlock && position.LogicalBlock != _state.Position.LogicalBlock + 1)
            Enqueue(new(C.Seek, 0, Math.Sign(position.LogicalBlock - _state.Position.LogicalBlock)));
        _state = _state with { Position = position };
    }

    /// <inheritdoc/>
    public void SetActivity(bool reading, bool writing)
    {
        lock (_sync) _state = _state with { Reading = reading, Writing = writing };
    }

    /// <inheritdoc/>
    public void Advance(MediaDrivePosition position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position.Cylinder);
        ArgumentOutOfRangeException.ThrowIfNegative(position.Head);
        lock (_sync) _state = _state with { Position = position };
    }

    /// <inheritdoc/>
    public void BeginRead(MediaDrivePosition position) => BeginAccess(position, false);

    /// <inheritdoc/>
    public void BeginWrite(MediaDrivePosition position) => BeginAccess(position, true);

    private void BeginAccess(MediaDrivePosition position, bool writing)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position.Cylinder);
        ArgumentOutOfRangeException.ThrowIfNegative(position.Head);
        lock (_sync)
        {
            SetMotorCore(true);
            SeekCore(position);
            _activeAccesses++;
            _state = _state with { Reading = !writing, Writing = writing };
        }
        NotifyAudio();
    }

    /// <inheritdoc/>
    public void EndAccess()
    {
        var stopped = false;
        lock (_sync)
        {
            if (_activeAccesses > 0) _activeAccesses--;
            if (_activeAccesses == 0)
            {
                _state = _state with { Reading = false, Writing = false };
                if (_stopAfterAccess)
                {
                    _stopAfterAccess = false;
                    SetMotorCore(false);
                    stopped = true;
                }
            }
        }
        if (stopped) NotifyAudio();
    }
    /// <inheritdoc/>
    public void Reset()
    {
        lock (_sync)
        {
            _state = default;
            _activeOperations = 0;
            _activeAccesses = 0;
            _stopAfterAccess = false;
            ClearAudio();
        }
    }

    private void Enqueue(MediaDriveSoundEvent sound)
    {
        if (!_soundEnabled) return;
        if (sound.Operation == C.Seek)
        {
            if (_seekVoices.Count == C.MaximumConcurrentSeekSounds) _seekVoices.RemoveAt(0);
            _seekVoices.Add((sound, 0));
        }
        else _events.Enqueue(sound);
    }

    private void NotifyAudio()
    {
        if (HasPendingAudio) AudioAvailable?.Invoke(this);
    }

    private void ClearAudio()
    {
        _events.Clear();
        _seekVoices.Clear();
        _audibleMotor = false;
        _eventOffset = 0;
        _motorOffset = 0;
    }
}
