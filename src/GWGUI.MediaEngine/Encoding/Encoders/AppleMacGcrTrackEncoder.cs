using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes Macintosh et Lisa utilisant le GCR 6-and-2 de l'IWM.</summary>
public class AppleMacGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleMacGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleMacGcr;
    /// <summary>Obtient l'octet de format IWM utilisé lorsqu'aucun attribut ne le remplace.</summary>
    protected virtual byte DefaultFormat => AppleIwmGcrFormat.DefaultFormat;
    /// <summary>Encode les secteurs demandés sous forme de cellules GCR IWM.</summary>
    /// <param name="request">Piste logique contenant cylindre, face, format, secteurs et éventuels octets de tags.</param>
    /// <returns>Cellules binaires de la piste, dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille IWM attendue.</exception>
    /// <remarks>L'en-tête protège ses quatre champs par XOR sur six bits ; les tags et la charge utile sont ensuite encodés ensemble en 6-and-2.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        ValidateValue(nameof(request.Cylinder), request.Cylinder, AppleIwmGcrFormat.MaximumCylinder);
        ValidateValue(nameof(request.Head), request.Head, AppleIwmGcrFormat.MaximumHead);
        var formatValue = Attribute(request, AppleIwmGcrFormat.FormatAttributeName, DefaultFormat);
        ValidateValue(nameof(formatValue), formatValue, AppleIwmGcrFormat.MaximumSixBitValue);
        var format = (byte)formatValue;
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != AppleIwmGcrFormat.SectorByteCount) throw AppleIwmGcrFormat.InvalidSectorSize(sector.Data.Count);
            ValidateValue(nameof(sector.Number), sector.Number, AppleIwmGcrFormat.MaximumSixBitValue);
            bits.Gap(AppleIwmGcrFormat.AddressLeadingGapBitCount, true);
            bits.Raw(BuildAddressField(request, sector, format));
            bits.Gap(AppleIwmGcrFormat.AddressTrailingGapBitCount, true);
            bits.Raw(BuildDataField(sector, ReadTags(sector)));
            bits.Gap(AppleIwmGcrFormat.DataTrailingGapBitCount, true);
        }
        return bits;
    }

    /// <summary>Construit le champ d'adresse IWM avec son checksum XOR sur six bits.</summary>
    private static byte[] BuildAddressField(TrackEncodeRequest request, TrackSector sector, byte format)
    {
        byte[] header = [(byte)(request.Cylinder & AppleIwmGcrFormat.SixBitMask), (byte)sector.Number, (byte)(((request.Cylinder >> AppleIwmGcrFormat.CylinderHighBitShift) & AppleIwmGcrFormat.CylinderHighBitMask) | (request.Head << AppleIwmGcrFormat.HeadBitShift)), format];
        var checksum = (byte)(header.Aggregate(0, (value, item) => value ^ item) & AppleIwmGcrFormat.SixBitMask);
        return AppleIwmGcrFormat.AddressMark.Concat(header.Append(checksum).Select(value => AppleIwmGcrFormat.SixAndTwoTable[value])).Concat(AppleIwmGcrFormat.AddressEpilogue).ToArray();
    }

    /// <summary>Lit et valide les douze octets de tags portés par un secteur.</summary>
    private static byte[] ReadTags(TrackSector sector)
    {
        var tags = new byte[AppleIwmGcrFormat.TagByteCount];
        for (var index = 0; index < tags.Length; index++)
        {
            var value = Attribute(sector, AppleIwmGcrFormat.TagAttributeName(index), 0);
            ValidateValue(AppleIwmGcrFormat.TagAttributeName(index), value, AppleIwmGcrFormat.MaximumTagValue);
            tags[index] = (byte)value;
        }
        return tags;
    }

    /// <summary>Construit le champ de données IWM depuis les tags et les 512 octets sectoriels.</summary>
    private static byte[] BuildDataField(TrackSector sector, IReadOnlyList<byte> tags) => AppleIwmGcrFormat.DataMark.Concat([AppleIwmGcrFormat.SixAndTwoTable[sector.Number]]).Concat(AppleIwmGcrCodec.Encode(tags.Concat(sector.Data).ToArray())).Concat(AppleIwmGcrFormat.DataEpilogue).ToArray();

    /// <summary>Valide une valeur entière avant son emploi dans une adresse ou un attribut IWM.</summary>
    private static void ValidateValue(string field, int value, int maximum)
    {
        if (value is < 0 || value > maximum) throw TrackEncodingExceptions.FormatValueOutOfRange("Apple IWM GCR", field, value, maximum);
    }
}
