namespace GWGUI.Scp.Decoding;

public sealed class QdMo5MfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] HeaderMark = [0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x44,0x91];
    private static readonly byte[] DataMark = [0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x91,0x44];
    public override string Id => "qdmo5.mfm"; public override string DisplayName => "QD MO5 MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(HeaderMark, FluxStructureKind.FormatHeader, "QD MO5 sector header"), (DataMark, FluxStructureKind.FormatData, "QD MO5 sector data")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedDataMarks = new HashSet<int>();
        const int markBits = 12 * 8;
        const int headerBits = 10 * 8 + 16 * 16;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "QD MO5 sector header"));
                offset += markBits - 1; continue;
            }

            var high = stream.DecodeMfmByte(offset + markBits); var low = stream.DecodeMfmByte(offset + markBits + 16);
            var number = (high << 8) | low; bytes.Add(high); bytes.Add(low);
            var dataOffset = FindNextData(stream, offset + headerBits, (88 + 16) * 8);
            var completeData = dataOffset >= 0 && dataOffset + 10 * 8 + 130 * 16 <= stream.Bits.Length;
            bool? checksumValid = null;
            if (completeData)
            {
                byte checksum = 0; var data = new byte[128];
                for (var index = 0; index < 129; index++) { var value = stream.DecodeMfmByte(dataOffset + 10 * 8 + index * 16); checksum += value; if (index > 0) data[index - 1] = value; }
                var stored = stream.DecodeMfmByte(dataOffset + 10 * 8 + 129 * 16); checksumValid = checksum == stored;
                pairedDataMarks.Add(dataOffset); bytes.AddRange(data);
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, 10 * 8 + 130 * 16, $"QD MO5 R{number} data, checksum {(checksumValid == true ? "valid" : "invalid")}"));
            }
            sectors.Add(new(0, 0, number, 0, 128, checksumValid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"QD MO5 R{number}, 128 bytes{(completeData ? $", data checksum {(checksumValid == true ? "valid" : "invalid")}" : ", data checksum unavailable")}"));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!pairedDataMarks.Contains(offset) && stream.MatchBytes(offset, DataMark)) structures.Add(new(FluxStructureKind.FormatData, offset, markBits, "QD MO5 sector data"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static int FindNextData(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - DataMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++)
        {
            if (stream.MatchBytes(offset, DataMark)) return offset;
            if (stream.MatchBytes(offset, HeaderMark)) return -1;
        }
        return -1;
    }
}
