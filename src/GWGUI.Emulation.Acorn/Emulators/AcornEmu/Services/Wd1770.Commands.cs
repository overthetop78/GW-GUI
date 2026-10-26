using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Wd1770Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Commandes de déplacement, secteurs, identifiants, pistes et interruption forcée.</summary>
internal sealed partial class Wd1770
{
    private void Command(byte value)
    {
        if ((value & 0xf0) == 0xd0)
        {
            _phase = Phase.Idle; _buffer = []; _drq = false; _irq = (value & 8) != 0;
            _forceIndex = (value & 4) != 0; _forceNotReady = (value & 2) != 0; _forceReady = (value & 1) != 0; _wasReady = Image is not null;
            _typeOne = true; return;
        }
        if (Active) return;
        _command = value; _errors = 0; _irq = _drq = _crcError = _forceIndex = _forceReady = _forceNotReady = false;
        _typeOne = value < 0x80; _targetTrack = _physicalTracks[Drive];
        var delay = Motor || (value & 8) != 0 ? 0 : RevolutionTicks * 6;
        if (_typeOne)
        {
            switch (value & 0xe0)
            {
                case 0:
                    if ((value & 0x10) == 0) _targetTrack = 0;
                    else _targetTrack = Math.Clamp(_physicalTracks[Drive] + _data - _track, 0, 255);
                    break;
                case 0x40: _direction = 1; _targetTrack = Math.Min(255, _targetTrack + 1); break;
                case 0x60: _direction = -1; _targetTrack = Math.Max(0, _targetTrack - 1); break;
                default: _targetTrack = Math.Clamp(_targetTrack + _direction, 0, 255); break;
            }
            delay += Math.Abs(_targetTrack - _physicalTracks[Drive]) * C.StepMilliseconds[value & 3] * _clock / 1000;
        }
        else if ((value & 4) != 0) delay += _clock * 15 / 1000;
        _phase = Phase.Positioning; _delay = MechanicalDelay(delay); _motorTicks = _delay + RevolutionTicks * 10;
    }

    private void Positioned()
    {
        if (_typeOne)
        {
            var previous = _physicalTracks[Drive]; _physicalTracks[Drive] = _targetTrack;
            if ((_command & 0xf0) == 0) _track = 0;
            else if ((_command & 0xf0) == 0x10) _track = _data;
            else if ((_command & 0x10) != 0) _track = (byte)Math.Clamp(_track + _targetTrack - previous, 0, 255);
            if ((_command & 4) != 0 && (Image is not { } disk || _track != _physicalTracks[Drive] || _track >= disk.Cylinders)) Complete(C.NotFound);
            else Complete();
            return;
        }
        if (Image is not { } image || _physicalTracks[Drive] >= image.Cylinders || Side >= image.Heads) { Complete(C.NotFound); return; }
        var isWrite = (_command & 0xe0) == 0xa0 || (_command & 0xf0) == 0xf0;
        if (isWrite && image.IsReadOnly) { Complete(C.WriteProtected); return; }
        if (DoubleDensity == (image.SectorsPerTrack == 10 && image.SectorSize == 256)) { Complete(C.NotFound); return; }
        switch (_command & 0xe0)
        {
            case 0x80: BeginSector(false); break;
            case 0xa0: BeginSector(true); break;
            case 0xc0: BeginAddress(); break;
            case 0xe0: BeginTrack((_command & 0x10) != 0); break;
            default: Complete(C.NotFound); break;
        }
    }

    private void BeginSector(bool write)
    {
        var image = Image;
        var index = image?.FindSector(_physicalTracks[Drive], Side, _sector);
        if (image is null || index is null || _physicalTracks[Drive] != _track) { Complete(C.NotFound); return; }
        try
        {
            var sector = image.ReadSectorAsync(index.Value).GetAwaiter().GetResult();
            _buffer = write ? new byte[sector.Data.Length] : sector.Data.ToArray();
            _crcError = sector.IntegrityValid == false;
            _phase = write ? Phase.WriteSector : Phase.ReadSector; _offset = 0; _delay = ByteTicks;
        }
        catch (Exception error) when (MediaFailure(error)) { Complete(C.NotFound); }
    }

    private void BeginAddress()
    {
        var image = Image!;
        for (var candidate = 0; candidate < image.SectorsPerTrack + 1; candidate++)
        {
            var number = _idIndex++ % (image.SectorsPerTrack + 1);
            if (image.FindSector(_physicalTracks[Drive], Side, number) is not { } index) continue;
            try
            {
                var sector = image.ReadSectorAsync(index).GetAwaiter().GetResult();
                var code = SizeCode(sector.Data.Length);
                if (code < 0) { Complete(C.NotFound); return; }
                var id = new byte[] { (byte)_physicalTracks[Drive], (byte)Side, (byte)number, (byte)code };
                var crc = Crc(DoubleDensity ? new byte[] { 0xa1, 0xa1, 0xa1, 0xfe }.Concat(id) : new byte[] { 0xfe }.Concat(id));
                _buffer = [.. id, (byte)(crc >> 8), (byte)crc]; _sector = (byte)_physicalTracks[Drive];
                _phase = Phase.ReadAddress; _offset = 0; _delay = ByteTicks; return;
            }
            catch (Exception error) when (MediaFailure(error)) { Complete(C.NotFound); return; }
        }
        Complete(C.NotFound);
    }

    private static int SizeCode(int size)
    {
        for (var code = 0; code <= C.MaximumSectorSizeCode; code++) if (size == C.SectorSizeBase << code) return code;
        return -1;
    }
}
