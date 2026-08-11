using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encodes the zoned 512-byte GCR sectors used by the Commodore 900.</summary>
public sealed class Commodore900GcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => Commodore900GcrFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => Commodore900GcrFormat.CodecDisplayName;

    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != Commodore900GcrFormat.SectorByteCount) throw Commodore900GcrFormat.InvalidSectorSize(sector.Data.Count);
            var headerChecksum = CommodoreGcrChecksum.Calculate([Commodore900GcrFormat.HeaderMark, (byte)request.Cylinder, (byte)sector.Number]);
            var dataChecksum = CommodoreGcrChecksum.Calculate(new byte[] { Commodore900GcrFormat.DataMark }.Concat(sector.Data));

            bits.Gap(Commodore900GcrFormat.SyncGapBitCount, true);
            bits.AddRange(CommodoreGcrCodec.Encode([Commodore900GcrFormat.HeaderMark, (byte)request.Cylinder, (byte)sector.Number, headerChecksum]));
            bits.Gap(Commodore900GcrFormat.RecordGapBitCount);
            bits.Gap(Commodore900GcrFormat.SyncGapBitCount, true);
            bits.AddRange(CommodoreGcrCodec.Encode(new byte[] { Commodore900GcrFormat.DataMark }.Concat(sector.Data).Append(dataChecksum)));
            bits.Gap(Commodore900GcrFormat.RecordGapBitCount);
        }
        return bits;
    }

}
