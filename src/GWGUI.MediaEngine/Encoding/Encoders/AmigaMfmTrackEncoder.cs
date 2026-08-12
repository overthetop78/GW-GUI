namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Amiga MFM.</summary>
public sealed class AmigaMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => AmigaMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => AmigaMfmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs demandés sous forme de cellules MFM Amiga odd/even.</summary>
    /// <param name="request">Piste logique contenant cylindre, face et secteurs de 512 octets.</param>
    /// <returns>Cellules binaires de la piste, dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne contient pas exactement 512 octets.</exception>
    /// <remarks>Chaque bloc conserve l'entrelacement odd/even et les parités distinctes de l'en-tête et des données.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        ValidateTrack(request);
        var bits = TrackBitEncoding.Bits();
        for (var sectorIndex = 0; sectorIndex < request.Sectors.Count; sectorIndex++)
        {
            var sector = request.Sectors[sectorIndex];
            if (sector.Data.Count != AmigaMfmFormat.SectorByteCount) throw AmigaMfmFormat.InvalidSectorSize(sector.Data.Count);
            ValidateSector(sector.Number);
            var remaining = request.Sectors.Count - sectorIndex;
            var encoded = BuildSector(request.Cylinder, request.Head, sector.Number, remaining, sector.Data);
            bits.Gap(AmigaMfmFormat.LeadingGapBitCount);
            bits.Raw((byte)(AmigaMfmFormat.SyncWord >> Primitives.BitPrimitives.BitsPerByte), unchecked((byte)AmigaMfmFormat.SyncWord), (byte)(AmigaMfmFormat.SyncWord >> Primitives.BitPrimitives.BitsPerByte), unchecked((byte)AmigaMfmFormat.SyncWord));
            bits.Mfm(encoded);
            bits.Gap(AmigaMfmFormat.TrailingGapBitCount);
        }
        return bits;
    }

    /// <summary>Valide le cylindre, la face et le nombre de secteurs de la piste.</summary>
    private static void ValidateTrack(TrackEncodeRequest request)
    {
        if (request.Cylinder is < 0 or > AmigaMfmFormat.MaximumCylinder) throw TrackEncodingExceptions.FormatValueOutOfRange("Amiga MFM", nameof(request.Cylinder), request.Cylinder, AmigaMfmFormat.MaximumCylinder);
        if (request.Head is < 0 or > AmigaMfmFormat.MaximumHead) throw TrackEncodingExceptions.FormatValueOutOfRange("Amiga MFM", nameof(request.Head), request.Head, AmigaMfmFormat.MaximumHead);
        if (request.Sectors.Count > AmigaMfmFormat.MaximumRemainingSectorCount) throw TrackEncodingExceptions.FormatValueOutOfRange("Amiga MFM", nameof(request.Sectors), request.Sectors.Count, AmigaMfmFormat.MaximumRemainingSectorCount);
    }

    /// <summary>Valide le numéro de secteur stocké sur un octet.</summary>
    private static void ValidateSector(int sectorNumber)
    {
        if (sectorNumber is < 0 or > AmigaMfmFormat.MaximumSectorNumber) throw TrackEncodingExceptions.FormatValueOutOfRange("Amiga MFM", nameof(sectorNumber), sectorNumber, AmigaMfmFormat.MaximumSectorNumber);
    }

    /// <summary>Construit les quatre octets d'information, le label, les parités et les données odd/even d'un secteur.</summary>
    private static byte[] BuildSector(int cylinder, int head, int sectorNumber, int remainingSectorCount, IReadOnlyList<byte> payload)
    {
        byte[] info = [AmigaMfmFormat.FormatByte, AmigaMfmFormat.PackTrack(cylinder, head), (byte)sectorNumber, (byte)remainingSectorCount];
        var headerAndLabel = AmigaMfmCodec.EncodeOddEven(info).Concat(new byte[AmigaMfmFormat.LabelByteCount]).ToArray();
        var headerParity = AmigaMfmCodec.CalculateParity(headerAndLabel, 0, headerAndLabel.Length);
        var data = AmigaMfmCodec.EncodeOddEven(payload);
        var dataParity = AmigaMfmCodec.CalculateSplitParity(data, 0, data.Length);
        var encoded = new byte[AmigaMfmFormat.EncodedSectorByteCount];
        headerAndLabel.CopyTo(encoded, 0);
        encoded[AmigaMfmFormat.HeaderParityHighOffset] = headerParity.High;
        encoded[AmigaMfmFormat.HeaderParityLowOffset] = headerParity.Low;
        encoded[AmigaMfmFormat.DataParityHighOffset] = dataParity.High;
        encoded[AmigaMfmFormat.DataParityLowOffset] = dataParity.Low;
        data.CopyTo(encoded, AmigaMfmFormat.EncodedDataOffset);
        return encoded;
    }
}
