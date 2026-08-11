using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding;

public sealed class NorthstarMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = FluxEncoding.EncodeMfm(0, 0, 0, 0, 0, 0, 0, 0xfb);
    public override string Id => FluxCodecIds.NorthstarMfm; public override string DisplayName => FluxCodecDisplayNames.NorthstarMfm;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "NorthStar hard-sector block")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int signatureBits = 8 * 16;
        const int payloadBits = 512 * 16;
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            var hasIdentity = offset + signatureBits + 16 <= stream.Bits.Length; var fullBlock = offset + signatureBits + 16 + payloadBits + 16 <= stream.Bits.Length;
            byte info = 0;
            if (hasIdentity && !FluxBitReader.TryDecodeMfmByte(stream, offset + signatureBits, out info)) continue;
            var cylinder = (byte)(info >> 4); var sectorNumber = (byte)(info & 0x0f); bool? checksumValid = null;
            if (fullBlock)
            {
                byte checksum = 0; var data = TryDecodeMfmBytes(stream, offset + signatureBits + 16, 512);
                if (data is null) continue;
                for (var index = 0; index < data.Length; index++)
                {
                    var value = data[index];
                    checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1));
                }
                if (!FluxBitReader.TryDecodeMfmByte(stream, offset + signatureBits + 16 + payloadBits, out var stored)) continue;
                checksumValid = stored == checksum; bytes.Add(info); bytes.AddRange(data);
            }
            if (hasIdentity) sectors.Add(new(cylinder, 0, sectorNumber, 2, 512, checksumValid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, fullBlock ? signatureBits + 16 + payloadBits + 16 : signatureBits,
                fullBlock ? $"NorthStar C{cylinder} R{sectorNumber}, 512 bytes, checksum {(checksumValid == true ? "valid" : "invalid")}" : hasIdentity ? $"NorthStar C{cylinder} R{sectorNumber}, checksum unavailable" : "NorthStar hard-sector block"));
            offset += signatureBits - 1;
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
