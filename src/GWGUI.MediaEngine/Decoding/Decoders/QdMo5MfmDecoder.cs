using GWGUI.MediaEngine.Containers.Scp;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public sealed class QdMo5MfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] HeaderMark = [0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x44,0x91];
    private static readonly byte[] DataMark = [0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x91,0x44];
    public override string Id => "qdmo5.mfm"; public override string DisplayName => "QD MO5 MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(HeaderMark, FluxStructureKind.FormatHeader, "QD MO5 sector header"), (DataMark, FluxStructureKind.FormatData, "QD MO5 sector data")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedDataMarks = new HashSet<int>();
        const int markBits = 12 * BitPrimitives.BitsPerByte;
        const int headerBits = 10 * BitPrimitives.BitsPerByte + 16 * 16;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "QD MO5 sector header"));
                offset += markBits - 1; continue;
            }

            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + markBits, out var high)) continue;
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + markBits + 16, out var low)) continue;
            var number = (high << BitPrimitives.BitsPerByte) | low; bytes.Add(high); bytes.Add(low);
            var dataOffset = FindNextData(stream, offset + headerBits, (88 + 16) * BitPrimitives.BitsPerByte);
            var completeData = dataOffset >= 0 && dataOffset + 10 * BitPrimitives.BitsPerByte + 130 * 16 <= stream.Bits.Length;
            bool? checksumValid = null;
            if (completeData)
            {
                var block = TryDecodeMfmBytes(stream, dataOffset + 10 * BitPrimitives.BitsPerByte, 130);
                if (block is null) continue;
                byte checksum = 0; var data = new byte[128];
                for (var index = 0; index < 129; index++) { var value = block[index]; checksum += value; if (index > 0) data[index - 1] = value; }
                var stored = block[129]; checksumValid = checksum == stored;
                pairedDataMarks.Add(dataOffset); bytes.AddRange(data);
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, 10 * BitPrimitives.BitsPerByte + 130 * 16, $"QD MO5 R{number} data, checksum {(checksumValid == true ? "valid" : "invalid")}"));
            }
            sectors.Add(new(0, 0, number, 0, 128, checksumValid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"QD MO5 R{number}, 128 bytes{(completeData ? $", data checksum {(checksumValid == true ? "valid" : "invalid")}" : ", data checksum unavailable")}"));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!pairedDataMarks.Contains(offset) && FluxBitReader.MatchBytes(stream, offset, DataMark)) structures.Add(new(FluxStructureKind.FormatData, offset, markBits, "QD MO5 sector data"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }

    private static int FindNextData(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - DataMark.Length * BitPrimitives.BitsPerByte, start + maximumDistance);
        for (var offset = start; offset <= end; offset++)
        {
            if (FluxBitReader.MatchBytes(stream, offset, DataMark)) return offset;
            if (FluxBitReader.MatchBytes(stream, offset, HeaderMark)) return -1;
        }
        return -1;
    }
}
