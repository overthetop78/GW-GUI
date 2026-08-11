using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

public sealed class AmigaMfmDecoder : IFluxDecoder
{
    public string Id => FluxCodecIds.AmigaMfm; public string DisplayName => "Amiga MFM";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int encodedBytes = 540; const int headerBytes = 28; const int dataOffset = 28; const int dataBytes = 512;
        for (var offset = 0; offset + 32 <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, 0x4489) || !FluxBitReader.Match(stream, offset + 16, 0x4489)) continue;
            var encoded = TryDecodeMfmBytes(stream, offset + 32, encodedBytes); var available = encoded ?? TryDecodeMfmBytes(stream, offset + 32, headerBytes);
            bool? headerValid = null; bool? dataValid = null; byte cylinder = 0; byte head = 0; byte number = 0; var length = 32; byte[]? payload = null;
            if (available is not null)
            {
                var header = DecodeOddEven(available.Take(4).ToArray()); cylinder = (byte)(header[1] >> 1); head = (byte)(header[1] & 1); number = header[2];
                var headerParity = CalculateParity(available, 0, 20); headerValid = header[0] == 0xff && available[22] == headerParity.High && available[23] == headerParity.Low;
                bytes.AddRange(header); length = 32 + available.Length * 16;
                if (encoded is not null)
                {
                    var parity = CalculateSplitParity(encoded, dataOffset, dataBytes); dataValid = encoded[26] == parity.High && encoded[27] == parity.Low;
                    payload = DecodeOddEven(encoded.Skip(dataOffset).Take(dataBytes).ToArray()); bytes.AddRange(payload); length = 32 + encodedBytes * 16;
                }
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, head, number, 2, 512, integrity, offset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.AmigaSync, offset, length, $"Amiga C{cylinder} H{head} S{number}, header checksum {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
            offset += Math.Max(31, length - 1);
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 3 + structures.Count) / 44d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * 16 > stream.Bits.Length) return null; var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
    private static byte[] DecodeOddEven(IReadOnlyList<byte> encoded)
    {
        var result = new byte[encoded.Count]; var half = encoded.Count / 2;
        for (var index = 0; index < half; index++)
        {
            var odd = encoded[index]; var even = encoded[index + half]; result[index * 2] = Interleave((byte)(odd >> 4), (byte)(even >> 4)); result[index * 2 + 1] = Interleave((byte)(odd & 15), (byte)(even & 15));
        }
        return result;
    }
    private static byte Interleave(byte odd, byte even)
    {
        byte value = 0; for (var index = 0; index < 4; index++) { value |= (byte)(((odd >> (3 - index)) & 1) << (7 - index * 2)); value |= (byte)(((even >> (3 - index)) & 1) << (6 - index * 2)); } return value;
    }
    private static (byte High, byte Low) CalculateParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
        byte high = 0, low = 0; for (var index = 0; index < count; index += 4) { high ^= (byte)(encoded[offset + index] ^ encoded[offset + index + 2]); low ^= (byte)(encoded[offset + index + 1] ^ encoded[offset + index + 3]); } return (high, low);
    }
    private static (byte High, byte Low) CalculateSplitParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
        byte high = 0, low = 0; var half = count / 2;
        for (var index = 0; index < half; index += 2) { high ^= (byte)(encoded[offset + index] ^ encoded[offset + half + index]); low ^= (byte)(encoded[offset + index + 1] ^ encoded[offset + half + index + 1]); } return (high, low);
    }
}
