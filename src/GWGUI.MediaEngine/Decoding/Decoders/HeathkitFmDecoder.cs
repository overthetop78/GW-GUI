using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding;

public sealed class HeathkitFmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = FluxEncoding.EncodeFm(0, 0, 0, 0xbf);
    public override string Id => "heathkit.fm"; public override string DisplayName => "Heathkit hard-sectored FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "Heathkit hard-sector header")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveClock(revolution.FluxIntervals, fm: true);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int signatureBits = 4 * 16;
        const int headerTailBits = 4 * 16;
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            var complete = offset + signatureBits + headerTailBits <= stream.Bits.Length;
            if (complete)
            {
                var volume = ReverseBits(FluxBitReader.DecodeMfmByte(stream, offset + signatureBits));
                var cylinder = ReverseBits(FluxBitReader.DecodeMfmByte(stream, offset + signatureBits + 16));
                var sectorNumber = ReverseBits(FluxBitReader.DecodeMfmByte(stream, offset + signatureBits + 32));
                var stored = ReverseBits(FluxBitReader.DecodeMfmByte(stream, offset + signatureBits + 48));
                byte checksum = 0;
                foreach (var value in new[] { volume, cylinder, sectorNumber }) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
                var headerValid = stored == checksum; var dataOffset = FindNextMark(stream, offset + signatureBits + headerTailBits, (88 + 16) * 8); bool? dataValid = null; var structureEnd = offset + signatureBits + headerTailBits;
                bytes.AddRange([volume, cylinder, sectorNumber]);
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var dataEnd = dataOffset + signatureBits + 257 * 16;
                    if (dataEnd <= stream.Bits.Length)
                    {
                        var data = Enumerable.Range(0, 256).Select(index => ReverseBits(FluxBitReader.DecodeMfmByte(stream, dataOffset + signatureBits + index * 16))).ToArray();
                        var dataStored = ReverseBits(FluxBitReader.DecodeMfmByte(stream, dataOffset + signatureBits + 256 * 16)); byte dataChecksum = 0;
                        foreach (var value in data) { dataChecksum ^= value; dataChecksum = (byte)((dataChecksum >> 7) | (dataChecksum << 1)); }
                        dataValid = dataStored == dataChecksum; bytes.AddRange(data); structureEnd = dataEnd;
                        structures.Add(new(FluxStructureKind.FormatData, dataOffset, dataEnd - dataOffset, $"Heathkit data, 256 bytes, checksum {(dataValid == true ? "valid" : "invalid")}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, signatureBits, "Heathkit data, checksum unavailable"));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(cylinder, 0, sectorNumber, 1, 256, integrity, offset, SectorIntegrityKind.Checksum));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, signatureBits + headerTailBits, $"Heathkit volume {volume}, C{cylinder} R{sectorNumber}, header checksum {(headerValid ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
                offset = Math.Max(offset + signatureBits - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, signatureBits, "Heathkit hard-sector header"));
            if (!complete) offset += signatureBits - 1;
        }
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorMark) && !pairedData.Contains(offset) && structures.All(item => item.BitOffset != offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, signatureBits, "Unpaired Heathkit data block")); offset += signatureBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindNextMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - SectorMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorMark)) return offset;
        return -1;
    }

    private static byte ReverseBits(byte value) => Primitives.BitPrimitives.Reverse(value);
}
