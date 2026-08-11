using System.Buffers.Binary;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>
/// Resolves FileCore new-map indirect disc addresses to physical image offsets.
/// The implementation follows the FileCore allocation-map layout used by ADFS.
/// </summary>
internal sealed class AcornFileCoreNewMap
{
    private const int DiscRecordOffset = 4;
    private const int DiscRecordLength = 60;
    private const int DiscRecordBits = DiscRecordLength * BitPrimitives.BitsPerByte;
    private const int RootFragment = 2;

    private readonly SectorImage _image;
    private readonly Zone[] _zones;
    private readonly int _idLength;
    private readonly int _mapToBlockShift;
    private readonly int _log2ShareSize;
    private readonly int _idsPerZone;

    private AcornFileCoreNewMap(SectorImage image, DiscRecord record, Zone[] zones)
    {
        _image = image;
        Record = record;
        _zones = zones;
        _idLength = record.IdLength;
        _mapToBlockShift = record.Log2BytesPerMapBit - record.Log2SectorSize;
        _log2ShareSize = record.Log2ShareSize;
        _idsPerZone = record.ZoneSizeBits / (_idLength + 1);
    }

    internal DiscRecord Record { get; }

    internal static bool TryCreate(SectorImage image, out AcornFileCoreNewMap? map)
    {
        map = null;
        if (!image.TryGetBlock(0, out var firstBlock) || firstBlock.Data.Count < DiscRecordOffset + DiscRecordLength)
            return false;

        var bytes = firstBlock.Data.ToArray();
        var recordBytes = bytes.AsSpan(DiscRecordOffset, DiscRecordLength);
        if (!DiscRecord.TryParse(recordBytes, image.Capacity, out var record)) return false;

        var mapAddress = Shift((record.ZoneCount >> 1) * record.ZoneSizeBits -
            (record.ZoneCount > 1 ? DiscRecordBits : 0),
            record.Log2BytesPerMapBit - record.Log2SectorSize);
        if (mapAddress < 0 || mapAddress + record.ZoneCount > image.BlockCount) return false;

        var zones = new Zone[record.ZoneCount];
        var describedBits = checked((int)(image.Capacity >> record.Log2BytesPerMapBit));
        for (var index = 0; index < zones.Length; index++)
        {
            if (!image.TryGetBlock(mapAddress + index, out var block) || block.Data.Count != image.BlockSize)
                return false;
            var startBit = index == 0 ? 32 + DiscRecordBits : 32;
            var startMapBit = index == 0 ? 0 : index * record.ZoneSizeBits - DiscRecordBits;
            var endBit = 32 + record.ZoneSizeBits;
            if (index == zones.Length - 1)
                endBit = 32 + describedBits - ((record.ZoneCount - 1) * record.ZoneSizeBits - DiscRecordBits);
            if (startBit >= endBit || endBit > block.Data.Count * BitPrimitives.BitsPerByte) return false;
            zones[index] = new(block.Data.ToArray(), startMapBit, startBit, endBit);
        }

        map = new(image, record, zones);
        return true;
    }

    internal bool TryResolveByteOffset(int indirectAddress, long objectByteOffset, out long physicalByteOffset)
    {
        physicalByteOffset = 0;
        if (indirectAddress <= 0 || objectByteOffset < 0) return false;
        var logicalBlock = checked((int)(objectByteOffset / _image.BlockSize));
        var offsetInBlock = checked((int)(objectByteOffset % _image.BlockSize));
        if (!TryResolveBlock(indirectAddress, logicalBlock, out var physicalBlock)) return false;
        physicalByteOffset = (long)physicalBlock * _image.BlockSize + offsetInBlock;
        return physicalByteOffset >= 0 && physicalByteOffset < _image.Capacity;
    }

    internal long ReadFreeBytes()
    {
        long freeMapBits = 0;
        var fragmentIdLength = Math.Min(_idLength, 15);
        var idMask = (1u << fragmentIdLength) - 1;
        foreach (var zone in _zones)
        {
            var link = checked((int)GetBits(zone.Data, 8, idMask));
            if (link == 0) continue;
            var start = 8;
            while (true)
            {
                start += link;
                if (start < zone.StartBit || start >= zone.EndBit) break;
                var fragment = GetBits(zone.Data, start, idMask);
                var fragmentEnd = FindNextSetBit(zone.Data, start + _idLength, zone.EndBit);
                if (fragmentEnd >= zone.EndBit) break;
                freeMapBits += fragmentEnd + 1L - start;
                if (fragment < _idLength + 1) break;
                link = checked((int)fragment);
            }
        }
        return Math.Min(_image.Capacity, freeMapBits << Record.Log2BytesPerMapBit);
    }

