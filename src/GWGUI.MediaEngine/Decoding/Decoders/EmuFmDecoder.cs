using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

public sealed class EmuFmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = EmuFmFormat.SectorMark.ToArray();
    public override string Id => FluxCodecIds.EmuFm; public override string DisplayName => FluxCodecDisplayNames.EmuFm;
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "E-mu Emulator header/data mark")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedMarks = new HashSet<int>();
        var markBits = SectorMark.Length * Primitives.BitPrimitives.BitsPerByte;
        const int headerBits = EmuFmFormat.HeaderDecodedByteCount * 32 + 2 * Primitives.BitPrimitives.BitsPerByte;
        const int sectorSize = EmuFmFormat.SectorSize;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark) || offset + headerBits > stream.Bits.Length) continue;
            var header = TryDecodeFmBytes(stream, offset + markBits, EmuFmFormat.HeaderDecodedByteCount);
            if (header is null) continue;
            var rawTrack = header[0];
            var crcHigh = header[1]; var crcLow = header[2];
            if (Primitives.Crc16Calculator.Compute([rawTrack, crcHigh, crcLow], EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue) != 0) continue;

            var track = Primitives.BitPrimitives.ReverseBits(rawTrack); var cylinder = (byte)(track >> EmuFmFormat.TrackShift); var head = (byte)(track & EmuFmFormat.HeadMask); bytes.Add(track); classifiedMarks.Add(offset);
            var dataOffset = FindNextMark(stream, offset + 4 * Primitives.BitPrimitives.BitsPerByte * 4, (88 + 16) * Primitives.BitPrimitives.BitsPerByte * 2);
            var completeData = dataOffset >= 0 && dataOffset + markBits + (sectorSize + 2) * 32 <= stream.Bits.Length;
            bool? dataCrcValid = null;
            if (completeData)
            {
                var block = TryDecodeFmBytes(stream, dataOffset + markBits, sectorSize + 2);
                if (block is null) continue;
                ushort crc = EmuFmFormat.CrcInitialValue; var data = new byte[sectorSize];
                for (var index = 0; index < block.Length; index++) { var value = block[index]; crc = Primitives.Crc16Calculator.Update(crc, value, EmuFmFormat.CrcPolynomial); if (index < sectorSize) data[index] = value; }
                dataCrcValid = crc == 0; classifiedMarks.Add(dataOffset); bytes.AddRange(data);
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits + (sectorSize + 2) * 32, $"E-mu C{cylinder} H{head} data, CRC {(dataCrcValid == true ? "valid" : "invalid")}"));
            }
            sectors.Add(new(cylinder, head, 1, 0, sectorSize, dataCrcValid, offset));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"E-mu C{cylinder} H{head} R1, 3584 bytes, header CRC valid{(completeData ? $", data CRC {(dataCrcValid == true ? "valid" : "invalid")}" : ", data CRC unavailable")}"));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!classifiedMarks.Contains(offset) && FluxBitReader.MatchBytes(stream, offset, SectorMark)) structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "E-mu Emulator unclassified header/data mark"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static int FindNextMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - SectorMark.Length * Primitives.BitPrimitives.BitsPerByte, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorMark)) return offset;
        return -1;
    }

    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * 32, out result[index])) return null;
        return result;
    }
}
