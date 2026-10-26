using Phase = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Enums.Intel8271Phase;
using System.IO;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Intel8271Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Transferts sectoriels temporisés : le conteneur reste derrière l'interface SDK empruntée.</summary>
internal sealed partial class Intel8271
{
    private long? _logicalSector;

    private void PrepareSector()
    {
        var disk = _disk(_drive);
        if (disk is null) { Complete(C.NotReady); return; }
        try
        {
            _logicalSector = disk.FindSector(_tracks[_drive], Side, _sector);
            if (_command == C.ReadId)
                for (var attempts = 0; _logicalSector is null && attempts < 256; attempts++) { _sector = (_sector + 1) & 255; _logicalSector = disk.FindSector(_tracks[_drive], Side, _sector); }
            if (_logicalSector is null) { Complete(C.NotFound); return; }
            var sector = disk.ReadSectorAsync(_logicalSector.Value).GetAwaiter().GetResult();
            _offset = 0; _awaitingData = false;
            if (_command == C.ReadId)
            {
                var size = 0; while ((128 << size) < sector.Data.Length && size < 7) size++;
                _buffer = [(byte)sector.Cylinder, (byte)sector.Head, (byte)sector.Number, (byte)size]; _phase = Phase.Id;
            }
            else
            {
                if (sector.Data.Length != _sectorSize) { Complete(C.NotFound); return; }
                _crcError |= sector.IntegrityValid == false;
                _phase = _command switch { C.WriteSingle or C.WriteMultiple => Phase.Write, C.VerifySingle or C.VerifyMultiple => Phase.Verify, _ => Phase.Read };
                _buffer = _phase == Phase.Write ? new byte[_sectorSize] : sector.Data.ToArray();
            }
            _delay = _phase == Phase.Verify ? _byteTicks * _sectorSize : _byteTicks;
            _motorTicks = Math.Max(_motorTicks, _delay + _unloadTicks);
        }
        catch (Exception error) when (error is IOException or InvalidDataException or ArgumentOutOfRangeException) { Complete(C.NotFound); }
    }

    private bool StoreSector()
    {
        var disk = _disk(_drive);
        if (disk is null) { Complete(C.NotReady); return false; }
        if (disk.IsReadOnly) { Complete(C.WriteProtected); return false; }
        try { disk.WriteSector(_logicalSector!.Value, _buffer); return true; }
        catch (UnauthorizedAccessException) { Complete(C.WriteProtected); return false; }
        catch (Exception error) when (error is IOException or InvalidDataException or ArgumentException) { Complete(C.NotFound); return false; }
    }

    private void NextSector()
    {
        if (--_remaining == 0) { Complete(_crcError ? C.DataCrc : (byte)0); return; }
        _sector = (_sector + 1) & 255;
        PrepareSector();
    }

    /// <summary>Les identifiants C/H/R/N sont fournis par le programme de formatage, puis les secteurs sont remplis.</summary>
    private void StoreFormattedTrack()
    {
        var disk = _disk(_drive);
        if (disk is null) { Complete(C.NotReady); return; }
        if (disk.IsReadOnly) { Complete(C.WriteProtected); return; }
        try
        {
            for (var offset = 0; offset < _buffer.Length; offset += C.FormatIdSize)
            {
                var cylinder = _buffer[offset]; var head = _buffer[offset + 1]; var number = _buffer[offset + 2]; var sizeCode = _buffer[offset + 3];
                if (cylinder != _tracks[_drive] || head != Side || sizeCode > 7) { Complete(C.NotFound); return; }
                var logical = disk.FindSector(cylinder, head, number);
                if (logical is null) { Complete(C.NotFound); return; }
                var fill = new byte[128 << sizeCode]; Array.Fill(fill, C.FormatFill);
                disk.WriteSector(logical.Value, fill);
            }
            Complete(0);
        }
        catch (UnauthorizedAccessException) { Complete(C.WriteProtected); }
        catch (Exception error) when (error is IOException or InvalidDataException or ArgumentException) { Complete(C.NotFound); }
    }
}
