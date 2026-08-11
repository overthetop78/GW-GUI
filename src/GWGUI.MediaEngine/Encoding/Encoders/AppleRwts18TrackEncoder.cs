using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encodes the standard Brøderbund RWTS18 six-sector track layout.</summary>
public sealed class AppleRwts18TrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleRwts18;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleRwts18;

    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors.OrderByDescending(sector => sector.Number))
        {
            if (sector.Number is < 0 or >= AppleRwts18Format.SectorCount || sector.Data.Count != AppleRwts18Format.SectorByteCount)
                throw AppleRwts18Format.InvalidTrackLayout(request.Sectors.Count);
            bits.Gap(sector.Number == AppleRwts18Format.LastSectorNumber ? AppleRwts18Format.FirstSectorGapBitCount : AppleRwts18Format.OtherSectorGapBitCount, true);
            bits.Raw((byte)(AppleRwts18Format.EncodedAddressMark >> BitPrimitives.BitsPerByte), (byte)(AppleRwts18Format.EncodedAddressMark & byte.MaxValue), AppleRwts18Format.NibbleTable[request.Cylinder & AppleRwts18Format.SixBitMask], AppleRwts18Format.NibbleTable[sector.Number],
                AppleRwts18Format.NibbleTable[(request.Cylinder ^ sector.Number) & AppleRwts18Format.SixBitMask], AppleRwts18Format.AddressTrailer, AppleRwts18Format.SyncByte, AppleRwts18Format.SyncByte);
            bits.Raw((byte)Attribute(request, AppleRwts18Format.IdentifierAttributeName, AppleRwts18Format.DefaultIdentifier));
            bits.Raw(AppleRwts18Codec.EncodePayload(sector.Data));
            bits.Raw(AppleRwts18Format.DataEpilogue, AppleRwts18Format.SyncByte);
        }
        return bits;
    }

}
