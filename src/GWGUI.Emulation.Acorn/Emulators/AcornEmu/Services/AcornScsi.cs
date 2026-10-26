using Phase = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Enums.AcornScsiPhase;
using System.IO;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.AcornScsiConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Interface Winchester Acorn : sélection et phases SCSI/SASI, sans conteneur ni fichier possédé.</summary>
/// <remarks>Le propriétaire sérialise les accès et avance Tick avec l'horloge BBC. Les images sont empruntées.
/// Les blocs du bus font 256 octets ; les secteurs du conteneur peuvent être plus grands.</remarks>
internal sealed partial class AcornScsi
{
    private readonly Func<int, IEmulationDiskImage?> _disk;
    private readonly int _clock;
    private readonly byte[] _command = new byte[16], _sense = new byte[C.UnitCount];
    private readonly long[] _senseAddress = new long[C.UnitCount];
    private readonly long[] _headPositions = new long[C.UnitCount], _seekCount = new long[C.UnitCount], _readBlocks = new long[C.UnitCount], _writtenBlocks = new long[C.UnitCount];
    internal long SeekCount(int unit) => _seekCount[unit];
    internal long ReadBlocks(int unit) => _readBlocks[unit];
    internal long WrittenBlocks(int unit) => _writtenBlocks[unit];
    private readonly int[] _latencies = [2, 2, 2, 2], _speeds = [100, 100, 100, 100];
    private readonly int[] _writeLatencies = [2, 2, 2, 2];
    private Phase _phase;
    private int _commandCount, _commandLength, _unit, _offset, _remaining, _delay, _requestDelay, _activityTicks;
    private long _lba;
    private byte[] _buffer = [];
    private byte _lastData, _result;
    private bool _irq, _select, _sectorTransfer;
    internal bool Irq => _irq;
    internal bool Busy => _phase != Phase.Free;
    internal int Unit => _unit;
    internal bool Writing => _phase is Phase.Write or Phase.Formatting;
    internal bool Active(int unit) => unit == _unit && (_activityTicks > 0 || _phase is Phase.Executing or Phase.Read or Phase.Write or Phase.Formatting);
    private IEmulationDiskImage? Image => _unit < C.UnitCount ? _disk(_unit) : null;

    internal AcornScsi(Func<int, IEmulationDiskImage?> disk, int ticksPerSecond = C.CpuClock)
    {
        ArgumentNullException.ThrowIfNull(disk); ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ticksPerSecond);
        _disk = disk; _clock = ticksPerSecond; Reset();
    }

    internal void Configure(int unit, int latencyMilliseconds, int speedPercent, int? writeLatencyMilliseconds = null)
    {
        if (unit is < 0 or >= C.UnitCount || latencyMilliseconds is < 0 or > 60_000 || writeLatencyMilliseconds is < 0 or > 60_000 || speedPercent < 0) throw new ArgumentOutOfRangeException(nameof(unit));
        _latencies[unit] = latencyMilliseconds; _writeLatencies[unit] = writeLatencyMilliseconds ?? latencyMilliseconds; _speeds[unit] = speedPercent;
    }

    /// <summary>Libère le bus et les interruptions ; les disques et leurs écritures restent au propriétaire.</summary>
    internal void Reset()
    {
        _phase = Phase.Free; _commandCount = _unit = _offset = _remaining = _delay = _requestDelay = _activityTicks = 0;
        _irq = _select = false; _buffer = []; _lastData = _result = 0; _lba = 0;
        Array.Clear(_sense); Array.Clear(_senseAddress);
    }

    internal byte Read(int register) => (register & 3) switch { 0 => ReadData(), 1 => StatusBits(), _ => 255 };
    private byte StatusBits()
    {
        var bits = _irq ? 0x10 : 0;
        if (_phase != Phase.Free) bits |= 2;
        if (_phase is Phase.Command or Phase.Status or Phase.Message) bits |= 0x80;
        if (_phase is Phase.Read or Phase.Status or Phase.Message) bits |= 0x40;
        if (_phase is Phase.Free or Phase.Selection || _phase is not (Phase.Executing or Phase.Formatting) && _requestDelay == 0) bits |= 0x20;
        if (_phase == Phase.Message) bits |= 1;
        return (byte)bits;
    }

    internal void Write(int register, byte value)
    {
        switch (register & 3)
        {
            case 0: _select = true; WriteData(value); break;
            case 1: _select = true; break;
            case 2: _select = false; WriteData(value); break;
            case 3: _select = true; _irq = value == 255; break;
        }
    }

    private void WriteData(byte value)
    {
        _lastData = value;
        switch (_phase)
        {
            case Phase.Free:
                if (_select) _phase = Phase.Selection;
                return;
            case Phase.Selection:
                if (!_select) { _phase = Phase.Command; _commandCount = 0; _commandLength = 6; _requestDelay = 0; }
                return;
            case Phase.Command:
                if (_requestDelay > 0) return;
                _command[_commandCount++] = value;
                if (_commandCount == 1) _commandLength = (value >> 5) switch { 1 or 2 => 10, 4 => 16, 5 => 12, _ => 6 };
                _requestDelay = 2;
                if (_commandCount == _commandLength) BeginCommand();
                return;
            case Phase.Write:
                if (_requestDelay > 0) return;
                _buffer[_offset++] = value; _requestDelay = 2;
                if (_offset == _buffer.Length) WriteBuffer();
                return;
            default: return;
        }
    }

    private byte ReadData()
    {
        if (_requestDelay > 0) return _lastData;
        switch (_phase)
        {
            case Phase.Read:
                _lastData = _buffer[_offset++]; _requestDelay = 2;
                if (_offset == _buffer.Length)
                {
                    if (_sectorTransfer && --_remaining > 0) { _lba++; PrepareRead(); }
                    else Finish();
                }
                return _lastData;
            case Phase.Status: _phase = Phase.Message; _requestDelay = 2; return _result;
            case Phase.Message: _phase = Phase.Free; _irq = false; _buffer = []; return 0;
            case Phase.Selection: _phase = Phase.Free; _irq = false; return _lastData;
            default: return _lastData;
        }
    }

    internal void Tick()
    {
        if (_activityTicks > 0) _activityTicks--;
        if (_requestDelay > 0) _requestDelay--;
        if (_phase is not (Phase.Executing or Phase.Formatting) || _delay > 0 && --_delay > 0) return;
        if (_phase == Phase.Executing) Execute(); else FormatNextBlock();
    }

    private void Finish(byte error = 0)
    {
        if (error != 0 && _unit < C.UnitCount) { _sense[_unit] = error; _senseAddress[_unit] = _lba; }
        _result = (byte)((_unit << 5) | (error != 0 ? C.CheckCondition : 0)); _phase = Phase.Status; _requestDelay = 0;
        _activityTicks = _clock / 20;
    }
    private static bool MediaFailure(Exception error) => error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException;
}
