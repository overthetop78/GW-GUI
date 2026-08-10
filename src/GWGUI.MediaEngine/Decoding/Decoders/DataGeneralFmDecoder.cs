using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding;

public sealed class DataGeneralFmDecoder : IFluxDecoder
{
    private static readonly byte[] Sync = FluxEncoding.EncodeFm(0x00, 0x01);

    public string Id => "datageneral.fm";
    public string DisplayName => "Data General 2F";

    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals, fm: true);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var syncOffsets = FindAll(stream, Sync);

        for (var index = 0; index + 1 < syncOffsets.Count; index++)
        {
            var headerOffset = syncOffsets[index];
            var headerStart = headerOffset + 32;
            if (headerStart + 32 > stream.Bits.Length) continue;
            var cylinderByte = stream.DecodeMfmByte(headerStart);
            var sectorByte = stream.DecodeMfmByte(headerStart + 16);
            var cylinder = (byte)(cylinderByte & 0x7f);
            var head = (byte)(cylinderByte >> 7);
            var sectorNumber = sectorByte >> 2;
            if (sectorNumber > 7) continue;

            var dataOffset = syncOffsets[index + 1];
            if (dataOffset - headerStart > 256 || dataOffset <= headerStart + 31) continue;
            var dataStart = dataOffset + 32;
            const int dataBytes = 514;
            bool? valid = null;
            if (dataStart + dataBytes * 16 <= stream.Bits.Length)
            {
                var block = Enumerable.Range(0, dataBytes).Select(byteIndex => stream.DecodeMfmByte(dataStart + byteIndex * 16)).ToArray();
                var stored = (ushort)((block[512] << 8) | block[513]);
                valid = Checksum(block.AsSpan(0, 512)) == stored;
                bytes.AddRange(block.AsSpan(0, 512).ToArray());
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, 32 + dataBytes * 16,
                    $"Data General C{cylinder} H{head} R{sectorNumber}, 512 bytes, checksum {(valid == true ? "valid" : "invalid")}"));
            }

            sectors.Add(new(cylinder, head, sectorNumber, 2, 512, valid, headerOffset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, headerOffset, 64, $"Data General C{cylinder} H{head} R{sectorNumber}"));
            index++;
        }

        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static List<int> FindAll(FluxBitstream stream, IReadOnlyList<byte> pattern)
    {
        var offsets = new List<int>();
        for (var offset = 0; offset + pattern.Count * 8 <= stream.Bits.Length; offset++) if (stream.MatchBytes(offset, pattern)) offsets.Add(offset);
        return offsets;
    }

    private static ushort Checksum(ReadOnlySpan<byte> data)
    {
        ushort value = 0;
        for (var index = 0; index <= data.Length; index++)
        {
            var input = index < data.Length ? data[index] : (byte)0;
            value = (ushort)(((value & 0xff) ^ (value >> 8)) | (((value & 0xff) ^ input) << 8));
        }
        return value;
    }
}
