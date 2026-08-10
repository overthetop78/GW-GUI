using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding;

public sealed class NorthstarMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = FluxEncoding.EncodeMfm(0, 0, 0, 0, 0, 0, 0, 0xfb);
    public override string Id => "northstar.mfm"; public override string DisplayName => "NorthStar hard-sectored MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "NorthStar hard-sector block")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int signatureBits = 8 * 16;
        const int payloadBits = 512 * 16;
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, SectorMark)) continue;
            var hasIdentity = offset + signatureBits + 16 <= stream.Bits.Length; var fullBlock = offset + signatureBits + 16 + payloadBits + 16 <= stream.Bits.Length;
            var info = hasIdentity ? stream.DecodeMfmByte(offset + signatureBits) : (byte)0;
            var cylinder = (byte)(info >> 4); var sectorNumber = (byte)(info & 0x0f); bool? checksumValid = null;
            if (fullBlock)
            {
                byte checksum = 0; var data = new byte[512];
                for (var index = 0; index < 512; index++)
                {
                    var value = stream.DecodeMfmByte(offset + signatureBits + 16 + index * 16); data[index] = value;
                    checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1));
                }
                var stored = stream.DecodeMfmByte(offset + signatureBits + 16 + payloadBits);
                checksumValid = stored == checksum; bytes.Add(info); bytes.AddRange(data);
            }
            if (hasIdentity) sectors.Add(new(cylinder, 0, sectorNumber, 2, 512, checksumValid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, fullBlock ? signatureBits + 16 + payloadBits + 16 : signatureBits,
                fullBlock ? $"NorthStar C{cylinder} R{sectorNumber}, 512 bytes, checksum {(checksumValid == true ? "valid" : "invalid")}" : hasIdentity ? $"NorthStar C{cylinder} R{sectorNumber}, checksum unavailable" : "NorthStar hard-sector block"));
            offset += signatureBits - 1;
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }
}
