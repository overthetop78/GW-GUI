using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Intel8271Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Décodage des commandes et registres de configuration du 8271.</summary>
internal sealed partial class Intel8271
{
    private void Command(byte value)
    {
        if ((_status & C.Busy) != 0) return;
        _command = (byte)(value & 0x3f); _drive = (value & C.Drive1Select) != 0 ? 1 : 0;
        if (_command < C.DriveStatus) _driveOutput = (byte)((_driveOutput & C.SideSelect) | (value & (C.Drive0Select | C.Drive1Select)));
        Array.Clear(_parameters); _parameterCount = 0; _awaitingData = false; _status = C.Busy; _crcError = false;
        _requiredParameters = _command switch
        {
            C.DriveStatus => 0,
            C.Seek or C.ReadSpecial => 1,
            C.ReadSingle or C.ReadIncludingDeletedSingle or C.WriteSingle or C.VerifySingle or C.WriteSpecial => 2,
            C.ReadMultiple or C.ReadIncludingDeletedMultiple or C.WriteMultiple or C.VerifyMultiple or C.ReadId => 3,
            C.Specify => 4,
            C.Format => 5,
            _ => -1
        };
        if (_requiredParameters < 0) Complete(C.NotFound);
        else if (_requiredParameters == 0) BeginCommand();
    }

    private void BeginCommand()
    {
        switch (_command)
        {
            case C.DriveStatus:
                var status = 0x80;
                if (_disk(0) is not null && (_driveOutput & C.Drive0Select) != 0) status |= 4;
                if (_disk(1) is not null && (_driveOutput & C.Drive1Select) != 0) status |= 0x40;
                if (_tracks[_drive] == 0) status |= 2;
                if (_disk(_drive)?.IsReadOnly == true) status |= 8;
                if (Motor && _rotation < _clock / 250) status |= 0x10;
                Complete((byte)status, false); return;
            case C.Specify:
                if (_parameters[0] == 0x0d)
                {
                    _stepTicks = _parameters[1] * _clock / 1000; _settleTicks = _parameters[2] * _clock / 1000;
                    _loadTicks = (_parameters[3] & 15) * _clock * 2 / 1000; _unloadTicks = (_parameters[3] >> 4) * _clock / 5;
                }
                _status = 0; return;
            case C.WriteSpecial:
                switch (_parameters[0])
                {
                    case C.Track0: _tracks[0] = _parameters[1]; break;
                    case C.Track1: _tracks[1] = _parameters[1]; break;
                    case C.Mode: _mode = _parameters[1]; break;
                    case C.DriveOutput: _driveOutput = _parameters[1]; break;
                    default: Complete(C.NotFound); return;
                }
                _status = 0; return;
            case C.ReadSpecial:
                var result = _parameters[0] switch { C.Track0 => _tracks[0], C.Track1 => _tracks[1], C.Mode => _mode, C.DriveOutput => _driveOutput, 6 => (byte)_sector, _ => (byte?)null };
                if (result is null) Complete(C.NotFound); else Complete(result.Value, false);
                return;
        }
        var disk = _disk(_drive);
        if (disk is null) { Complete(C.NotReady); return; }
        if (_command is C.WriteSingle or C.WriteMultiple or C.Format && disk.IsReadOnly) { Complete(C.WriteProtected); return; }
        if (_parameters[0] >= disk.Cylinders || Side >= disk.Heads) { Complete(C.NotFound); return; }
        _remaining = _command is C.ReadSingle or C.ReadIncludingDeletedSingle or C.WriteSingle or C.VerifySingle ? 1 : (_parameters[2] & 31) == 0 ? 32 : _parameters[2] & 31;
        _sectorSize = _command is C.ReadSingle or C.ReadIncludingDeletedSingle or C.WriteSingle or C.VerifySingle ? 128 : 128 << (_parameters[2] >> 5);
        _sector = _command == C.ReadId ? 0 : _parameters[1];
        _phase = Phase.Positioning;
        _delay = Math.Abs(_parameters[0] - _tracks[_drive]) * _stepTicks + _settleTicks + (Motor ? 0 : _loadTicks);
        _delay = MechanicalSpeedPercent == 0 ? 0 : checked((int)((long)_delay * 100 / MechanicalSpeedPercent));
        _motorTicks = Math.Max(_unloadTicks, _delay + _clock / 5);
    }

    private void Positioned()
    {
        _tracks[_drive] = _parameters[0];
        if (_command == C.Seek) { Complete(0); return; }
        if (_command == C.Format)
        {
            _phase = Phase.Format; _buffer = new byte[_remaining * C.FormatIdSize]; _offset = 0; _delay = _byteTicks;
            return;
        }
        PrepareSector();
    }
}
