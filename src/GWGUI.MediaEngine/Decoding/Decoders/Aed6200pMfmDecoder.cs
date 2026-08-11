using GWGUI.MediaEngine.Containers.Scp;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public sealed class Aed6200pMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorHeader = [0x50, 0x94];
    private static readonly byte[][] SectorData = [[0x50, 0x8a], [0x50, 0x89], [0x50, 0x84], [0x50, 0x85]];
    public override string Id => "aed6200p.mfm"; public override string DisplayName => "AED 6200P MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorHeader, FluxStructureKind.FormatHeader, "AED 6200P C6 header mark"), .. SectorData.Select(mark => (mark, FluxStructureKind.FormatData, "AED 6200P data mark"))];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int headerBits = 7 * 16; var pairedData = new HashSet<int>();
        for (var offset = 0; offset + SectorHeader.Length * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorHeader)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = TryDecodeMfmBytes(stream, offset, 7);
                if (header is null) continue;
                var size = (header[4] << BitPrimitives.BitsPerByte) | header[2]; var headerValid = header[0] == 0xc6 && Primitives.Crc16Calculator.Compute(header) == 0; bytes.AddRange(header);
                var dataOffset = FindDataMark(stream, offset + 1, Math.Min(stream.Bits.Length, offset + 104 * BitPrimitives.BitsPerByte));
                bool? dataValid = null; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var dataBlockBytes = 1 + size + 2; var dataEnd = (long)dataOffset + dataBlockBytes * 16L;
                    if (size > 0 && dataEnd <= stream.Bits.Length)
                    {
                        var data = TryDecodeMfmBytes(stream, dataOffset, dataBlockBytes);
                        if (data is null) continue;
                        dataValid = data[0] is >= 0xc0 and <= 0xc3 && Primitives.Crc16Calculator.Compute(data) == 0; bytes.AddRange(data.Skip(1).Take(size)); structureEnd = (int)dataEnd;
                        structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, $"AED 6200P data {data[0]:X2}, {size} bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, 16, "AED 6200P data block, CRC unavailable"));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(header[1], 0, header[3], SizeCode(size), size, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"AED 6200P C{header[1]} R{header[3]}, {size} bytes, header CRC {(headerValid ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
                offset = Math.Max(offset + SectorHeader.Length * BitPrimitives.BitsPerByte - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, SectorHeader.Length * BitPrimitives.BitsPerByte, "AED 6200P C6 header mark"));
            if (!complete) offset += SectorHeader.Length * BitPrimitives.BitsPerByte - 1;
        }
        for (var offset = 0; offset + 16 <= stream.Bits.Length; offset++) if (SectorData.Any(mark => FluxBitReader.MatchBytes(stream, offset, mark)) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, 16, "Unpaired AED 6200P data block")); offset += 15; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindDataMark(FluxBitstream stream, int start, int end)
    {
        for (var offset = Math.Max(0, start); offset + 16 <= end; offset++) if (SectorData.Any(mark => FluxBitReader.MatchBytes(stream, offset, mark))) return offset;
        return -1;
    }

    private static byte SizeCode(int size)
    {
        for (byte code = 0; code < 8; code++) if ((128 << code) == size) return code;
        return 0;
    }

    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
