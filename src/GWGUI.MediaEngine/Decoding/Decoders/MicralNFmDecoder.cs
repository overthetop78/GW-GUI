using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding;

public sealed class MicralNFmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = FluxEncoding.EncodeFm(0, 0, 0, 0xff);
    public override string Id => "micraln.fm"; public override string DisplayName => "Micral N hard-sectored FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "Micral N hard-sector block")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int markBits = 4 * 16; const int syncOffset = 3 * 16; const int blockBytes = 1 + 2 + 128 + 1;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            var blockStart = offset + syncOffset;
            var complete = blockStart + blockBytes * 16 <= stream.Bits.Length;
            if (complete)
            {
                var block = TryDecodeMfmBytes(stream, blockStart + 16, 131);
                if (block is null) continue;
                var number = block[0];
                var cylinder = block[1];
                var data = block.AsSpan(2, 128).ToArray();
                var storedChecksum = block[130];
                byte checksum = 0;
                foreach (var value in data) checksum = UpdateChecksum(checksum, value);
                var valid = checksum == storedChecksum;
                bytes.AddRange(data);
                sectors.Add(new(cylinder, 0, number, 0, 128, valid, offset, SectorIntegrityKind.Checksum));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, syncOffset + blockBytes * 16,
                    $"Micral N C{cylinder} R{number}, 128 bytes, checksum {(valid ? "valid" : "invalid")}"));
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "Micral N hard-sector block, checksum unavailable"));
            offset += markBits - 1;
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte UpdateChecksum(byte checksum, byte data)
    {
        var carrySource = ((data ^ checksum) ^ 0xff) & ((data + checksum) ^ data);
        var carry = (carrySource & 0x80) != 0 ? 1 : 0;
        return (byte)(checksum + data + carry);
    }

    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
