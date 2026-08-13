using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Commodore GCR.</summary>
public sealed class CommodoreGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => CommodoreGcrFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => CommodoreGcrFormat.CodecDisplayName;

    /// <summary>Encode les secteurs Commodore avec leurs en-têtes, identifiants et sommes de contrôle.</summary>
    /// <param name="request">Piste logique, secteurs et attributs d'identification du disque.</param>
    /// <returns>Cellules GCR de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille Commodore attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        var id2 = ReadByteAttribute(request, CommodoreGcrFormat.Id2AttributeName, CommodoreGcrFormat.DefaultId2);
        var id1 = ReadByteAttribute(request, CommodoreGcrFormat.Id1AttributeName, CommodoreGcrFormat.DefaultId1);
        var diskTrack = ResolveDiskTrack(request);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != CommodoreGcrFormat.SectorByteCount) throw CommodoreGcrFormat.InvalidSectorSize(sector.Data.Count);
            ValidateByte(nameof(sector.Number), sector.Number);
            bits.Gap(CommodoreGcrFormat.LeadingGapBitCount, true);
            bits.RawBits(new string('0', CommodoreGcrFormat.RawGapBitCount));
            bits.Gap(CommodoreGcrFormat.SyncGapBitCount, true);
            bits.AddRange(CommodoreGcrCodec.Encode(BuildHeader((byte)sector.Number, (byte)diskTrack, id2, id1)));
            bits.Gap(CommodoreGcrFormat.HeaderDataGapBitCount);
            bits.Gap(CommodoreGcrFormat.SyncGapBitCount, true);
            bits.AddRange(CommodoreGcrCodec.Encode(BuildDataRecord(sector.Data)));
            bits.Gap(CommodoreGcrFormat.TrailingGapBitCount);
        }
        return bits;
    }

    /// <summary>Calcule ou lit le numéro de piste disque et vérifie sa plage standard.</summary>
    private static int ResolveDiskTrack(TrackEncodeRequest request)
    {
        if (request.Cylinder > CommodoreGcrFormat.MaximumCylinder) throw TrackEncodingExceptions.FormatValueOutOfRange("Commodore GCR", nameof(request.Cylinder), request.Cylinder, CommodoreGcrFormat.MaximumCylinder);
        var tracksPerSide = Attribute(request, TrackEncodingAttributeKeys.TracksPerSide, CommodoreGcrFormat.TracksPerSide);
        var diskTrack = Attribute(request, CommodoreGcrFormat.TrackAttributeName, request.Cylinder + CommodoreGcrFormat.MinimumDiskTrack + request.Head * tracksPerSide);
        if (diskTrack is < CommodoreGcrFormat.MinimumDiskTrack or > CommodoreGcrFormat.MaximumDiskTrack) throw new ArgumentOutOfRangeException(CommodoreGcrFormat.TrackAttributeName, diskTrack, $"Commodore disk track must be between {CommodoreGcrFormat.MinimumDiskTrack} and {CommodoreGcrFormat.MaximumDiskTrack}.");
        return diskTrack;
    }

    /// <summary>Construit les six octets d'en-tête dans l'ordre marque, checksum, secteur, piste, ID2 et ID1.</summary>
    private static byte[] BuildHeader(byte sector, byte diskTrack, byte id2, byte id1) => [CommodoreGcrFormat.HeaderMark, (byte)(sector ^ diskTrack ^ id2 ^ id1), sector, diskTrack, id2, id1];

    /// <summary>Construit le champ de données avec sa marque et son checksum XOR.</summary>
    private static byte[] BuildDataRecord(IReadOnlyList<byte> data) => new byte[] { CommodoreGcrFormat.DataMark }.Concat(data).Append(CommodoreGcrChecksum.Calculate(data)).ToArray();

    /// <summary>Lit un attribut de requête dont la valeur doit tenir dans un octet.</summary>
    private static byte ReadByteAttribute(TrackEncodeRequest request, string name, byte fallback)
    {
        var value = Attribute(request, name, fallback);
        ValidateByte(name, value);
        return (byte)value;
    }

    /// <summary>Valide une valeur avant sa conversion en octet.</summary>
    private static void ValidateByte(string field, int value)
    {
        if (value is < 0 || value > CommodoreGcrFormat.MaximumByteValue) throw TrackEncodingExceptions.FormatValueOutOfRange("Commodore GCR", field, value, CommodoreGcrFormat.MaximumByteValue);
    }
}
