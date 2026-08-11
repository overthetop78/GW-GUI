using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public sealed class DecRx02Decoder : SignatureMfmDecoder
{
    private static readonly byte[] HeaderMark = DecRx02EncodingFormat.HeaderMark.ToArray();
    private static readonly (byte[] Pattern, byte Mark)[] DataMarks = DecRx02EncodingFormat.DataMarks.Select(item => (item.Pattern.ToArray(), item.Mark)).ToArray();
    public override string Id => FluxCodecIds.DecRx02; public override string DisplayName => FluxCodecDisplayNames.DecRx02;
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(HeaderMark, FluxStructureKind.FormatHeader, "DEC RX02 sector header"), .. DataMarks.Select(item => (item.Pattern, FluxStructureKind.FormatData, $"DEC RX02 {item.Mark:X2} data"))];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedData = new HashSet<int>();
        const int markBits = DecRx02EncodingFormat.MarkBitCount;
        const int headerBits = DecRx02EncodingFormat.HeaderBitCount;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "DEC RX02 sector header")); offset += markBits - 1; continue;
            }
            var header = TryDecodeFmBytes(stream, offset + DecRx02EncodingFormat.MarkBitCount, DecRx02EncodingFormat.HeaderDecodedByteCount);
            if (header is null) continue;
            var cylinder = header[0]; var head = header[1]; var number = header[2]; var sizeCode = header[3];
            var crcHigh = header[4]; var crcLow = header[5];
            if (Crc16Calculator.Compute([DecRx02EncodingFormat.HeaderAddressMark, cylinder, head, number, sizeCode, crcHigh, crcLow], DecRx02EncodingFormat.CrcPolynomial, DecRx02EncodingFormat.CrcInitialValue) != 0)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"DEC RX02 C{cylinder} H{head} R{number}, header CRC invalid")); offset += markBits - 1; continue;
            }
            bytes.AddRange([cylinder, head, number, sizeCode]);

            var data = FindNextDataMark(stream, offset + headerBits, DecRx02EncodingFormat.DataSearchByteCount * BitPrimitives.BitsPerByte * 2);
            var m2fm = data.Mark is DecRx02EncodingFormat.M2FmDataMark or DecRx02EncodingFormat.M2FmDeletedDataMark; var sectorSize = m2fm ? DecRx02Geometry.PhysicalSectorSize : DecRx02EncodingFormat.FmSectorByteCount; var decodedCount = sectorSize + DecRx02EncodingFormat.CrcByteCount;
            var completeData = data.Offset >= 0 && (m2fm ? data.Offset + markBits + DecRx02EncodingFormat.M2FmPhaseBitCount + decodedCount * DecRx02EncodingFormat.EncodedMfmByteBitCount : data.Offset + (DecRx02EncodingFormat.DataMarkByteCount + sectorSize + DecRx02EncodingFormat.CrcByteCount) * DecRx02EncodingFormat.EncodedFmByteBitCount) <= stream.Bits.Length;
            bool? dataCrcValid = null; byte[]? payload = null;
            if (completeData)
            {
                ushort crc = Crc16Calculator.Update(DecRx02EncodingFormat.CrcInitialValue, data.Mark, DecRx02EncodingFormat.CrcPolynomial);
                if (m2fm)
                {
                    var decoded = DecodeM2Fm(stream, data.Offset + markBits + DecRx02EncodingFormat.M2FmPhaseBitCount, decodedCount); foreach (var value in decoded) crc = Crc16Calculator.Update(crc, value, DecRx02EncodingFormat.CrcPolynomial); payload = decoded.Take(sectorSize).ToArray();
                }
                else
                {
                    var decoded = TryDecodeFmBytes(stream, data.Offset + DecRx02EncodingFormat.MarkBitCount, sectorSize + DecRx02EncodingFormat.CrcByteCount);
                    if (decoded is null) continue;
                    payload = decoded.AsSpan(0, sectorSize).ToArray();
                    foreach (var value in decoded) crc = Crc16Calculator.Update(crc, value, DecRx02EncodingFormat.CrcPolynomial);
                }
                dataCrcValid = crc == 0; classifiedData.Add(data.Offset); bytes.AddRange(payload);
                structures.Add(new(FluxStructureKind.FormatData, data.Offset, m2fm ? markBits + DecRx02EncodingFormat.M2FmPhaseBitCount + decodedCount * DecRx02EncodingFormat.EncodedMfmByteBitCount : (DecRx02EncodingFormat.DataMarkByteCount + sectorSize + DecRx02EncodingFormat.CrcByteCount) * DecRx02EncodingFormat.EncodedFmByteBitCount, $"DEC RX02 {data.Mark:X2} C{cylinder} H{head} R{number} {(m2fm ? "M²FM" : "FM")} data, CRC {(dataCrcValid == true ? "valid" : "invalid")}"));
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
        var bits = new bool[count * DecRx02EncodingFormat.EncodedMfmByteBitCount + DecRx02EncodingFormat.M2FmPhaseBitCount];
        for (var index = 0; index < count * DecRx02EncodingFormat.EncodedMfmByteBitCount && start + index < stream.Bits.Length; index++) bits[index + DecRx02EncodingFormat.M2FmPhaseBitCount] = stream.Bits[start + index];
        var encodedRule = DecRx02EncodingFormat.EncodedM2FmRule;
        var normalRule = DecRx02EncodingFormat.NormalM2FmRule;
        for (var offset = 0; offset + encodedRule.Count <= bits.Length; offset++)
        {
            var matches = true; for (var index = 0; index < encodedRule.Count; index++) if (bits[offset + index] != encodedRule[index]) { matches = false; break; }
            if (offset % 2 != 0 || !matches) continue;
            for (var index = 0; index < normalRule.Count; index++) bits[offset + index] = normalRule[index];
            offset += encodedRule.Count - 2;
        }
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            byte value = 0;
            for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++) if (!bits[DecRx02EncodingFormat.M2FmPhaseBitCount + index * DecRx02EncodingFormat.EncodedMfmByteBitCount + bit * 2] && bits[DecRx02EncodingFormat.M2FmPhaseBitCount + index * DecRx02EncodingFormat.EncodedMfmByteBitCount + bit * 2 + 1]) value |= (byte)(1 << (BitPrimitives.BitsPerByte - 1 - bit));
            result[index] = value;
        }
        return result;
    }

    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * DecRx02EncodingFormat.EncodedFmByteBitCount, out result[index])) return null;
        return result;
    }
}
