using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

public sealed class TycomFmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] HeaderMark = [0x55,0x11,0x15,0x54];
    private static readonly (byte[] Pattern, byte Mark)[] DataMarks = [([0x55,0x11,0x14,0x44], 0xf8), ([0x55,0x11,0x14,0x45], 0xf9), ([0x55,0x11,0x14,0x54], 0xfa), ([0x55,0x11,0x14,0x55], 0xfb)];
    public override string Id => "tycom.fm"; public override string DisplayName => "TYCOM FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(HeaderMark, FluxStructureKind.FormatHeader, "TYCOM sector header"), .. DataMarks.Select(item => (item.Pattern, FluxStructureKind.FormatData, $"TYCOM {item.Mark:X2} data"))];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedData = new HashSet<int>();
        const int markBits = 4 * 8;
        const int headerBits = 5 * 32;
        const int sectorSize = 128;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "TYCOM sector header")); offset += markBits - 1; continue;
            }
            var cylinder = stream.DecodeFmByte32(offset + 32); var number = stream.DecodeFmByte32(offset + 64);
            var crcHigh = stream.DecodeFmByte32(offset + 96); var crcLow = stream.DecodeFmByte32(offset + 128);
            if (Crc16([0xfe, cylinder, (byte)number, crcHigh, crcLow]) != 0)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"TYCOM C{cylinder} R{number}, header CRC invalid")); offset += markBits - 1; continue;
            }
            bytes.AddRange([cylinder, (byte)number]);

            var data = FindNextDataMark(stream, offset + headerBits, (88 + 16) * 8 * 2);
            var completeData = data.Offset >= 0 && data.Offset + (1 + sectorSize + 2) * 32 <= stream.Bits.Length;
            bool? dataCrcValid = null;
            if (completeData)
            {
                ushort crc = 0xffff; var payload = new byte[sectorSize];
                for (var index = 0; index < 1 + sectorSize + 2; index++) { var value = stream.DecodeFmByte32(data.Offset + index * 32); crc = UpdateCrc(crc, value); if (index is > 0 and <= sectorSize) payload[index - 1] = value; }
                dataCrcValid = crc == 0; classifiedData.Add(data.Offset); bytes.AddRange(payload);
                structures.Add(new(FluxStructureKind.FormatData, data.Offset, (1 + sectorSize + 2) * 32, $"TYCOM {data.Mark:X2} C{cylinder} R{number} data, CRC {(dataCrcValid == true ? "valid" : "invalid")}"));
            }
            sectors.Add(new(cylinder, 0, number, 0, sectorSize, dataCrcValid, offset));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"TYCOM C{cylinder} R{number}, 128 bytes, header CRC valid{(completeData ? $", {data.Mark:X2} data CRC {(dataCrcValid == true ? "valid" : "invalid")}" : ", data CRC unavailable")}"));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (classifiedData.Contains(offset)) continue;
            foreach (var item in DataMarks) if (stream.MatchBytes(offset, item.Pattern)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, $"TYCOM {item.Mark:X2} data")); offset += markBits - 1; break; }
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static (int Offset, byte Mark) FindNextDataMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - HeaderMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) foreach (var item in DataMarks) if (stream.MatchBytes(offset, item.Pattern)) return (offset, item.Mark);
        return (-1, 0);
    }

    private static ushort Crc16(IEnumerable<byte> values)
        => Primitives.Crc16Calculator.Compute(values);

    private static ushort UpdateCrc(ushort crc, byte value)
        => Primitives.Crc16Calculator.Update(crc, value);
}
