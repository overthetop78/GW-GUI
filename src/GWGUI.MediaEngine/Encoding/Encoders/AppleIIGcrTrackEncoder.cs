using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Apple IIGCR.</summary>
public sealed class AppleIIGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Distingue les deux encodages sectoriels pris en charge par les pistes Apple II.</summary>
    private enum TrackFormat { FiveAndThree, SixAndTwo }

    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleIIGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleIIGcr;
    /// <summary>Encode les secteurs demandés en GCR Apple II 5-and-3 ou 6-and-2.</summary>
    /// <param name="request">Piste logique contenant le volume éventuel, le cylindre et les secteurs de 256 octets.</param>
    /// <returns>Cellules binaires de la piste, dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne contient pas exactement 256 octets.</exception>
    /// <remarks>Une piste complète sélectionne automatiquement le 5-and-3 à treize secteurs ou le 6-and-2 à seize secteurs. Une piste partielle doit fournir explicitement son format.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        var volumeValue = Attribute(request, AppleIIGcrFormat.VolumeAttributeName, AppleIIGcrFormat.DefaultVolume);
        ValidateAddressValue(nameof(volumeValue), volumeValue);
        ValidateAddressValue(nameof(request.Cylinder), request.Cylinder);
        var volume = (byte)volumeValue;
        var format = ResolveFormat(request);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != AppleIIGcrFormat.SectorSize) throw AppleIIGcrFormat.InvalidSectorSize(sector.Data.Count);
            ValidateAddressValue(nameof(sector.Number), sector.Number);
            bits.Gap(AppleIIGcrFormat.LeadingGapBitCount, true);
            bits.Raw((format == TrackFormat.FiveAndThree ? AppleIIGcrFormat.FiveAndThreeAddressPrologueBytes : AppleIIGcrFormat.SixAndTwoAddressPrologueBytes).ToArray());
            bits.Raw(AppleIIGcrCodec.EncodeAddress(volume, (byte)request.Cylinder, (byte)sector.Number));
            bits.Raw(AppleIIGcrFormat.AddressToDataSeparatorBytes.ToArray());
            bits.Raw(format == TrackFormat.FiveAndThree ? AppleIIGcrCodec.EncodeFiveAndThree(sector.Data) : AppleIIGcrCodec.EncodeSixAndTwo(sector.Data));
            bits.Raw(AppleIIGcrFormat.EpilogueBytes.ToArray());
            bits.Gap(AppleIIGcrFormat.TrailingGapBitCount);
        }
        return bits;
    }

    /// <summary>Sélectionne explicitement le format 5-and-3 ou 6-and-2 de la piste.</summary>
    private static TrackFormat ResolveFormat(TrackEncodeRequest request)
    {
        var sectorCount = request.Sectors.Count;
        var explicitSectorCount = 0;
        var hasExplicitFormat = request.Attributes is not null && request.Attributes.TryGetValue(AppleIIGcrFormat.SectorsPerTrackAttributeName, out explicitSectorCount);
        if (hasExplicitFormat) sectorCount = explicitSectorCount;
        var format = sectorCount switch { AppleIIGcrFormat.FiveAndThreeSectorsPerTrack => TrackFormat.FiveAndThree, AppleIIGcrFormat.SixAndTwoSectorsPerTrack => TrackFormat.SixAndTwo, _ => throw AppleIIGcrFormat.InvalidSectorsPerTrack(sectorCount) };
        if (hasExplicitFormat && request.Sectors.Count is AppleIIGcrFormat.FiveAndThreeSectorsPerTrack or AppleIIGcrFormat.SixAndTwoSectorsPerTrack && request.Sectors.Count != sectorCount) throw AppleIIGcrFormat.InvalidSectorsPerTrack(request.Sectors.Count);
        return format;
    }

    /// <summary>Valide une valeur d'adresse avant sa conversion sur un octet.</summary>
    private static void ValidateAddressValue(string field, int value)
    {
        if (value is < 0 or > AppleIIGcrFormat.MaximumAddressValue) throw TrackEncodingExceptions.FormatValueOutOfRange("Apple II GCR", field, value, AppleIIGcrFormat.MaximumAddressValue);
    }
}
