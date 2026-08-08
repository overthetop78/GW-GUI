namespace GWGUI.Scp.Decoding;

public sealed class AppleMacGcrDecoder : IFluxDecoder
{
    private static readonly byte[] SixAndTwo = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
    private static readonly Dictionary<byte, byte> Inverse = SixAndTwo.Select((value, index) => (value, index)).ToDictionary(item => item.value, item => (byte)item.index);
    private static readonly byte[] AddressMark = [0xd5, 0xaa, 0x96];
    private static readonly byte[] DataMark = [0xd5, 0xaa, 0xad];
    public string Id => "applemac.gcr"; public string DisplayName => "Apple Macintosh GCR";

    public FluxDecodeResult Decode(ScpRevolution revolution) => DecodeCore(revolution, FluxBitstream.FromNrziIntervals(revolution.FluxIntervals));

    internal FluxDecodeResult DecodeBits(bool[] bits) => DecodeCore(
        new ScpRevolution((uint)bits.Length, 0, []), new FluxBitstream(bits, 1));

    public FluxDecodeResult DecodeAtBitCell(ScpRevolution revolution, double bitCellTicks) =>
        DecodeCore(revolution, FluxBitstream.FromNrziIntervals(revolution.FluxIntervals, bitCellTicks));

    private FluxDecodeResult DecodeCore(ScpRevolution revolution, FluxBitstream stream)
    {
        var trackBitLength = stream.Bits.Length;
        stream = stream.WithCircularTail(8192);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int markBits = 24; const int headerSymbols = 5; const int dataSymbols = 704;
        for (var offset = 0; offset < trackBitLength && offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, AddressMark)) continue;
            var header = TryReadSymbols(stream, offset + markBits, headerSymbols); bool? headerValid = null; byte cylinder = 0, head = 0, number = 0;
            if (header is not null && header.All(Inverse.ContainsKey))
            {
                var values = header.Select(value => Inverse[value]).ToArray();
                cylinder = (byte)(((values[2] & 3) << 6) | (values[0] & 0x3f)); head = (byte)((values[2] >> 5) & 1); number = values[1];
                headerValid = (byte)((values[0] ^ values[1] ^ values[2] ^ values[3]) & 0x3f) == values[4];
            }
            var headerEnd = offset + markBits + (header is null ? 0 : headerSymbols * 8);
            var dataOffset = headerValid == true ? FindMark(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + 512), DataMark) : -1;
            bool? dataValid = null; byte[]? sectorData = null; byte[]? sectorTag = null; var structureEnd = headerEnd;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset); var encoded = TryReadSymbols(stream, dataOffset + markBits, dataSymbols);
                if (encoded is not null && encoded.All(Inverse.ContainsKey))
                {
                    var values = encoded.Select(value => Inverse[value]).ToArray(); var decoded = DecodeSixAndTwo(values.AsSpan(1, 699), out var checksum);
                    dataValid = checksum[3] == values[700] && checksum[2] == values[701] && checksum[1] == values[702] && checksum[0] == values[703];
                    sectorTag = decoded.Take(12).ToArray(); sectorData = decoded.Skip(12).Take(512).ToArray(); bytes.AddRange(sectorData); structureEnd = dataOffset + markBits + dataSymbols * 8;
                    structures.Add(new(FluxStructureKind.AppleData, dataOffset, structureEnd - dataOffset, $"Apple Macintosh data block, 512 bytes, checksum {(dataValid == true ? "valid" : "invalid")}"));
                }
                else structures.Add(new(FluxStructureKind.AppleData, dataOffset, markBits, "Apple Macintosh data block, checksum unavailable"));
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, head, number, 2, 512, integrity, offset, SectorIntegrityKind.Checksum, sectorData, sectorTag));
            structures.Add(new(FluxStructureKind.AppleAddress, offset, Math.Max(markBits, headerEnd - offset), $"Apple Macintosh C{cylinder} H{head} S{number}, address checksum {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
            offset = headerValid == true ? Math.Max(offset + markBits - 1, structureEnd - 1) : offset + markBits - 1;
        }
        for (var offset = 0; offset < trackBitLength && offset + markBits <= stream.Bits.Length; offset++) if (stream.MatchBytes(offset, DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.AppleData, offset, markBits, "Unpaired Apple Macintosh data prologue")); offset += markBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static byte[] DecodeSixAndTwo(ReadOnlySpan<byte> symbols, out byte[] checksum)
    {
        var b1 = new byte[175]; var b2 = new byte[175]; var b3 = new byte[175]; var source = 0;
        for (var index = 0; index <= 174; index++)
        {
            var w4 = symbols[source++]; var w1 = symbols[source++]; var w2 = symbols[source++]; var w3 = index == 174 ? (byte)0 : symbols[source++];
            b1[index] = (byte)((w1 & 0x3f) | ((w4 << 2) & 0xc0)); b2[index] = (byte)((w2 & 0x3f) | ((w4 << 4) & 0xc0)); b3[index] = (byte)((w3 & 0x3f) | ((w4 << 6) & 0xc0));
        }
        var output = new byte[524]; uint c1 = 0, c2 = 0, c3 = 0; var destination = 0;
        for (var index = 0; ; index++)
        {
            c1 = (c1 & 0xff) << 1; if ((c1 & 0x100) != 0) c1++;
            var value = (byte)(b1[index] ^ c1); c3 += value; if ((c1 & 0x100) != 0) { c3++; c1 &= 0xff; } output[destination++] = value;
            value = (byte)(b2[index] ^ c3); c2 += value; if (c3 > 0xff) { c2++; c3 &= 0xff; } output[destination++] = value;
            if (destination == 524) break;
            value = (byte)(b3[index] ^ c2); c1 += value; if (c2 > 0xff) { c1++; c2 &= 0xff; } output[destination++] = value;
        }
        checksum = [(byte)(c1 & 0x3f), (byte)(c2 & 0x3f), (byte)(c3 & 0x3f), (byte)(((c1 & 0xc0) >> 6) | ((c2 & 0xc0) >> 4) | ((c3 & 0xc0) >> 2))];
        return output;
    }

    private static byte[]? TryReadSymbols(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * 8 > stream.Bits.Length) return null;
        return Enumerable.Range(0, count).Select(index => stream.DecodeByte(offset + index * 8)).ToArray();
    }
    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = start; offset + mark.Count * 8 <= end; offset++) if (stream.MatchBytes(offset, mark)) return offset;
        return -1;
    }
}
