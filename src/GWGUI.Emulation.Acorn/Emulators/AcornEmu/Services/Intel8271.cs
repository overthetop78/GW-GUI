using Phase = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Enums.Intel8271Phase;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Intel8271Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Contrôleur disquette 8271 : registres, séquence de commandes et demandes de transfert non DMA.</summary>
/// <remarks>Les images sont empruntées au propriétaire de la carte ; le composant ne les sauvegarde ni ne les libère.
/// Son horloge est avancée par la carte, sans thread hôte. Les conteneurs sont accessibles uniquement par le SDK.</remarks>
internal sealed partial class Intel8271
{
    private readonly Func<int, IEmulationDiskImage?> _disk;
    private readonly int _clock, _byteTicks;
    private readonly byte[] _parameters = new byte[C.ParameterCapacity], _tracks = new byte[2];
    private readonly long[] _seekSteps = new long[2];
    private readonly long[] _readBytes = new long[2], _writtenBytes = new long[2];
    internal long SeekSteps(int drive) => _seekSteps[drive];
    internal long ReadBytes(int drive) => _readBytes[drive];
    internal long WrittenBytes(int drive) => _writtenBytes[drive];
    private byte _status, _result, _data, _command, _driveOutput, _mode;
    private int _parameterCount, _requiredParameters, _drive, _sector, _remaining, _sectorSize, _offset;
    private int _delay, _stepTicks, _settleTicks, _loadTicks, _unloadTicks, _motorTicks, _rotation;
    private byte[] _buffer = [];
    private bool _awaitingData, _crcError;
    private Phase _phase;
    internal bool Nmi => (_status & C.Interrupt) != 0;
    internal bool Motor => _motorTicks > 0;
    internal int ActiveDrive => _drive;
    internal bool Reading => _phase is Phase.Read or Phase.Id or Phase.Verify;
    internal bool Writing => _phase is Phase.Write or Phase.Format;
    internal bool Active => _phase != Phase.Idle;
    /// <summary>Accélère les délais mécaniques ; le transfert non DMA conserve son horloge matérielle.</summary>
    internal int MechanicalSpeedPercent { get; set; } = 100;
    private int Side => (_driveOutput & C.SideSelect) != 0 ? 1 : 0;

    internal Intel8271(Func<int, IEmulationDiskImage?> disk, int ticksPerSecond = 2_000_000)
    {
        ArgumentNullException.ThrowIfNull(disk);
        ArgumentOutOfRangeException.ThrowIfLessThan(ticksPerSecond, C.FmBytesPerSecond);
        _disk = disk; _clock = ticksPerSecond; _byteTicks = ticksPerSecond / C.FmBytesPerSecond;
        Reset();
    }

    /// <summary>Réinitialise le contrôleur ; les médias et leurs changements appartiennent toujours au cœur.</summary>
    internal void Reset()
    {
        _status = _result = _data = _command = _driveOutput = _mode = 0;
        _parameterCount = _requiredParameters = _drive = _sector = _remaining = _offset = _delay = _motorTicks = _rotation = 0;
        _stepTicks = _clock * 6 / 1000; _settleTicks = _clock * 2 / 1000; _loadTicks = _clock / 5; _unloadTicks = _clock * 3;
        Array.Clear(_tracks); Array.Clear(_parameters); _buffer = []; _awaitingData = _crcError = false; _phase = Phase.Idle;
    }

    internal byte Read(int register)
    {
        switch (register & 7)
        {
            case 0: return _status;
            case 1: _status &= unchecked((byte)~(C.ResultFull | C.Interrupt)); return _result;
            case 4:
                if (_awaitingData && Reading) _readBytes[_drive]++;
                _status &= unchecked((byte)~(C.Interrupt | C.NonDma)); _awaitingData = false;
                return _data;
            default: return 0;
        }
    }

    internal void Write(int register, byte value)
    {
        switch (register & 7)
        {
            case 0: Command(value); break;
            case 1:
                if ((_status & C.Busy) == 0 || _parameterCount >= _requiredParameters) return;
                _parameters[_parameterCount++] = value;
                if (_parameterCount == _requiredParameters) BeginCommand();
                break;
            case 2: if ((value & 1) != 0) Reset(); break;
            case 4:
                _data = value;
                if (_awaitingData && _phase is Phase.Write or Phase.Format && _offset < _buffer.Length) { _buffer[_offset++] = value; _writtenBytes[_drive]++; }
                _status &= unchecked((byte)~(C.Interrupt | C.NonDma)); _awaitingData = false;
                break;
        }
    }

    /// <summary>Avance moteurs, positionnement et échéances des octets ; un octet non acquitté produit Late DMA.</summary>
    internal void Tick()
    {
        if (_motorTicks > 0) _motorTicks--;
        _rotation = (_rotation + 1) % Math.Max(1, _clock / 5);
        if (_phase == Phase.Idle || _delay > 0 && --_delay > 0) return;
        if (_phase == Phase.Positioning) { Positioned(); return; }
        if (_awaitingData) { Complete(C.LateDma); return; }
        if (_phase == Phase.Verify) { NextSector(); return; }
        if (_offset == _buffer.Length)
        {
            if (_phase == Phase.Write && !StoreSector()) return;
            if (_phase == Phase.Format) { StoreFormattedTrack(); return; }
            NextSector(); return;
        }
        if (_phase is Phase.Read or Phase.Id) _data = _buffer[_offset++];
        _awaitingData = true; _status = C.Busy | C.Interrupt | C.NonDma; _delay = _byteTicks;
    }

    private void Complete(byte result, bool interrupt = true)
    {
        _result = result; _status = (byte)(C.ResultFull | (interrupt ? C.Interrupt : 0));
        _phase = Phase.Idle; _awaitingData = false; _buffer = []; _delay = 0;
    }
}
