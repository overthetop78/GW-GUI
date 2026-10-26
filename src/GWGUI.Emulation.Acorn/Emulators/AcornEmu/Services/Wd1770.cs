using System.IO;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Wd1770Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>WD1770 : protocole de registres et transferts cadencés sur les secteurs du SDK.</summary>
/// <remarks>La carte possède la sélection du lecteur et la ligne NMI ; le cœur possède les images.
/// Le composant ne crée ni fichier ni thread. Chaque Tick représente un cycle de son horloge d'entrée.</remarks>
internal sealed partial class Wd1770
{
    private enum Phase { Idle, Positioning, ReadSector, WriteSector, ReadAddress, ReadTrack, WriteTrack }
    private readonly Func<int, IEmulationDiskImage?> _disk;
    private readonly int _clock;
    private readonly int[] _physicalTracks = new int[2];
    private byte _command, _track, _sector = 1, _data, _errors;
    private int _delay, _offset, _targetTrack, _motorTicks, _rotation, _idIndex, _direction = 1;
    private byte[] _buffer = [];
    private bool _drq, _irq, _typeOne, _crcError, _forceIndex, _forceReady, _forceNotReady, _wasReady;
    private Phase _phase;
    internal int Drive { get; set; }
    internal int Side { get; set; }
    internal bool Selected { get; set; } = true;
    internal bool DoubleDensity { get; set; } = true;
    internal int MechanicalSpeedPercent { get; set; } = 100;
    internal bool Irq => _irq;
    internal bool Drq => _drq;
    internal bool Nmi => _irq || _drq;
    internal bool Motor => _motorTicks > 0;
    internal bool Active => _phase != Phase.Idle;
    internal bool Writing => _phase is Phase.WriteSector or Phase.WriteTrack;
    private IEmulationDiskImage? Image => Selected && Drive is >= 0 and < 2 ? _disk(Drive) : null;
    private int ByteTicks => Math.Max(1, _clock / (DoubleDensity ? C.DoubleDensityBytesPerSecond : C.SingleDensityBytesPerSecond));
    private int RevolutionTicks => _clock / C.RevolutionsPerSecond;

    internal Wd1770(Func<int, IEmulationDiskImage?> disk, int ticksPerSecond = C.CpuClock)
    {
        ArgumentNullException.ThrowIfNull(disk); ArgumentOutOfRangeException.ThrowIfLessThan(ticksPerSecond, C.DoubleDensityBytesPerSecond);
        _disk = disk; _clock = ticksPerSecond; Reset();
    }

    /// <summary>Réinitialise l'électronique ; la position mécanique des têtes et les images restent en place.</summary>
    internal void Reset()
    {
        _command = _track = _data = _errors = 0; _sector = 1;
        _phase = Phase.Idle; _buffer = []; _delay = _offset = _motorTicks = _rotation = _idIndex = 0;
        _drq = _irq = _crcError = _forceIndex = _forceReady = _forceNotReady = false; _typeOne = true; _wasReady = Image is not null;
    }

    internal byte Read(int register)
    {
        switch (register & 3)
        {
            case 0:
                _irq = false;
                var status = (byte)(_errors | (Active ? C.Busy : 0) | (Motor ? C.Motor : 0));
                if (_typeOne)
                {
                    if (_physicalTracks[Drive] == 0) status |= 4;
                    if (Motor && _rotation < _clock / 250) status |= 2;
                    if (Motor) status |= 0x20;
                    if (Image?.IsReadOnly == true) status |= C.WriteProtected;
                }
                else if (_drq) status |= C.DataRequest;
                return status;
            case 1: return _track;
            case 2: return _sector;
            default: _drq = false; return _data;
        }
    }

    internal void Write(int register, byte value)
    {
        switch (register & 3)
        {
            case 0: Command(value); break;
            case 1: if (!Active) _track = value; break;
            case 2: if (!Active) _sector = value; break;
            case 3:
                _data = value;
                if (_drq && Writing && _offset < _buffer.Length) _buffer[_offset++] = value;
                _drq = false; break;
        }
    }

    /// <summary>Avance le moteur, les délais mécaniques et la fenêtre de service des octets DRQ.</summary>
    internal void Tick()
    {
        if (_motorTicks > 0) _motorTicks--;
        _rotation = (_rotation + 1) % RevolutionTicks;
        var ready = Image is not null;
        if (_forceIndex && _rotation == 0 || _forceReady && ready && !_wasReady || _forceNotReady && !ready && _wasReady) _irq = true;
        _wasReady = ready;
        if (!Active || _delay > 0 && --_delay > 0) return;
        if (_phase == Phase.Positioning) { Positioned(); return; }
        if (_drq)
        {
            _errors |= C.LostData;
            if (Writing && _offset < _buffer.Length) _buffer[_offset++] = 0;
        }
        if (_offset == _buffer.Length) { TransferCompleted(); return; }
        if (!Writing) _data = _buffer[_offset++];
        _drq = true; _delay = ByteTicks; _motorTicks = RevolutionTicks * 10;
    }

    private int MechanicalDelay(int ticks) => MechanicalSpeedPercent == 0 ? 0 : checked((int)((long)ticks * 100 / MechanicalSpeedPercent));
    private void Complete(byte errors = 0)
    {
        _errors |= errors; _phase = Phase.Idle; _drq = false; _irq = true; _buffer = [];
        _motorTicks = Math.Max(_motorTicks, RevolutionTicks * 9);
    }
    private static bool MediaFailure(Exception error) => error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException;
}
