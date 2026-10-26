using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.CassetteConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

/// <summary>Magnétophone externe et démodulateur cassette BBC : signal FSK, commande moteur et horloge de réception.</summary>
/// <remarks>Possède le signal en mémoire, sans conserver de fichier ouvert. Le lecteur avance seulement lorsque
/// Play et le moteur MOS sont actifs. L'ACIA reste un composant distinct, emprunté à la carte.
/// La cadence du fichier est suivie par un accumulateur entier ; elle ne dépend pas du nombre d'images vidéo.</remarks>
internal sealed class Cassette(Acia6850 acia) : IEmulatorCassetteTransport
{
    private EmulationCassetteSignal? _signal;
    private int _position, _samplePhase, _sinceEdge, _receivePhase, _carrierEdges, _pulseTicks;
    private bool _level, _hasSignal, _inputBit = true;
    private byte _ula;
    private List<short>? _recorded;
    private bool _writable;
    private int _recordPhase;
    private bool _recordLevel;
    private EmulationCassetteSettings _settings = new();
    private int? _seekTarget;
    private EmulationCassetteCommand? _operation;
    private long _latencyTicks;
    internal EmulationCassetteSettings Settings => _settings;
    internal bool Seeking => _seekTarget is not null;
    public EmulationCassetteCommand? CassetteOperation => _state == EmulationCassetteState.Paused ? _operation : _seekTarget is not null ? _operation : _state == EmulationCassetteState.Recording ? EmulationCassetteCommand.Record : _state == EmulationCassetteState.Playing ? EmulationCassetteCommand.Play : null;
    internal void Configure(EmulationCassetteSettings settings)
    {
        _settings = settings; acia.BaudRateOverride = settings.BaudRate;
        if (settings.WriteProtected && _state == EmulationCassetteState.Recording) _state = EmulationCassetteState.Stopped;
        if (settings.Mode != EmulationCassettePlaybackMode.RealTime && _seekTarget is { } target) { _position = target; CompleteSeek(); }
    }
    /// <summary>Remplace l'index après sauvegarde tout en gardant la tête et l'opération du magnétophone.</summary>
    internal void RefreshIndex(EmulationCassetteSignal signal) { _signal = signal; _recorded = null; _position = Math.Min(_position, signal.Samples.Length); }
    /// <summary>Reste vrai jusqu'à confirmation de l'écriture atomique ; une pause ou un moteur arrêté ne perd aucune donnée.</summary>
    internal bool HasChanges { get; private set; }
    internal EmulationCassetteSignal RecordedSignal => new(_recorded is null ? _signal?.Samples ?? ReadOnlyMemory<short>.Empty : _recorded.ToArray(), _signal?.SampleRate ?? C.RecordingSampleRate);
    internal void MarkSaved() => HasChanges = false;
    private volatile EmulationCassetteState _state = EmulationCassetteState.Empty;
    internal int Position => _position;
    internal bool Motor => (_ula & 128) != 0;
    public EmulationCassetteState CassetteState => _state;
    public bool CassetteActive => _seekTarget is not null && _state != EmulationCassetteState.Paused || Motor && (_ula & 64) == 0 && _state is EmulationCassetteState.Playing or EmulationCassetteState.Recording;
    public IReadOnlySet<EmulationCassetteCommand> CassetteCommands => _signal is null ? new HashSet<EmulationCassetteCommand>() : _writable && !_settings.WriteProtected ? C.RecordingCommands : C.Commands;

    /// <summary>Insère le signal au début ; Play est prêt, comme sur un lecteur laissé en lecture sous contrôle moteur.</summary>
    internal void Insert(EmulationCassetteSignal signal, bool writable = false)
    {
        if (signal.SampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(signal));
        _signal = signal; _recorded = null; _writable = writable; HasChanges = false; _recordPhase = 0; _seekTarget = null;
        _position = _samplePhase = 0; _state = signal.Samples.Length == 0 ? EmulationCassetteState.Stopped : EmulationCassetteState.Playing; ResetDecoder();
    }
    internal void Eject() { _signal = null; _recorded = null; _writable = HasChanges = false; _position = _samplePhase = 0; _state = EmulationCassetteState.Empty; ResetDecoder(); }
    /// <summary>Reset électrique : ne rembobine pas une bande déjà insérée.</summary>
    internal void ResetHardware() { _ula = 0; acia.Reset(); ResetDecoder(); }
    internal void WriteUla(byte value)
    {
        var starting = !Motor && (value & 128) != 0;
        _ula = value; acia.SetCts((value & 64) != 0);
        if (starting) _latencyTicks = (long)C.CpuClock * (_state == EmulationCassetteState.Recording ? _settings.WriteLatencyMilliseconds : _settings.ReadLatencyMilliseconds) / 1000;
        if (!CassetteActive) ResetDecoder();
    }
    internal byte ReadUla() { WriteUla(0xfe); return 0; }
    private void ResetDecoder() { _sinceEdge = _receivePhase = _carrierEdges = _pulseTicks = 0; _hasSignal = false; _inputBit = true; acia.CancelReceive(); acia.SetDcd(false); }

