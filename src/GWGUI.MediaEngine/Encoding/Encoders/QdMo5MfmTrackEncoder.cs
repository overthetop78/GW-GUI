using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Qd Mo5 MFM.</summary>
public sealed class QdMo5MfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => QdMo5MfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => QdMo5MfmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != QdMo5MfmFormat.SectorSize) throw QdMo5MfmFormat.InvalidSectorSize(sector.Data.Count);
            bits.Raw(QdMo5MfmFormat.HeaderMark.ToArray());
            bits.Mfm(new byte[] { (byte)(sector.Number >> BitPrimitives.BitsPerByte),(byte)sector.Number }.Concat(new byte[QdMo5MfmFormat.HeaderPaddingByteCount]));
            bits.Gap(QdMo5MfmFormat.HeaderGapBitCount);
            bits.Raw(QdMo5MfmFormat.Preamble.ToArray());
            var prefix = (byte)Attribute(sector, QdMo5MfmFormat.PrefixAttribute, QdMo5MfmFormat.DefaultPrefix);
            var checksum = QdMo5Checksum.Compute(prefix, sector.Data);
            bits.Mfm(new[] { prefix }.Concat(sector.Data).Append(checksum));
            bits.Gap(QdMo5MfmFormat.DataGapBitCount);
        }
        return bits;
    }
}
