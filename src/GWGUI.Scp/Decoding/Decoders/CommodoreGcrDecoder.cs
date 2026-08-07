namespace GWGUI.Scp.Decoding;

public sealed class CommodoreGcrDecoder : IFluxDecoder
{
    private static readonly Dictionary<int, int> Gcr = new() { [0b01010]=0,[0b01011]=1,[0b10010]=2,[0b10011]=3,[0b01110]=4,[0b01111]=5,[0b10110]=6,[0b10111]=7,[0b01001]=8,[0b11001]=9,[0b11010]=10,[0b11011]=11,[0b01101]=12,[0b11101]=13,[0b11110]=14,[0b10101]=15 };
    public string Id => "commodore.gcr"; public string DisplayName => "Commodore GCR";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromNrziIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>();
        var headers = new List<(int SyncOffset, int DataOffset, int EndOffset, byte[]? Bytes)>(); var dataBlocks = new List<(int SyncOffset, int DataOffset, int EndOffset, byte[]? Bytes)>();
        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            if (!stream.Bits[offset]) continue; var end = offset; while (end < stream.Bits.Length && stream.Bits[end]) end++;
            var length = end - offset;
            if (length >= 10)
            {
                structures.Add(new(FluxStructureKind.CommodoreSync, offset, length, "Commodore GCR sync"));
                if (TryDecodeByte(stream.Bits, end, out var value))
                {
                    if (value == 0x08) { var decoded = TryDecodeBytes(stream.Bits, end, 6); headers.Add((offset, end, decoded is null ? end + 10 : end + 60, decoded)); if (decoded is not null) bytes.AddRange(decoded); else bytes.Add(value); }
                    else if (value == 0x07) { var decoded = TryDecodeBytes(stream.Bits, end, 258); dataBlocks.Add((offset, end, decoded is null ? end + 10 : end + 2580, decoded)); if (decoded is not null) bytes.AddRange(decoded); else bytes.Add(value); }
                    else bytes.Add(value);
                }
            }
            offset = end;
        }
        foreach (var block in dataBlocks)
        {
            bool? valid = null;
            if (block.Bytes is not null) { byte checksum = 0; for (var index = 1; index < 258; index++) checksum ^= block.Bytes[index]; valid = checksum == 0; }
            structures.Add(new(FluxStructureKind.FormatData, block.SyncOffset, Math.Max(10, block.EndOffset - block.SyncOffset), $"Commodore data block, 256 bytes, checksum {(valid is null ? "unavailable" : valid == true ? "valid" : "invalid")}"));
        }
        for (var headerIndex = 0; headerIndex < headers.Count; headerIndex++)
        {
            var block = headers[headerIndex];
            bool? headerValid = null; byte cylinder = 0; byte number = 0;
            if (block.Bytes is not null)
            {
                cylinder = block.Bytes[3]; number = block.Bytes[2]; headerValid = block.Bytes[0] == 0x08 && (byte)(block.Bytes[1] ^ block.Bytes[2] ^ block.Bytes[3] ^ block.Bytes[4] ^ block.Bytes[5]) == 0;
            }
            var nextHeaderOffset = headerIndex + 1 < headers.Count ? headers[headerIndex + 1].SyncOffset : int.MaxValue;
            var data = dataBlocks.FirstOrDefault(candidate => candidate.SyncOffset > block.EndOffset && candidate.SyncOffset < nextHeaderOffset); bool? dataValid = null;
            if (data.Bytes is not null) { byte checksum = 0; for (var index = 1; index < 258; index++) checksum ^= data.Bytes[index]; dataValid = checksum == 0; }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            var payload = data.Bytes is null ? null : data.Bytes.Skip(1).Take(256).ToArray();
            sectors.Add(new(cylinder, 0, number, 1, 256, integrity, block.SyncOffset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.CommodoreHeader, block.SyncOffset, Math.Max(10, block.EndOffset - block.SyncOffset), $"Commodore T{cylinder} S{number}, header checksum {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 42d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte[]? TryDecodeBytes(bool[] bits, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!TryDecodeByte(bits, offset + index * 10, out result[index])) return null;
        return result;
    }

    private static bool TryDecodeByte(bool[] bits, int offset, out byte value)
    {
        value = 0; if (offset + 10 > bits.Length) return false;
        var high = 0; var low = 0;
        for (var bit = 0; bit < 5; bit++) { high = (high << 1) | (bits[offset + bit] ? 1 : 0); low = (low << 1) | (bits[offset + 5 + bit] ? 1 : 0); }
        if (!Gcr.TryGetValue(high, out var highNibble) || !Gcr.TryGetValue(low, out var lowNibble)) return false;
        value = (byte)((highNibble << 4) | lowNibble); return true;
    }
}
