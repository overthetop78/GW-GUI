namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode une piste Apple II selon la disposition RWTS18 à six secteurs de Brøderbund.</summary>
public sealed class AppleRwts18TrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => AppleRwts18Format.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => AppleRwts18Format.CodecDisplayName;

    /// <summary>Encode les six secteurs d'une piste RWTS18 en cellules GCR 6-and-2.</summary>
    /// <param name="request">Piste logique contenant le cylindre, les secteurs et l'identifiant RWTS18 éventuel.</param>
    /// <returns>Cellules binaires de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La piste ne contient pas six secteurs RWTS18 de la taille attendue.</exception>
    /// <remarks>Les secteurs sont émis dans l'ordre décroissant imposé par RWTS18.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        ValidateTrack(request);
        var bits = TrackBitEncoding.Bits();
        var identifierValue = Attribute(request, AppleRwts18Format.IdentifierAttributeName, AppleRwts18Format.DefaultIdentifier);
        ValidateValue(AppleRwts18Format.IdentifierAttributeName, identifierValue, AppleRwts18Format.MaximumIdentifier);
        foreach (var sector in AppleRwts18Format.OrderForEncoding(request.Sectors))
        {
            bits.Gap(sector.Number == AppleRwts18Format.LastSectorNumber ? AppleRwts18Format.FirstSectorGapBitCount : AppleRwts18Format.OtherSectorGapBitCount, true);
            bits.Raw(BuildAddressField(request.Cylinder, sector.Number));
            bits.Raw((byte)identifierValue);
            bits.Raw(AppleRwts18Codec.EncodePayload(sector.Data));
            bits.Raw(AppleRwts18Format.DataEpilogue, AppleRwts18Format.SyncByte);
        }
        return bits;
    }

    /// <summary>Construit la marque, l'adresse, son checksum XOR, la fin d'adresse et les octets de synchronisation.</summary>
    private static byte[] BuildAddressField(int cylinder, int sector) => [(byte)(AppleRwts18Format.EncodedAddressMark >> Primitives.BitPrimitives.BitsPerByte), (byte)(AppleRwts18Format.EncodedAddressMark & byte.MaxValue), AppleIIGcrFormat.SixAndTwoTable[cylinder], AppleIIGcrFormat.SixAndTwoTable[sector], AppleIIGcrFormat.SixAndTwoTable[cylinder ^ sector], AppleRwts18Format.AddressTrailer, AppleRwts18Format.SyncByte, AppleRwts18Format.SyncByte];

    /// <summary>Valide le cylindre et l'ensemble exact des six secteurs RWTS18 avant l'encodage.</summary>
    private static void ValidateTrack(TrackEncodeRequest request)
    {
        ValidateValue(nameof(request.Cylinder), request.Cylinder, AppleRwts18Format.MaximumCylinder);
        if (request.Sectors.Count != AppleRwts18Format.SectorCount || !request.Sectors.Select(sector => sector.Number).Order().SequenceEqual(Enumerable.Range(0, AppleRwts18Format.SectorCount))) throw AppleRwts18Format.InvalidTrackLayout(request.Sectors.Count);
        foreach (var sector in request.Sectors)
            if (sector.Data.Count != AppleRwts18Format.SectorByteCount) throw AppleRwts18Format.InvalidSectorSize(sector.Number, sector.Data.Count);
    }

    /// <summary>Valide une valeur entière avant son encodage dans un champ RWTS18.</summary>
    private static void ValidateValue(string field, int value, int maximum)
    {
        if (value is < 0 || value > maximum) throw TrackEncodingExceptions.FormatValueOutOfRange("Apple II RWTS18", field, value, maximum);
    }
}
