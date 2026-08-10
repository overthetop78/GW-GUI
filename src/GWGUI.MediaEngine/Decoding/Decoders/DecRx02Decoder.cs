using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

public sealed class DecRx02Decoder : SignatureMfmDecoder
{
    private static readonly byte[] HeaderMark = [0x55,0x11,0x15,0x54];
    private static readonly (byte[] Pattern, byte Mark)[] DataMarks = [([0x55,0x11,0x14,0x44], 0xf8), ([0x55,0x11,0x14,0x45], 0xf9), ([0x55,0x11,0x14,0x54], 0xfa), ([0x55,0x11,0x14,0x55], 0xfb), ([0x55,0x11,0x15,0x44], 0xfc), ([0x55,0x11,0x15,0x45], 0xfd)];
    public override string Id => "dec.rx02"; public override string DisplayName => "DEC RX02 M²FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(HeaderMark, FluxStructureKind.FormatHeader, "DEC RX02 sector header"), .. DataMarks.Select(item => (item.Pattern, FluxStructureKind.FormatData, $"DEC RX02 {item.Mark:X2} data"))];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedData = new HashSet<int>();
        const int markBits = 4 * 8;
        const int headerBits = 7 * 32;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "DEC RX02 sector header")); offset += markBits - 1; continue;
            }
            var cylinder = stream.DecodeFmByte32(offset + 32); var head = stream.DecodeFmByte32(offset + 64); var number = stream.DecodeFmByte32(offset + 96); var sizeCode = stream.DecodeFmByte32(offset + 128);
            var crcHigh = stream.DecodeFmByte32(offset + 160); var crcLow = stream.DecodeFmByte32(offset + 192);
            if (Crc16([0xfe, cylinder, head, number, sizeCode, crcHigh, crcLow]) != 0)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"DEC RX02 C{cylinder} H{head} R{number}, header CRC invalid")); offset += markBits - 1; continue;
            }
            bytes.AddRange([cylinder, head, number, sizeCode]);

            var data = FindNextDataMark(stream, offset + headerBits, (88 + 16) * 8 * 2);
            var m2fm = data.Mark is 0xf9 or 0xfd; var sectorSize = m2fm ? 256 : 128; var decodedCount = sectorSize + 2;
            var completeData = data.Offset >= 0 && (m2fm ? data.Offset + markBits + 1 + decodedCount * 16 : data.Offset + (1 + sectorSize + 2) * 32) <= stream.Bits.Length;
            bool? dataCrcValid = null; byte[]? payload = null;
            if (completeData)
            {
                ushort crc = UpdateCrc(0xffff, data.Mark);
                if (m2fm)
                {
                    var decoded = DecodeM2Fm(stream, data.Offset + markBits + 1, decodedCount); foreach (var value in decoded) crc = UpdateCrc(crc, value); payload = decoded.Take(sectorSize).ToArray();
                }
                else
                {
                    payload = new byte[sectorSize];
                    for (var index = 1; index < 1 + sectorSize + 2; index++) { var value = stream.DecodeFmByte32(data.Offset + index * 32); crc = UpdateCrc(crc, value); if (index <= sectorSize) payload[index - 1] = value; }
                }
                dataCrcValid = crc == 0; classifiedData.Add(data.Offset); bytes.AddRange(payload);
                structures.Add(new(FluxStructureKind.FormatData, data.Offset, m2fm ? markBits + 1 + decodedCount * 16 : (1 + sectorSize + 2) * 32, $"DEC RX02 {data.Mark:X2} C{cylinder} H{head} R{number} {(m2fm ? "M²FM" : "FM")} data, CRC {(dataCrcValid == true ? "valid" : "invalid")}"));
            }
            sectors.Add(new(cylinder, head, number, sizeCode, sectorSize, dataCrcValid, offset, Data: payload));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"DEC RX02 C{cylinder} H{head} R{number}, {sectorSize} bytes, header CRC valid{(completeData ? $", {data.Mark:X2} data CRC {(dataCrcValid == true ? "valid" : "invalid")}" : ", data CRC unavailable")}"));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (classifiedData.Contains(offset)) continue;
            foreach (var item in DataMarks) if (stream.MatchBytes(offset, item.Pattern)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, $"DEC RX02 {item.Mark:X2} data")); offset += markBits - 1; break; }
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static (int Offset, byte Mark) FindNextDataMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - HeaderMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) foreach (var item in DataMarks) if (stream.MatchBytes(offset, item.Pattern)) return (offset, item.Mark);
        return (-1, 0);
    }

    private static byte[] DecodeM2Fm(FluxBitstream stream, int start, int count)
    {
        var bits = new bool[count * 16 + 1];
        for (var index = 0; index < count * 16 && start + index < stream.Bits.Length; index++) bits[index + 1] = stream.Bits[start + index];
        bool[] encodedRule = [false, true, false, false, false, true, false, false, false, true, false];
        bool[] normalRule = [false, false, true, false, true, false, true, false, true, false, false];
        for (var offset = 0; offset + encodedRule.Length <= bits.Length; offset++)
        {
            var matches = true; for (var index = 0; index < encodedRule.Length; index++) if (bits[offset + index] != encodedRule[index]) { matches = false; break; }
            if (offset % 2 != 0 || !matches) continue;
            for (var index = 0; index < normalRule.Length; index++) bits[offset + index] = normalRule[index];
            offset += encodedRule.Length - 2;
        }
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            byte value = 0;
            for (var bit = 0; bit < 8; bit++) if (!bits[1 + index * 16 + bit * 2] && bits[1 + index * 16 + bit * 2 + 1]) value |= (byte)(1 << (7 - bit));
            result[index] = value;
        }
        return result;
    }

    private static ushort Crc16(IEnumerable<byte> values)
        => Primitives.Crc16Calculator.Compute(values);

    private static ushort UpdateCrc(ushort crc, byte value)
        => Primitives.Crc16Calculator.Update(crc, value);
}
