using GWGUI.MediaEngine.Containers.Scp;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public sealed class DecRx02Decoder : SignatureMfmDecoder
{
    private static readonly byte[] HeaderMark = [0x55,0x11,0x15,0x54];
    private static readonly (byte[] Pattern, byte Mark)[] DataMarks = [([0x55,0x11,0x14,0x44], 0xf8), ([0x55,0x11,0x14,0x45], 0xf9), ([0x55,0x11,0x14,0x54], 0xfa), ([0x55,0x11,0x14,0x55], 0xfb), ([0x55,0x11,0x15,0x44], 0xfc), ([0x55,0x11,0x15,0x45], 0xfd)];
    public override string Id => FluxCodecIds.DecRx02; public override string DisplayName => FluxCodecDisplayNames.DecRx02;
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(HeaderMark, FluxStructureKind.FormatHeader, "DEC RX02 sector header"), .. DataMarks.Select(item => (item.Pattern, FluxStructureKind.FormatData, $"DEC RX02 {item.Mark:X2} data"))];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedData = new HashSet<int>();
        const int markBits = 4 * BitPrimitives.BitsPerByte;
        const int headerBits = 7 * 32;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "DEC RX02 sector header")); offset += markBits - 1; continue;
            }
            var header = TryDecodeFmBytes(stream, offset + 32, 6);
            if (header is null) continue;
            var cylinder = header[0]; var head = header[1]; var number = header[2]; var sizeCode = header[3];
            var crcHigh = header[4]; var crcLow = header[5];
            if (Primitives.Crc16Calculator.Compute([0xfe, cylinder, head, number, sizeCode, crcHigh, crcLow]) != 0)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"DEC RX02 C{cylinder} H{head} R{number}, header CRC invalid")); offset += markBits - 1; continue;
            }
            bytes.AddRange([cylinder, head, number, sizeCode]);

            var data = FindNextDataMark(stream, offset + headerBits, (88 + 16) * BitPrimitives.BitsPerByte * 2);
            var m2fm = data.Mark is 0xf9 or 0xfd; var sectorSize = m2fm ? 256 : 128; var decodedCount = sectorSize + 2;
            var completeData = data.Offset >= 0 && (m2fm ? data.Offset + markBits + 1 + decodedCount * 16 : data.Offset + (1 + sectorSize + 2) * 32) <= stream.Bits.Length;
            bool? dataCrcValid = null; byte[]? payload = null;
            if (completeData)
            {
                ushort crc = Primitives.Crc16Calculator.Update(Primitives.Crc16Calculator.AllBitsSetInitialValue, data.Mark);
                if (m2fm)
                {
                    var decoded = DecodeM2Fm(stream, data.Offset + markBits + 1, decodedCount); foreach (var value in decoded) crc = Primitives.Crc16Calculator.Update(crc, value); payload = decoded.Take(sectorSize).ToArray();
                }
                else
                {
                    var decoded = TryDecodeFmBytes(stream, data.Offset + 32, sectorSize + 2);
                    if (decoded is null) continue;
                    payload = decoded.AsSpan(0, sectorSize).ToArray();
                    foreach (var value in decoded) crc = Primitives.Crc16Calculator.Update(crc, value);
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
            foreach (var item in DataMarks) if (FluxBitReader.MatchBytes(stream, offset, item.Pattern)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, $"DEC RX02 {item.Mark:X2} data")); offset += markBits - 1; break; }
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static (int Offset, byte Mark) FindNextDataMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - HeaderMark.Length * BitPrimitives.BitsPerByte, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) foreach (var item in DataMarks) if (FluxBitReader.MatchBytes(stream, offset, item.Pattern)) return (offset, item.Mark);
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
            for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++) if (!bits[1 + index * 16 + bit * 2] && bits[1 + index * 16 + bit * 2 + 1]) value |= (byte)(1 << (BitPrimitives.BitsPerByte - 1 - bit));
            result[index] = value;
        }
        return result;
    }

    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * 32, out result[index])) return null;
        return result;
    }
}
