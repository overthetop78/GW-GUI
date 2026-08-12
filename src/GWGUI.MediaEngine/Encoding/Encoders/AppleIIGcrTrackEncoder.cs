using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Apple IIGCR.</summary>
public sealed class AppleIIGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleIIGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleIIGcr;
    /// <summary>Encode les secteurs demandés en GCR Apple II 5-and-3 ou 6-and-2.</summary>
    /// <param name="request">Piste logique contenant le volume éventuel, le cylindre et les secteurs de 256 octets.</param>
    /// <returns>Cellules binaires de la piste, dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne contient pas exactement 256 octets.</exception>
    /// <remarks>Le nombre de secteurs demandé sélectionne le parcours 5-and-3 à treize secteurs ; les autres pistes utilisent le parcours 6-and-2.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits(); var volume = (byte)Attribute(request, AppleIIGcrFormat.VolumeAttributeName, AppleIIGcrFormat.DefaultVolume);
        var useFiveAndThree = Attribute(request, AppleIIGcrFormat.SectorsPerTrackAttributeName, request.Sectors.Count) == AppleIIGcrFormat.FiveAndThreeSectorsPerTrack;
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != AppleIIGcrFormat.SectorSize) throw AppleIIGcrFormat.InvalidSectorSize(sector.Data.Count);
            bits.Gap(AppleIIGcrFormat.LeadingGapBitCount, true); bits.Raw(AppleIIGcrFormat.PrologueFirstByte, AppleIIGcrFormat.PrologueSecondByte, useFiveAndThree ? AppleIIGcrFormat.FiveAndThreeAddressPrologueLastByte : AppleIIGcrFormat.SixAndTwoAddressPrologueLastByte);
            foreach (var value in new[] { volume,(byte)request.Cylinder,(byte)sector.Number,(byte)(volume ^ request.Cylinder ^ sector.Number) }) { var encoded = AppleIIGcrCodec.EncodeFourAndFour(value); bits.Raw(encoded.High, encoded.Low); }
            bits.Raw(AppleIIGcrFormat.EpilogueFirstByte, AppleIIGcrFormat.EpilogueSecondByte, AppleIIGcrFormat.EpilogueLastByte, AppleIIGcrFormat.SyncByte, AppleIIGcrFormat.SyncByte, AppleIIGcrFormat.SyncByte, AppleIIGcrFormat.PrologueFirstByte, AppleIIGcrFormat.PrologueSecondByte, AppleIIGcrFormat.DataPrologueLastByte);
            bits.Raw(useFiveAndThree ? AppleIIGcrCodec.EncodeFiveAndThree(sector.Data) : AppleIIGcrCodec.EncodeSixAndTwo(sector.Data)); bits.Raw(AppleIIGcrFormat.EpilogueFirstByte, AppleIIGcrFormat.EpilogueSecondByte, AppleIIGcrFormat.EpilogueLastByte); bits.Gap(AppleIIGcrFormat.TrailingGapBitCount);
        }
        return bits;
    }
}