    private bool TryResolveBlock(int indirectAddress, int objectBlock, out int physicalBlock)
    {
        physicalBlock = 0;
        var block = objectBlock;
        if ((indirectAddress & 0xFF) != 0)
            block += ((indirectAddress & 0xFF) - 1) << _log2ShareSize;
        var fragmentId = (uint)indirectAddress >> BitPrimitives.BitsPerByte;
        var zoneIndex = fragmentId == RootFragment ? _zones.Length >> 1 : checked((int)(fragmentId / _idsPerZone));
        if (zoneIndex < 0 || zoneIndex >= _zones.Length) return false;

        var mapOffset = Shift(block, -_mapToBlockShift);
        if (mapOffset < 0) return false;
        var remainingOffset = mapOffset;
        for (var scanned = 0; scanned < _zones.Length; scanned++)
        {
            var zone = _zones[(zoneIndex + scanned) % _zones.Length];
            if (!TryLookupZone(zone, fragmentId, ref remainingOffset, out var foundBit)) continue;
            var mapPosition = foundBit - zone.StartBit + zone.StartMapBit;
            var sectorOffset = block - Shift(mapOffset, _mapToBlockShift);
            var resolved = sectorOffset + Shift(mapPosition, _mapToBlockShift);
            if (resolved <= 0 || resolved >= _image.BlockCount) return false;
            physicalBlock = resolved;
            return true;
        }
        return false;
    }

    private bool TryLookupZone(Zone zone, uint fragmentId, ref int offset, out int foundBit)
    {
        foundBit = 0;
        var idMask = (1u << _idLength) - 1;
        var freeMask = idMask & 0x7FFF;
        var freeLinkValue = GetBits(zone.Data, 8, freeMask);
        var freeLink = freeLinkValue == 0 ? 0 : checked(8 + (int)freeLinkValue);
        var start = zone.StartBit;
        while (start < zone.EndBit)
        {
            var fragment = GetBits(zone.Data, start, idMask);
            var fragmentEnd = FindNextSetBit(zone.Data, start + _idLength, zone.EndBit);
            if (fragmentEnd >= zone.EndBit) return false;
            if (start == freeLink)
            {
                freeLink += checked((int)(fragment & 0x7FFF));
            }
            else if (fragment == fragmentId)
            {
                var length = fragmentEnd + 1 - start;
                if (offset < length)
                {
                    foundBit = start + offset;
                    return true;
                }
                offset -= length;
            }
            start = fragmentEnd + 1;
        }
        return false;
    }

    private static uint GetBits(byte[] data, int bitOffset, uint mask)
    {
        var byteOffset = bitOffset >> 3;
        Span<byte> value = stackalloc byte[4];
        data.AsSpan(byteOffset, Math.Min(4, data.Length - byteOffset)).CopyTo(value);
        return (BinaryPrimitives.ReadUInt32LittleEndian(value) >> (bitOffset & 7)) & mask;
    }

    private static int FindNextSetBit(byte[] data, int start, int end)
    {
        for (var bit = start; bit < end; bit++)
            if ((data[bit >> 3] & (1 << (bit & 7))) != 0) return bit;
        return end;
    }

    private static int Shift(int value, int shift) => shift >= 0 ? checked(value << shift) : value >> -shift;

    private sealed record Zone(byte[] Data, int StartMapBit, int StartBit, int EndBit);

    internal sealed record DiscRecord(int Log2SectorSize, int IdLength, int Log2BytesPerMapBit,
        int ZoneCount, int ZoneSpareBits, int RootAddress, long DiscSize, string DiscName, int Log2ShareSize)
    {
        internal int ZoneSizeBits => (8 << Log2SectorSize) - ZoneSpareBits;

        internal static bool TryParse(ReadOnlySpan<byte> data, long imageCapacity, out DiscRecord record)
        {
            record = null!;
            if (data.Length < DiscRecordLength) return false;
            var log2Sector = data[0];
            var idLength = data[4];
            var log2BytesPerMapBit = data[5];
            var zones = data[9] | data[42] << BitPrimitives.BitsPerByte;
            var zoneSpare = BinaryPrimitives.ReadUInt16LittleEndian(data[10..12]);
            var root = BinaryPrimitives.ReadInt32LittleEndian(data[12..16]);
            var discSize = (long)BinaryPrimitives.ReadUInt32LittleEndian(data[16..20]) |
                (long)BinaryPrimitives.ReadUInt32LittleEndian(data[36..40]) << 32;
            if (log2Sector is < 8 or > 10 || idLength < log2Sector + 3 || idLength > 19 ||
                log2BytesPerMapBit > log2Sector || zones <= 0 || root <= 0 || discSize <= 0 ||
                discSize > imageCapacity || zoneSpare >= 8 << log2Sector)
                return false;
            var name = System.Text.Encoding.ASCII.GetString(data[22..32]).TrimEnd('\0', '\r', ' ');
            record = new(log2Sector, idLength, log2BytesPerMapBit, zones, zoneSpare, root, discSize, name, data[40] & 0x0F);
            return true;
        }
    }
}
