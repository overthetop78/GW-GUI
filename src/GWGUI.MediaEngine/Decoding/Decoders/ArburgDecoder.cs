using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

public sealed class ArburgDecoder : SignatureMfmDecoder
{
    private static readonly byte[] DataMark = [0x44,0x44,0x44,0x44,0x55,0x55,0x55,0x55];
    private static readonly byte[] SystemMark = [0x55,0x55,0x55,0x55,0x55,0x24,0x92,0x49];
    public override string Id => FluxCodecIds.Arburg; public override string DisplayName => "Arburg system/data";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(DataMark, FluxStructureKind.FormatData, "Arburg data block"), (SystemMark, FluxStructureKind.FormatHeader, "Arburg system block")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        ScanFmData(stream, structures, sectors, bytes);
        ScanSystemData(stream, structures, sectors, bytes);
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 8d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static void ScanFmData(FluxBitstream stream, List<FluxStructure> structures, List<DecodedSector> sectors, List<byte> bytes)
    {
        const int markBits = 8 * Primitives.BitPrimitives.BitsPerByte, blockSize = 0xa00, usefulSize = 0x9fe;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, DataMark)) continue;
            var complete = offset + markBits + blockSize * 32 <= stream.Bits.Length; bool? valid = null;
            if (complete)
            {
                var decoded = TryDecodeFmBytes(stream, offset + markBits, blockSize);
                if (decoded is null) continue;
                ushort checksum = 0; var data = new byte[usefulSize];
                for (var index = 0; index < usefulSize; index++) { var value = Primitives.BitPrimitives.ReverseBits(decoded[index]); data[index] = value; checksum += value; }
                var low = Primitives.BitPrimitives.ReverseBits(decoded[usefulSize]); var high = Primitives.BitPrimitives.ReverseBits(decoded[usefulSize + 1]);
                valid = low == (byte)checksum && high == (byte)(checksum >> Primitives.BitPrimitives.BitsPerByte); bytes.AddRange(data);
            }
            sectors.Add(new(0, 0, 1, 0, blockSize, valid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatData, offset, complete ? markBits + blockSize * 32 : markBits, $"Arburg data block, 2560 bytes, checksum {(valid is null ? "unavailable" : valid == true ? "valid" : "invalid")}"));
            offset += markBits - 1;
        }
    }

    private static void ScanSystemData(FluxBitstream stream, List<FluxStructure> structures, List<DecodedSector> sectors, List<byte> bytes)
    {
        const int markBits = 8 * Primitives.BitPrimitives.BitsPerByte, blockSize = 0xf00, usefulSize = 0xefe;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SystemMark)) continue;
            var decoded = TryDecodeSystemBytes(stream, offset + markBits, blockSize); bool? valid = null;
            if (decoded is not null)
            {
                ushort checksum = 0; for (var index = 0; index < usefulSize; index++) checksum += decoded.Value.Bytes[index];
                valid = decoded.Value.Bytes[usefulSize] == (byte)checksum && decoded.Value.Bytes[usefulSize + 1] == (byte)(checksum >> Primitives.BitPrimitives.BitsPerByte); bytes.AddRange(decoded.Value.Bytes.Take(usefulSize));
            }
            sectors.Add(new(0, 0, 1, 0, blockSize, valid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, decoded is null ? markBits : decoded.Value.EndOffset - offset, $"Arburg system block, 3840 bytes, checksum {(valid is null ? "unavailable" : valid == true ? "valid" : "invalid")}"));
            offset += markBits - 1;
        }
    }

    private static (byte[] Bytes, int EndOffset)? TryDecodeSystemBytes(FluxBitstream stream, int start, int count)
    {
        var result = new byte[count]; var offset = start;
        for (var index = 0; index < count; index++)
        {
            byte value = 0;
            for (var bit = 0; bit < Primitives.BitPrimitives.BitsPerByte; bit++)
            {
                if (offset + 2 > stream.Bits.Length || stream.Bits[offset]) return null;
                if (stream.Bits[offset + 1]) offset += 2;
                else
                {
                    if (offset + 3 > stream.Bits.Length || !stream.Bits[offset + 2]) return null;
                    value |= (byte)(1 << bit); offset += 3;
                }
            }
            result[index] = value;
        }
        return (result, offset);
    }

    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * 32, out result[index])) return null;
        return result;
    }
}
