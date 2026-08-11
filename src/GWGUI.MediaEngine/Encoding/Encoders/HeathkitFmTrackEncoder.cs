using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Heathkit FM.</summary>
public sealed class HeathkitFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.HeathkitFm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.HeathkitFm;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        var volume = (byte)Attribute(request, "volume", 0);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != HeathkitFmFormat.SectorSize) throw HeathkitFmFormat.InvalidSectorSize(sector.Data.Count);
            byte[] identity = [volume, (byte)request.Cylinder, (byte)sector.Number];
            bits.Raw(HeathkitFmFormat.SectorMark.ToArray());
            bits.Fm(identity.Append(TrackEncoding.RotatingChecksum(identity)).Select(Primitives.BitPrimitives.ReverseBits));
            bits.Gap(HeathkitFmFormat.HeaderGapBitCount);
            bits.Raw(HeathkitFmFormat.SectorMark.ToArray());
            bits.Fm(sector.Data.Append(TrackEncoding.RotatingChecksum(sector.Data)).Select(Primitives.BitPrimitives.ReverseBits));
            bits.Gap(HeathkitFmFormat.DataGapBitCount);
        }
        return bits;
    }
}
