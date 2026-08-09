namespace GWGUI.Scp.Decoding;

/// <summary>
/// Decodes Roland Gustafsson's Brøderbund RWTS18 track format: six physical
/// sectors of 768 bytes (three independently scattered 256-byte pages) per track.
/// </summary>
public sealed class AppleRwts18Decoder : IFluxDecoder
{
    private static readonly byte[] Nibbles =
    [
        0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,
        0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,
        0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,
        0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff
    ];
    private static readonly Dictionary<byte, byte> Inverse = Nibbles.Select((value, index) => (value, index))
        .ToDictionary(pair => pair.value, pair => (byte)pair.index);

    public string Id => "apple2.rwts18";
    public string DisplayName => "Apple II Brøderbund RWTS18";
    public FluxDecodeResult Decode(ScpRevolution revolution) => DecodeCore(FluxBitstream.FromNrziIntervals(revolution.FluxIntervals));
    internal FluxDecodeResult DecodeBits(bool[] bits) => DecodeCore(new FluxBitstream(bits, 1));

    private FluxDecodeResult DecodeCore(FluxBitstream source)
    {
        var trackBitLength = source.Bits.Length;
        var stream = source.WithCircularTail(16_384);
        var structures = new List<FluxStructure>();
        var decodedBytes = new List<byte>();
        var sectors = new List<DecodedSector>();

        for (var offset = 0; offset + 16 <= trackBitLength; offset++)
        {
            if (!stream.Match(offset, 0xd59d, 16)) continue;
            var cursor = offset + 16;
            var address = AppleBitLatch.TryReadBytes(stream.Bits, ref cursor, 4);
            if (address is null || !Inverse.TryGetValue(address[0], out var track) ||
                !Inverse.TryGetValue(address[1], out var sector) ||
                !Inverse.TryGetValue(address[2], out var checksum) || address[3] != 0xaa ||
                sector >= 6 || (byte)(track ^ sector) != checksum)
                continue;

            var data = TryReadData(stream.Bits, cursor);
            var integrity = data?.Valid;
            var payload = data?.Data;
            structures.Add(new(FluxStructureKind.AppleAddress, offset, cursor - offset,
                $"Apple II RWTS18 T{track} S{sector}, address checksum valid"));
            if (data is not null)
            {
                structures.Add(new(FluxStructureKind.AppleData, data.Value.StartOffset,
                    data.Value.EndOffset - data.Value.StartOffset,
                    $"Apple II RWTS18 data block, 768 bytes, checksum {(data.Value.Valid ? "valid" : "invalid")}"));
                decodedBytes.AddRange(data.Value.Data);
            }
            sectors.Add(new(track, 0, sector, 3, 768, integrity, offset,
                SectorIntegrityKind.Checksum, payload));
            offset = Math.Max(offset + 15, (data?.EndOffset ?? cursor) - 1);
        }

        var valid = sectors.Count(sector => sector.IntegrityValid == true);
        var confidence = sectors.Count == 0 ? 0 : Math.Min(1, valid / 6d + sectors.Count / 24d);
        return new(Id, DisplayName, confidence, source.BitCellTicks, structures, decodedBytes, sectors);
    }

    private static (byte[] Data, bool Valid, int StartOffset, int EndOffset)? TryReadData(bool[] bits, int offset)
    {
        var cursor = offset;
        var stream = AppleBitLatch.TryReadBytes(bits, ref cursor, 1_100);
        if (stream is null) return null;
        // The first byte is a modifiable Brøderbund identifier. Find it by the
        // following uninterrupted run of 1025 valid GCR symbols and D4 epilogue.
        for (var start = 0; start + 1_027 <= stream.Length; start++)
        {
            var values = new byte[1_025];
            var validSymbols = true;
            for (var index = 0; index < values.Length; index++)
            {
                if (Inverse.TryGetValue(stream[start + 1 + index], out values[index])) continue;
                validSymbols = false;
                break;
            }
            if (!validSymbols || stream[start + 1_026] != 0xd4) continue;
            var decoded = DecodePayload(values);
            var startOffset = offset + start * 8;
            var endOffset = offset + (start + 1_027) * 8;
            return (decoded.Data, decoded.Valid, startOffset, endOffset);
        }
        return null;
    }

    private static (byte[] Data, bool Valid) DecodePayload(IReadOnlyList<byte> values)
    {
        var page1 = new byte[256]; var page2 = new byte[256]; var page3 = new byte[256];
        byte accumulator = 0; byte previousPage1 = 0;
        for (var index = 0; index < 256; index++)
        {
            var high = values[index * 4];
            var checksum = (byte)(accumulator ^ previousPage1 ^ high);
            page1[index] = (byte)(((high << 2) & 0xc0) | values[index * 4 + 1]);
            previousPage1 = page1[index];
            page2[index] = (byte)(((high << 4) & 0xc0) | values[index * 4 + 2]);
            page3[index] = (byte)(((high << 6) & 0xc0) | values[index * 4 + 3]);
            accumulator = (byte)(page3[index] ^ page2[index] ^ checksum);
        }
        var valid = ((accumulator ^ values[1_024] ^ previousPage1) & 0x3f) == 0;
        return ([.. page1, .. page2, .. page3], valid);
    }

}
