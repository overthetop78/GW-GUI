using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode une piste Apple II selon la disposition RWTS18 à six secteurs de Brøderbund.</summary>
public sealed class AppleRwts18TrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleRwts18;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleRwts18;

    /// <summary>Encode les six secteurs d'une piste RWTS18 en cellules GCR 6-and-2.</summary>
    /// <param name="request">Piste logique contenant le cylindre, les secteurs et l'identifiant RWTS18 éventuel.</param>
    /// <returns>Cellules binaires de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La piste ne contient pas six secteurs RWTS18 de la taille attendue.</exception>
    /// <remarks>Les secteurs sont émis dans l'ordre décroissant imposé par RWTS18.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors.OrderByDescending(sector => sector.Number))
        {
            if (sector.Number is < 0 or >= AppleRwts18Format.SectorCount || sector.Data.Count != AppleRwts18Format.SectorByteCount)
                throw AppleRwts18Format.InvalidTrackLayout(request.Sectors.Count);
            bits.Gap(sector.Number == AppleRwts18Format.LastSectorNumber ? AppleRwts18Format.FirstSectorGapBitCount : AppleRwts18Format.OtherSectorGapBitCount, true);
            bits.Raw((byte)(AppleRwts18Format.EncodedAddressMark >> BitPrimitives.BitsPerByte), (byte)(AppleRwts18Format.EncodedAddressMark & byte.MaxValue), AppleIIGcrFormat.SixAndTwoTable[request.Cylinder & AppleRwts18Format.SixBitMask], AppleIIGcrFormat.SixAndTwoTable[sector.Number], AppleIIGcrFormat.SixAndTwoTable[(request.Cylinder ^ sector.Number) & AppleRwts18Format.SixBitMask], AppleRwts18Format.AddressTrailer, AppleRwts18Format.SyncByte, AppleRwts18Format.SyncByte);
            bits.Raw((byte)Attribute(request, AppleRwts18Format.IdentifierAttributeName, AppleRwts18Format.DefaultIdentifier));
            bits.Raw(AppleRwts18Codec.EncodePayload(sector.Data));
            bits.Raw(AppleRwts18Format.DataEpilogue, AppleRwts18Format.SyncByte);
        }
        return bits;
    }

}
