using Phase = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Enums.Wd1770Phase;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Wd1770Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Transfert des secteurs et représentation des marques, identifiants et CRC d'une piste régulière.</summary>
internal sealed partial class Wd1770
{
    private void TransferCompleted()
    {
        _drq = false;
        if (_phase == Phase.WriteTrack) { StoreTrack(); return; }
        if (_phase == Phase.WriteSector)
        {
            try
            {
                var image = Image;
                if (image?.FindSector(_physicalTracks[Drive], Side, _sector) is not { } index) { Complete(C.NotFound); return; }
                image.WriteSector(index, _buffer);
            }
            catch (Exception error) when (MediaFailure(error)) { Complete(Image?.IsReadOnly == true ? C.WriteProtected : C.NotFound); return; }
        }
        if (_crcError) { Complete(C.CrcError); return; }
        if (_phase is Phase.ReadSector or Phase.WriteSector && (_command & 0x10) != 0)
        {
            var write = _phase == Phase.WriteSector; _sector++; BeginSector(write); return;
        }
        Complete();
    }

    private void BeginTrack(bool write)
    {
        if (write)
        {
            _buffer = new byte[(DoubleDensity ? C.DoubleDensityBytesPerSecond : C.SingleDensityBytesPerSecond) / C.RevolutionsPerSecond];
            _phase = Phase.WriteTrack; _offset = 0; _delay = ByteTicks; return;
        }
        var bytes = new List<byte>(); var image = Image!;
        bytes.AddRange(Enumerable.Repeat((byte)(DoubleDensity ? 0x4e : 0xff), 40));
        try
        {
            foreach (var sector in TrackSectors(image))
            {
                var code = SizeCode(sector.Data.Length);
                if (code < 0) { Complete(C.NotFound); return; }
                AddRecord(bytes, 0xfe, [(byte)sector.Cylinder, (byte)sector.Head, (byte)sector.Number, (byte)code]);
                bytes.AddRange(Enumerable.Repeat((byte)(DoubleDensity ? 0x4e : 0xff), DoubleDensity ? 22 : 11));
                AddRecord(bytes, 0xfb, sector.Data.ToArray(), sector.IntegrityValid == false);
                bytes.AddRange(Enumerable.Repeat((byte)(DoubleDensity ? 0x4e : 0xff), DoubleDensity ? 24 : 10));
            }
        }
        catch (Exception error) when (MediaFailure(error)) { Complete(C.NotFound); return; }
        var count = (DoubleDensity ? C.DoubleDensityBytesPerSecond : C.SingleDensityBytesPerSecond) / C.RevolutionsPerSecond;
        if (bytes.Count > count) { Complete(C.NotFound); return; }
        bytes.AddRange(Enumerable.Repeat((byte)(DoubleDensity ? 0x4e : 0xff), count - bytes.Count));
        _buffer = bytes.ToArray(); _phase = Phase.ReadTrack; _offset = 0; _delay = ByteTicks;
    }

    private IEnumerable<EmulationDiskSector> TrackSectors(IEmulationDiskImage image)
    {
        for (var number = 0; number <= image.SectorsPerTrack; number++)
            if (image.FindSector(_physicalTracks[Drive], Side, number) is { } index)
                yield return image.ReadSectorAsync(index).GetAwaiter().GetResult();
    }

    private void AddRecord(List<byte> bytes, byte mark, byte[] data, bool badCrc = false)
    {
        bytes.AddRange(Enumerable.Repeat((byte)0, DoubleDensity ? 12 : 6));
        var start = bytes.Count;
        if (DoubleDensity) bytes.AddRange([0xa1, 0xa1, 0xa1]);
        bytes.Add(mark); bytes.AddRange(data);
        var crc = Crc(bytes.Skip(start));
        if (badCrc) crc ^= 1;
        bytes.Add((byte)(crc >> 8)); bytes.Add((byte)crc);
    }

    /// <summary>Applique les blocs de données dont les identifiants existent dans le conteneur sectoriel.</summary>
    private void StoreTrack()
    {
        var image = Image;
        if (image is null) { Complete(C.NotFound); return; }
        if (image.IsReadOnly) { Complete(C.WriteProtected); return; }
        var pending = new List<(long Index, byte[] Data)>();
        int cylinder = -1, head = -1, number = -1, size = 0;
        for (var offset = 0; offset < _buffer.Length; offset++)
        {
            if (_buffer[offset] == 0xfe && HasSync(offset) && offset + 4 < _buffer.Length)
            {
                cylinder = _buffer[++offset]; head = _buffer[++offset]; number = _buffer[++offset];
                var code = _buffer[++offset]; size = code <= C.MaximumSectorSizeCode ? C.SectorSizeBase << code : 0;
            }
            else if (_buffer[offset] is 0xfb or 0xf8 && HasSync(offset) && size > 0)
            {
                if (_buffer[offset] == 0xf8 || cylinder != _physicalTracks[Drive] || head != Side || offset + size >= _buffer.Length || image.FindSector(cylinder, head, number) is not { } index) { Complete(C.NotFound); return; }
                var data = _buffer.AsSpan(offset + 1, size).ToArray(); offset += size;
                pending.Add((index, data)); size = 0;
            }
        }
        if (pending.Count == 0) { Complete(C.NotFound); return; }
        try { foreach (var sector in pending) image.WriteSector(sector.Index, sector.Data); }
        catch (Exception error) when (MediaFailure(error)) { Complete(Image?.IsReadOnly == true ? C.WriteProtected : C.NotFound); return; }
        Complete();
    }

    private bool HasSync(int offset) => offset >= 3 && (DoubleDensity
        ? _buffer[offset - 1] is 0xa1 or 0xf5 && _buffer[offset - 2] is 0xa1 or 0xf5 && _buffer[offset - 3] is 0xa1 or 0xf5
        : _buffer[offset - 1] == 0 && _buffer[offset - 2] == 0 && _buffer[offset - 3] == 0);

    private static ushort Crc(IEnumerable<byte> bytes)
    {
        ushort value = 0xffff;
        foreach (var item in bytes)
        {
            value ^= (ushort)(item << 8);
            for (var bit = 0; bit < 8; bit++) value = (ushort)((value & 0x8000) != 0 ? value << 1 ^ 0x1021 : value << 1);
        }
        return value;
    }
}
