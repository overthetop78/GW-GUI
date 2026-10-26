using Phase = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Enums.AcornScsiPhase;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.AcornScsiConstants;
using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Accès aux blocs de 256 octets du bus, en conservant les autres portions des secteurs du conteneur.</summary>
internal sealed partial class AcornScsi
{
    private long VirtualSectorCount => Image is { } image ? image.SectorCount * image.SectorSize / C.SectorSize : 0;
    private bool RangeAvailable() => Image is { SectorSize: >= C.SectorSize } image && image.SectorSize % C.SectorSize == 0 && _lba >= 0 && _lba <= VirtualSectorCount && _remaining <= VirtualSectorCount - _lba;

    private void PrepareRead()
    {
        try
        {
            var image = Image!; var byteOffset = _lba * C.SectorSize;
            var sector = image.ReadSectorAsync(byteOffset / image.SectorSize).GetAwaiter().GetResult();
            if (sector.IntegrityValid == false) { Finish(C.ReadError); return; }
            _buffer = sector.Data.Slice((int)(byteOffset % image.SectorSize), C.SectorSize).ToArray();
            TrackHeadPosition(); _readBlocks[_unit]++;
            _offset = 0; _phase = Phase.Read; _requestDelay = 2;
        }
        catch (Exception error) when (MediaFailure(error)) { Finish(C.ReadError); }
    }

    private void StoreBlock(ReadOnlySpan<byte> data)
    {
        var image = Image!; var byteOffset = _lba * C.SectorSize; var index = byteOffset / image.SectorSize;
        if (image.SectorSize == C.SectorSize) image.WriteSector(index, data);
        else
        {
            var original = image.ReadSectorAsync(index).GetAwaiter().GetResult().Data.ToArray();
            data.CopyTo(original.AsSpan((int)(byteOffset % image.SectorSize), C.SectorSize));
            image.WriteSector(index, original);
        }
        TrackHeadPosition(); _writtenBlocks[_unit]++;
    }

    private void TrackHeadPosition()
    {
        var image = Image!;
        var position = image.Heads > 0 && image.SectorsPerTrack > 0 && image.SectorSize > 0 ? _lba * C.SectorSize / image.SectorSize / image.Heads / image.SectorsPerTrack : _lba;
        if (image.Heads > 0 && image.SectorsPerTrack > 0)
        {
            var sector = _lba * C.SectorSize / image.SectorSize;
            image.Mechanics.Seek(checked((int)position), (int)(sector / image.SectorsPerTrack % image.Heads), (int)(sector % image.SectorsPerTrack));
        }
        else image.Mechanics.Seek(0, 0, logicalBlock: _lba);
        if (_headPositions[_unit] != position) { _seekCount[_unit]++; _headPositions[_unit] = position; }
    }

    private void WriteBuffer()
    {
        if (_command[0] == 0x15)
        {
            var cylinders = _buffer[13] << 8 | _buffer[14]; var heads = _buffer[15];
            if (cylinders == 0 || heads == 0 || (long)cylinders * heads * C.SectorsPerTrack != VirtualSectorCount) { Finish(C.InvalidParameter); return; }
            try { Image!.UpdateMetadata(new Dictionary<string, string> { [EmulationDiskMetadataKeys.AcornScsiParameters] = Convert.ToBase64String(_buffer) }); Finish(); }
            catch (Exception error) when (MediaFailure(error)) { Finish(Image?.IsReadOnly == true ? C.WriteProtected : C.InvalidParameter); }
            return;
        }
        try
        {
            if (Image is null) { Finish(C.NotReady); return; }
            StoreBlock(_buffer);
            if (--_remaining == 0) Finish(); else { _lba++; _offset = 0; _requestDelay = 2; }
        }
        catch (Exception error) when (MediaFailure(error)) { Finish(Image?.IsReadOnly == true ? C.WriteProtected : C.WriteError); }
    }

    private void FormatNextBlock()
    {
        try
        {
            if (Image is null) { Finish(C.NotReady); return; }
            if (_lba >= VirtualSectorCount) { Finish(); return; }
            StoreBlock(new byte[C.SectorSize]); _lba++;
            _delay = _speeds[_unit] == 0 ? 0 : Math.Max(1, _clock / 20_000 * 100 / _speeds[_unit]);
            _activityTicks = _clock / 20;
        }
        catch (Exception error) when (MediaFailure(error)) { Finish(Image?.IsReadOnly == true ? C.WriteProtected : C.WriteError); }
    }
}
