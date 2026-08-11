using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

public sealed class EmuFmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = [0x45,0x45,0x55,0x55,0x45,0x54,0x54,0x45];
    public override string Id => "emu.fm"; public override string DisplayName => "E-mu Emulator FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "E-mu Emulator header/data mark")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveClock(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedMarks = new HashSet<int>();
        const int markBits = 8 * 8;
        const int headerBits = 5 * 32;
        const int sectorSize = 0xe00;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark) || offset + headerBits > stream.Bits.Length) continue;
            var rawTrack = FluxBitReader.DecodeFmByte32(stream, offset + markBits);
            var crcHigh = FluxBitReader.DecodeFmByte32(stream, offset + markBits + 32); var crcLow = FluxBitReader.DecodeFmByte32(stream, offset + markBits + 64);
            if (Crc16([rawTrack, crcHigh, crcLow]) != 0) continue;

            var track = ReverseBits(rawTrack); var cylinder = (byte)(track >> 1); var head = (byte)(track & 1); bytes.Add(track); classifiedMarks.Add(offset);
            var dataOffset = FindNextMark(stream, offset + 4 * 8 * 4, (88 + 16) * 8 * 2);
            var completeData = dataOffset >= 0 && dataOffset + markBits + (sectorSize + 2) * 32 <= stream.Bits.Length;
            bool? dataCrcValid = null;
            if (completeData)
            {
                ushort crc = 0; var data = new byte[sectorSize];
                for (var index = 0; index < sectorSize + 2; index++) { var value = FluxBitReader.DecodeFmByte32(stream, dataOffset + markBits + index * 32); crc = UpdateCrc(crc, value); if (index < sectorSize) data[index] = value; }
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
        var end = Math.Min(stream.Bits.Length - SectorMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorMark)) return offset;
        return -1;
    }

    private static byte ReverseBits(byte value) => Primitives.BitPrimitives.Reverse(value);

    private static ushort Crc16(IEnumerable<byte> values) => Primitives.Crc16Calculator.Compute(values, polynomial: 0x8005, initial: 0);

    private static ushort UpdateCrc(ushort crc, byte value) => Primitives.Crc16Calculator.Update(crc, value, polynomial: 0x8005);
}