    public void ExecuteCassette(EmulationCassetteCommand command)
    {
        if (_signal is null || !CassetteCommands.Contains(command)) throw new NotSupportedException(command.ToString());
        switch (command)
        {
            case EmulationCassetteCommand.Play: _seekTarget = null; _state = _position < Length ? EmulationCassetteState.Playing : EmulationCassetteState.EndOfTape; _latencyTicks = (long)C.CpuClock * _settings.ReadLatencyMilliseconds / 1000; break;
            case EmulationCassetteCommand.Record: _seekTarget = null; _state = EmulationCassetteState.Recording; _latencyTicks = (long)C.CpuClock * _settings.WriteLatencyMilliseconds / 1000; ResetDecoder(); break;
            case EmulationCassetteCommand.Stop: _seekTarget = null; _state = EmulationCassetteState.Stopped; ResetDecoder(); break;
            case EmulationCassetteCommand.Pause:
                if (_state == EmulationCassetteState.Paused) { _state = _operation == EmulationCassetteCommand.Record ? EmulationCassetteState.Recording : EmulationCassetteState.Playing; }
                else { _operation = CassetteOperation; _state = EmulationCassetteState.Paused; }
                break;
            case EmulationCassetteCommand.Rewind: Seek((_signal.BlockPositions ?? []).Where(position => position < _position - 1).DefaultIfEmpty(0).Max(), command); break;
            case EmulationCassetteCommand.FastForward: Seek((_signal.BlockPositions ?? []).Where(position => position > _position + 1).DefaultIfEmpty(Length).Min(), command); break;
        }
    }

    /// <summary>Avance d'un cycle CPU, détecte les demi-ondes et présente les bits à l'ACIA au débit programmé.</summary>
    internal void Tick()
    {
        acia.Tick();
        if (_seekTarget is { } target)
        {
            if (_state == EmulationCassetteState.Paused) return;
            _samplePhase += (_signal?.SampleRate ?? C.RecordingSampleRate) * _settings.WindMultiplier * _settings.SpeedPercent / 100;
            while (_samplePhase >= C.CpuClock && _position != target) { _samplePhase -= C.CpuClock; _position += Math.Sign(target - _position); }
            if (_position == target) CompleteSeek();
            return;
        }
        if (!CassetteActive || _signal is null) return;
        if (_latencyTicks > 0) { _latencyTicks--; return; }
        if (_state == EmulationCassetteState.Recording) { RecordTick(); return; }
        if (_position >= Length) { _state = EmulationCassetteState.EndOfTape; ResetDecoder(); return; }
        if (_pulseTicks > 0 && --_pulseTicks == 0) acia.SetDcd(false);
        _sinceEdge++;
        var sample = _recorded is null ? _signal.Samples.Span[_position] : _recorded[_position];
        var level = sample > 0;
        if (sample != 0 && _hasSignal && level != _level)
        {
            var duration = _sinceEdge; _sinceEdge = 0;
            if (duration >= C.CpuClock / 8000 && duration <= C.CpuClock / 1800)
            {
                var bit = duration < C.HalfWaveThreshold;
                if (!bit) _carrierEdges = 0;
                else if (++_carrierEdges == C.CarrierHalfWaves) { acia.SetDcd(true); _pulseTicks = C.CarrierPulseTicks; }
                if (_inputBit && !bit && !acia.Receiving) { acia.BeginReceive(); _receivePhase = -C.CpuClock / 4; }
                _inputBit = bit;
            }
        }
        if (sample != 0) { if (!_hasSignal) _sinceEdge = 0; _level = level; _hasSignal = true; }
        else _hasSignal = false;
        if (_sinceEdge > C.CpuClock / 600) { _carrierEdges = 0; _inputBit = true; acia.SetDcd(false); }
        if (acia.Receiving)
        {
            _receivePhase += acia.BaudRate;
            if (_receivePhase >= C.CpuClock) { _receivePhase -= C.CpuClock; acia.ReceiveBit(_inputBit); }
        }
        _samplePhase += _signal.SampleRate * _settings.SpeedPercent / 100;
        while (_samplePhase >= C.CpuClock) { _samplePhase -= C.CpuClock; _position++; }
    }

