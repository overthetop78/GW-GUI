using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

public sealed class HpMmfmDecoder : IFluxDecoder
{
    private static readonly byte[] SectorSync = HpMmfmFormat.SectorSync.ToArray();
    private static readonly byte[] DataSync = HpMmfmFormat.DataSync.ToArray();

    public string Id => FluxCodecIds.HpMmfm;
    public string DisplayName => FluxCodecDisplayNames.HpMmfm;

    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var usedDataOffsets = new HashSet<int>();

        for (var offset = 0; offset + 32 + 64 <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorSync)) continue;

            var id = TryDecodeBytes(stream, offset + 32, 4);
            if (id is null) continue;
            var headerValid = Primitives.Crc16Calculator.Compute(id) == 0;
            var cylinder = Primitives.BitPrimitives.ReverseBits(id[0]);
            var encodedSector = Primitives.BitPrimitives.ReverseBits(id[1]);
            var head = (byte)(encodedSector >> HpMmfmFormat.HeadShift);
            var sectorNumber = encodedSector & HpMmfmFormat.SectorMask;
            var dataOffset = Find(stream, offset + 32 + 8 * 16, Math.Min(stream.Bits.Length, offset + 32 + 58 * 16), DataSync);
            bool? dataValid = null;

            if (dataOffset >= 0 && usedDataOffsets.Add(dataOffset))
            {
                const int encodedBytes = HpMmfmFormat.EncodedDataByteCount;
                var dataStart = dataOffset + 32;
                if (dataStart + encodedBytes * 16 <= stream.Bits.Length)
                {
                    var encoded = TryDecodeBytes(stream, dataStart, encodedBytes);
                    if (encoded is null) continue;
                    dataValid = Primitives.Crc16Calculator.Compute(encoded) == 0;
                    var payload = encoded.AsSpan(0, HpMmfmFormat.SectorSize).ToArray();
                    for (var index = 0; index < payload.Length; index++) payload[index] = Primitives.BitPrimitives.ReverseBits(payload[index]);
                    for (var index = 0; index < payload.Length; index += 2) (payload[index], payload[index + 1]) = (payload[index + 1], payload[index]);
                    bytes.AddRange(payload);
                    structures.Add(new(FluxStructureKind.FormatData, dataOffset, 32 + encodedBytes * 16,
                        $"HP MMFM C{cylinder} H{head} R{sectorNumber}, 256 bytes, data CRC {(dataValid == true ? "valid" : "invalid")}"));
                }
            }

            bool? integrity = !headerValid || dataValid == false
                ? false
                : dataValid is null ? null : true;
            sectors.Add(new(cylinder, head, sectorNumber, 1, HpMmfmFormat.SectorSize, integrity, offset));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, 96,
                $"HP MMFM C{cylinder} H{head} R{sectorNumber}, header CRC {(headerValid ? "valid" : "invalid")}"));
            offset += 31;
        }

        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte[]? TryDecodeBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }

    private static int Find(FluxBitstream stream, int start, int end, IReadOnlyList<byte> pattern)
    {
        for (var offset = start; offset + pattern.Count * Primitives.BitPrimitives.BitsPerByte <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, pattern)) return offset;
        return -1;
    }

}
