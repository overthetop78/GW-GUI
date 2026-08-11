using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Decodes the zoned 512-byte GCR sectors used by the Commodore 900.</summary>
public sealed class Commodore900GcrDecoder : IFluxDecoder
{
    private static readonly Dictionary<int, int> Gcr = new()
    {
        [0x0a]=0,[0x0b]=1,[0x12]=2,[0x13]=3,[0x0e]=4,[0x0f]=5,[0x16]=6,[0x17]=7,
        [0x09]=8,[0x19]=9,[0x1a]=10,[0x1b]=11,[0x0d]=12,[0x1d]=13,[0x1e]=14,[0x15]=15
    };

    public string Id => "commodore900.gcr";
    public string DisplayName => "Commodore 900 GCR";

    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var decodedBytes = new List<byte>();
        var headers = new List<(int Offset, int End, byte[] Bytes)>();
        var dataBlocks = new List<(int Offset, int End, byte[] Bytes)>();

        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            if (!stream.Bits[offset]) continue;
            var end = offset;
            while (end < stream.Bits.Length && stream.Bits[end]) end++;
            if (end - offset < 10) { offset = end; continue; }
            structures.Add(new(FluxStructureKind.CommodoreSync, offset, end - offset, "Commodore 900 GCR sync"));
            if (TryDecodeBytes(stream.Bits, end, 4) is { } header && header[0] == 0x08)
            {
                headers.Add((offset, end + 40, header)); decodedBytes.AddRange(header);
            }
            else if (TryDecodeBytes(stream.Bits, end, 514) is { } data && data[0] == 0x07)
            {
                dataBlocks.Add((offset, end + 5140, data)); decodedBytes.AddRange(data);
            }
            offset = end;
        }

        var sectors = new List<DecodedSector>();
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index];
            var cylinder = header.Bytes[1]; var number = header.Bytes[2];
            var headerValid = (byte)(header.Bytes[0] ^ cylinder ^ number ^ header.Bytes[3]) == 0;
            var next = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            var data = dataBlocks.FirstOrDefault(candidate => candidate.Offset > header.End && candidate.Offset < next);
            var dataValid = data.Bytes is not null && data.Bytes.Aggregate((byte)0, (checksum, value) => (byte)(checksum ^ value)) == 0;
            var payload = data.Bytes?.Skip(1).Take(512).ToArray();
            var valid = !headerValid || data.Bytes is null || !dataValid ? false : true;
            sectors.Add(new(cylinder, 0, number, 2, 512, valid, header.Offset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.CommodoreHeader, header.Offset, Math.Max(10, header.End - header.Offset),
                $"Commodore 900 C{cylinder} S{number}"));
            if (data.Bytes is not null)
                structures.Add(new(FluxStructureKind.FormatData, data.Offset, Math.Max(10, data.End - data.Offset),
                    $"Commodore 900 data, checksum {(dataValid ? "valid" : "invalid")}"));
        }
        var validCount = sectors.Count(sector => sector.IntegrityValid == true);
        return new(Id, DisplayName, Math.Min(1, validCount / 13d), stream.BitCellTicks, structures, decodedBytes, sectors);
    }

    private static byte[]? TryDecodeBytes(bool[] bits, int offset, int count)
    {
        if (offset + count * 10 > bits.Length) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            if (!TryNibble(bits, offset + index * 10, out var high) ||
                !TryNibble(bits, offset + index * 10 + 5, out var low)) return null;
            result[index] = (byte)((high << 4) | low);
        }
        return result;
    }

    private static bool TryNibble(bool[] bits, int offset, out int value)
    {
        var code = 0; value = 0;
        for (var bit = 0; bit < 5; bit++) code = (code << 1) | (bits[offset + bit] ? 1 : 0);
        return Gcr.TryGetValue(code, out value);
    }
}
