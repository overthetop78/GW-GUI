namespace GWGUI.Scp.Decoding;

public sealed class AppleGcrDecoder : IFluxDecoder
{
    private static readonly byte[] SixAndTwo = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
    private static readonly Dictionary<byte, byte> InverseSixAndTwo = SixAndTwo.Select((value, index) => (value, index)).ToDictionary(x => x.value, x => (byte)x.index);
    public string Id => "apple2.gcr"; public string DisplayName => "Apple II GCR";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromNrziIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>(); var pairedData = new HashSet<int>();
        for (var offset = 0; offset + 24 <= stream.Bits.Length; offset++)
        {
            if (!stream.Match(offset, 0xD5AA96, 24)) continue;
            var address = TryReadBytes(stream.Bits, offset + 24, 8); bool? headerValid = null; byte volume = 0; byte cylinder = 0; byte number = 0;
            if (address is not null)
            {
                volume = DecodeFourAndFour(address[0], address[1]); cylinder = DecodeFourAndFour(address[2], address[3]); number = DecodeFourAndFour(address[4], address[5]);
                var checksum = DecodeFourAndFour(address[6], address[7]); headerValid = (byte)(volume ^ cylinder ^ number) == checksum; bytes.AddRange([volume, cylinder, number, checksum]);
            }
            var headerEnd = offset + 24 + (address is null ? 0 : 64); var epilogueOffset = Find(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + 512), 0xDEAAEB);
            var dataOffset = epilogueOffset < 0 ? -1 : Find(stream, epilogueOffset + 24, Math.Min(stream.Bits.Length, epilogueOffset + 24 + 512), 0xD5AAAD); bool? dataValid = null; var structureEnd = headerEnd;
            byte[]? sectorData = null;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset); var data = TryDecodeSixAndTwo(stream.Bits, dataOffset + 24);
                if (data is not null)
                {
                    dataValid = data.Value.Valid; structureEnd = data.Value.EndOffset; sectorData = data.Value.Data; bytes.AddRange(sectorData);
                    structures.Add(new(FluxStructureKind.AppleData, dataOffset, data.Value.EndOffset - dataOffset, $"Apple II data block, 256 bytes, checksum {(dataValid == true ? "valid" : "invalid")}"));
                }
                else structures.Add(new(FluxStructureKind.AppleData, dataOffset, 24, "Apple II data block, checksum unavailable"));
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, 0, number, 1, 256, integrity, offset, SectorIntegrityKind.Checksum, sectorData));
            structures.Add(new(FluxStructureKind.AppleAddress, offset, Math.Max(24, headerEnd - offset), $"Apple II V{volume} T{cylinder} S{number}, address checksum {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
            offset = headerValid == true ? Math.Max(offset + 23, structureEnd - 1) : offset + 23;
        }
        for (var offset = 0; offset + 24 <= stream.Bits.Length; offset++) if (stream.Match(offset, 0xD5AAAD, 24) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.AppleData, offset, 24, "Unpaired Apple II data prologue D5 AA AD")); offset += 23; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 32d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte DecodeFourAndFour(byte high, byte low) => (byte)(((high << 1) | 1) & low);
    private static byte[]? TryReadBytes(bool[] bits, int offset, int count)
    {
        if (offset + count * 8 > bits.Length) return null; var result = new byte[count];
        for (var index = 0; index < count; index++) for (var bit = 0; bit < 8; bit++) if (bits[offset + index * 8 + bit]) result[index] |= (byte)(1 << (7 - bit));
        return result;
    }
    private static int Find(FluxBitstream stream, int start, int end, uint mark)
    {
        for (var offset = Math.Max(0, start); offset + 24 <= end; offset++) if (stream.Match(offset, mark, 24)) return offset;
        return -1;
    }
    private static (byte[] Data, bool Valid, int EndOffset)? TryDecodeSixAndTwo(bool[] bits, int offset)
    {
        var encoded = TryReadBytes(bits, offset, 343); if (encoded is null) return null; var values = new byte[343];
        for (var index = 0; index < values.Length; index++) if (!InverseSixAndTwo.TryGetValue(encoded[index], out values[index])) return null;
        var decoded = new byte[342]; byte previous = 0; var encodedIndex = 0;
        for (var index = 341; index >= 256; index--) { decoded[index] = (byte)(values[encodedIndex++] ^ previous); previous = decoded[index]; }
        for (var index = 0; index < 256; index++) { decoded[index] = (byte)(values[encodedIndex++] ^ previous); previous = decoded[index]; }
        var valid = (byte)(values[342] ^ previous) == 0; var data = new byte[256]; byte auxiliaryOffset = 0;
        for (var index = 0; index < 256; index++)
        {
            auxiliaryOffset = (byte)((auxiliaryOffset + 85) % 86); var auxiliary = decoded[256 + auxiliaryOffset]; decoded[256 + auxiliaryOffset] = (byte)(auxiliary >> 2);
            data[index] = (byte)((decoded[index] << 2) | ((auxiliary & 2) >> 1) | ((auxiliary & 1) << 1));
        }
        return (data, valid, offset + 343 * 8);
    }
}
