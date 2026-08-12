using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Victor9k GCR.</summary>
public sealed class Victor9kGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => Victor9kGcrFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => Victor9kGcrFormat.CodecDisplayName;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != Victor9kGcrFormat.SectorByteCount) throw Victor9kGcrFormat.InvalidSectorSize(sector.Data.Count);
            byte[] header = [Victor9kGcrFormat.HeaderType, (byte)request.Cylinder, (byte)sector.Number, (byte)(request.Cylinder + sector.Number), Victor9kGcrFormat.HeaderId2, Victor9kGcrFormat.HeaderId1];
            var checksum = Victor9kChecksum.Compute(sector.Data);
            AddBlock(bits, Victor9kGcrFormat.HeaderMark, header);
            bits.Gap(Victor9kGcrFormat.HeaderGapBitCount);
            AddBlock(bits, Victor9kGcrFormat.DataMark, new[] { Victor9kGcrFormat.DataPrefix }.Concat(sector.Data).Concat([(byte)checksum, (byte)(checksum >> 8)]));
            bits.Gap(Victor9kGcrFormat.DataGapBitCount);
        }
        return bits;
    }
    /// <summary>Exécute le traitement « Add Block » propre à ce format.</summary>
    private static void AddBlock(List<bool> target, IReadOnlyList<byte> marker, IEnumerable<byte> values)
    {
        var block = new List<bool>();
        block.Raw(marker.ToArray());
        CommodoreGcrCodec.Write(block, Victor9kGcrFormat.EncodedDataStartBitOffset, values, Victor9kGcrFormat.EncodedCellStride);
        target.AddRange(block);
    }
}
