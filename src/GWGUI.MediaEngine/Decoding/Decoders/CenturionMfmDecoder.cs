using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

public sealed class CenturionMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = [0x91, 0x22, 0x44, 0x89];
    private static readonly byte[] DataMark = [0xaa, 0xaa, 0xaa, 0xa9];
    public override string Id => "centurion.mfm"; public override string DisplayName => "Centurion MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "Centurion sector mark"), (DataMark, FluxStructureKind.FormatData, "Centurion data mark")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveClock(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int markBits = 4 * 8;
        const int headerBits = markBits + 4 * 16;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = Enumerable.Range(0, 4).Select(index => FluxBitReader.DecodeMfmByte(stream, offset + markBits + index * 16)).ToArray();
                var headerValid = Crc16(header) == 0; bytes.AddRange(header);
                var dataOffset = FindDataMark(stream, offset + headerBits + 400); bool? dataValid = null; var size = 0; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var prefixEnd = dataOffset + markBits + 3 * 16;
                    if (prefixEnd <= stream.Bits.Length)
                    {
                        var key = FluxBitReader.DecodeMfmByte(stream, dataOffset + markBits); size = (FluxBitReader.DecodeMfmByte(stream, dataOffset + markBits + 16) << 8) | FluxBitReader.DecodeMfmByte(stream, dataOffset + markBits + 32);
                        var dataEnd = (long)prefixEnd + (size + 2L) * 16;
                        if (key == 0 && size > 0 && dataEnd <= stream.Bits.Length)
                        {
                            var block = Enumerable.Range(0, size + 4).Select(index => FluxBitReader.DecodeMfmByte(stream, dataOffset + markBits + 16 + index * 16)).ToArray();
                            dataValid = Crc16(block) == 0; bytes.AddRange(block.Skip(2).Take(size)); structureEnd = (int)dataEnd;
                            structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, $"Centurion data, {size} bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                        }
                        else structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits + 3 * 16, key == 0 ? "Centurion data, CRC unavailable" : $"Centurion data with unsupported key {key}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits, "Centurion data, CRC unavailable"));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(header[0], 0, header[1], SizeCode(size), size, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"Centurion C{header[0]} R{header[1]}, {size} bytes, header CRC {(headerValid ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
                offset = Math.Max(offset + markBits - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "Centurion sector mark"));
            if (!complete) offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, "Unpaired Centurion data block")); offset += markBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindDataMark(FluxBitstream stream, int start)
    {
        for (var offset = Math.Max(0, start); offset + DataMark.Length * 8 <= stream.Bits.Length; offset++) { if (FluxBitReader.MatchBytes(stream, offset, SectorMark)) return -1; if (FluxBitReader.MatchBytes(stream, offset, DataMark)) return offset; }
        return -1;
    }

    private static byte SizeCode(int size)
    {
        for (byte code = 0; code < 8; code++) if ((128 << code) == size) return code;
        return 0;
    }

    private static ushort Crc16(IEnumerable<byte> values) => Primitives.Crc16Calculator.Compute(values, initial: 0);
}