    private int Length => _recorded?.Count ?? _signal?.Samples.Length ?? 0;
    private void Seek(int target, EmulationCassetteCommand operation)
    {
        _seekTarget = target; _operation = operation; _state = EmulationCassetteState.Stopped; _samplePhase = 0; ResetDecoder();
        if (_settings.Mode != EmulationCassettePlaybackMode.RealTime || target == _position) { _position = target; CompleteSeek(); }
    }
    private void CompleteSeek() { _seekTarget = null; _operation = null; _state = _position >= Length ? EmulationCassetteState.EndOfTape : EmulationCassetteState.Stopped; _samplePhase = 0; }

    /// <summary>Réunit un fichier BBC standard validé ; le chemin direct ne devine ni les adresses ni les blocs manquants.</summary>
    internal bool TryReadFile(string requestedName, out byte[] data, out uint load, out uint execution, out int nextPosition)
    {
        data = []; load = execution = 0; nextPosition = _position;
        if (_settings.Mode != EmulationCassettePlaybackMode.Direct || _state is EmulationCassetteState.Recording or EmulationCassetteState.Paused || _signal?.Blocks is not { Count: > 0 } blocks) return false;
        for (var first = 0; first < blocks.Count; first++)
        {
            var block = blocks[first];
            if (block.SamplePosition < _position || block.Metadata.GetValueOrDefault("blockNumber") != "0" || block.IntegrityValid != true) continue;
            var name = block.Metadata.GetValueOrDefault("fileName") ?? string.Empty;
            if (requestedName.Length != 0 && !requestedName.Equals(name, StringComparison.OrdinalIgnoreCase)) continue;
            var bytes = new List<byte>();
            for (var current = first; current < blocks.Count; current++)
            {
                block = blocks[current];
                if (block.IntegrityValid != true || block.Metadata.GetValueOrDefault("fileName") != name || block.Metadata.GetValueOrDefault("blockNumber") != (current - first).ToString(System.Globalization.CultureInfo.InvariantCulture)) return false;
                bytes.AddRange(block.Data.ToArray());
                if ((Convert.ToInt32(block.Metadata["flags"], 16) & 128) == 0) continue;
                load = Convert.ToUInt32(blocks[first].Metadata["loadAddress"], 16); execution = Convert.ToUInt32(blocks[first].Metadata["executionAddress"], 16);
                data = bytes.ToArray(); nextPosition = current + 1 < blocks.Count ? blocks[current + 1].SamplePosition : Length; return true;
            }
        }
        return false;
    }
    internal void CompleteDirectRead(int position) { _position = Math.Clamp(position, 0, Length); _state = EmulationCassetteState.Stopped; ResetDecoder(); }
    /// <summary>Module TXD à 1 200/2 400 Hz et enregistre à la cadence de la bande ; le débit série vient de l'ACIA.</summary>
    /// <remarks>L'écriture remplace uniquement les échantillons sous la tête et prolonge la bande à son extrémité.
    /// Les silences, fichiers précédents et fichiers suivants restent présents. L'appelant possède la sauvegarde.</remarks>
    private void RecordTick()
    {
        _recordPhase += 2 * (acia.TransmitBit ? 2400 : 1200);
        if (_recordPhase >= C.CpuClock) { _recordPhase -= C.CpuClock; _recordLevel = !_recordLevel; }
        _samplePhase += _signal!.SampleRate * _settings.SpeedPercent / 100;
        while (_samplePhase >= C.CpuClock)
        {
            _samplePhase -= C.CpuClock;
            _recorded ??= _signal.Samples.ToArray().ToList();
            var value = (short)(_recordLevel ? 16000 : -16000);
            if (_position < _recorded.Count) _recorded[_position] = value; else _recorded.Add(value);
            _position++; HasChanges = true;
        }
    }
}
