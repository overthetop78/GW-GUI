using System.Buffers.Binary;
using GWGUI.Scp.Decoding;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

/// <summary>Reads the raw bit-cell tracks stored by the 86Box 86F format.</summary>
public sealed class I86fImageReader(FluxDecoderRegistry decoders)
{
    private const uint Magic = 0x46423638;

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length < 8 || BinaryPrimitives.ReadUInt32LittleEndian(data) != Magic)
            throw new InvalidDataException("The file does not contain an 86F signature.");

        var flags = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(6));
        var sides = (flags & 0x0008) != 0 ? 2 : 1;
        var offsets = sides == 2 ? 512 : 256;
        var tableEnd = checked(8 + offsets * 4);
        if (data.Length < tableEnd) throw new InvalidDataException("The 86F track table is incomplete.");

        var candidates = new Dictionary<SectorAddress, List<DecodedSector>>();
        for (var logicalTrack = 0; logicalTrack < offsets; logicalTrack++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var offset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(8 + logicalTrack * 4)));
            if (offset == 0) continue;
            var nextOffset = NextOffset(data, logicalTrack + 1, offsets, data.Length);
            var revolution = ReadTrack(data, offset, nextOffset, flags);
            if (revolution is null) continue;
            var sideFlags = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset));
            var decoder = (sideFlags & 0x18) == 0x08 ? "iso.mfm" : "iso.fm";
            var decoded = decoders.Decode(decoder, revolution);
            foreach (var sector in decoded.Sectors ?? [])
            {
                if (sector.Data is null || sector.Number < 0) continue;
                var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                if (!candidates.TryGetValue(address, out var values)) candidates[address] = values = [];
                values.Add(sector);
            }
        }

        if (candidates.Count == 0) throw new InvalidDataException("No FM or MFM sector could be decoded from the 86F image.");
        return BuildSectorImage(candidates);
    }

    private static ScpRevolution? ReadTrack(byte[] data, int offset, int nextOffset, ushort flags)
    {
        var hasExtraBitCells = (flags & 0x0080) != 0;
        var headerSize = hasExtraBitCells ? 10 : 6;
        if (offset < 0 || offset > data.Length - headerSize || nextOffset < offset + headerSize)
            throw new InvalidDataException("An 86F track points outside the image.");

        var bitCount = hasExtraBitCells && (flags & 0x0060) == 0 && (flags & 0x1000) != 0
            ? BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset + 2))
            : checked((nextOffset - offset - headerSize) * 8);
        if (bitCount <= 0) throw new InvalidDataException("An 86F track has an invalid bit-cell count.");
        var byteCount = checked(((bitCount + 15) / 16) * 2);
        if (offset + headerSize > data.Length - byteCount)
            throw new InvalidDataException("An 86F track is incomplete.");

        var track = data.AsSpan(offset + headerSize, byteCount);
        var reverseBytes = (flags & 0x0800) != 0;
        var intervals = new List<uint>(bitCount / 2);
        var cells = 0u;
        for (var bit = 0; bit < bitCount; bit++)
        {
            var wordByte = (bit >> 4) * 2;
            var byteInWord = (bit >> 3) & 1;
            if (reverseBytes) byteInWord ^= 1;
            var set = (track[wordByte + byteInWord] & (0x80 >> (bit & 7))) != 0;
            cells++;
            if (!set) continue;
            intervals.Add(cells * 40);
            cells = 0;
        }
        if (intervals.Count == 0) return null;
        return new((uint)(bitCount * 40), (uint)intervals.Count, intervals);
    }

    private static int NextOffset(byte[] data, int start, int count, int fallback)
    {
        for (var index = start; index < count; index++)
        {
            var value = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(8 + index * 4)));
            if (value != 0) return value;
        }
        return fallback;
    }

    private static SectorImage BuildSectorImage(Dictionary<SectorAddress, List<DecodedSector>> candidates)
    {
        var sectorSize = candidates.Values.SelectMany(value => value).GroupBy(value => value.Data!.Count)
            .OrderByDescending(group => group.Count()).First().Key;
        var cylinders = candidates.Keys.Max(address => address.Cylinder) + 1;
        var heads = candidates.Keys.Max(address => address.Head) + 1;
        var sectorNumbers = candidates.Keys.Select(address => address.Number).Distinct().OrderBy(value => value).ToArray();
        var sectorsPerTrack = candidates.Keys.GroupBy(address => (address.Cylinder, address.Head))
            .Select(group => group.Select(address => address.Number).Distinct().Count())
            .GroupBy(value => value).OrderByDescending(group => group.Count()).ThenByDescending(group => group.Key).First().Key;
        var zeroBased = sectorNumbers.Length > 0 && sectorNumbers[0] == 0;
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            if (address.Cylinder >= cylinders || address.Head >= heads) continue;
            var sectorIndex = zeroBased ? Array.IndexOf(sectorNumbers, address.Number) : address.Number - 1;
            if (sectorIndex < 0 || sectorIndex >= sectorsPerTrack) continue;
            var matchingSize = values.Where(value => value.Data?.Count == sectorSize).ToArray();
            if (matchingSize.Length == 0) continue;
            var best = matchingSize.OrderByDescending(value => value.IntegrityValid == true)
                .ThenByDescending(value => value.IntegrityValid is null).First();
            var logical = (address.Cylinder * heads + address.Head) * sectorsPerTrack + sectorIndex;
            blocks.Add(new(logical, address, best.Data!.ToArray(), best.IntegrityValid));
        }
        var format = sectorSize == 512
            ? IbmPcImageReader.FormatIdForGeometry(cylinders, heads, sectorsPerTrack, sectorSize)
            : $"86f.{sectorSize}.{cylinders}.{heads}.{sectorsPerTrack}";
        return new(format, sectorSize, cylinders, heads, sectorsPerTrack, blocks,
            capacity: (long)cylinders * heads * sectorsPerTrack * sectorSize);
    }
}
