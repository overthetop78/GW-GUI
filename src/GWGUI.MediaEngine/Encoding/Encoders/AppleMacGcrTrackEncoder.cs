using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes Macintosh et Lisa utilisant le GCR 6-and-2 de l'IWM.</summary>
public class AppleMacGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id=>FluxCodecIds.AppleMacGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName=>FluxCodecDisplayNames.AppleMacGcr;
    /// <summary>Conserve la définition « Default Format » utilisée par ce codec.</summary>
    protected virtual byte DefaultFormat => AppleIwmGcrFormat.DefaultFormat;
    /// <summary>Encode les secteurs demandés sous forme de cellules GCR IWM.</summary>
    /// <param name="request">Piste logique contenant cylindre, face, format, secteurs et éventuels octets de tags.</param>
    /// <returns>Cellules binaires de la piste, dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille IWM attendue.</exception>
    /// <remarks>L'en-tête protège ses quatre champs par XOR sur six bits ; les tags et la charge utile sont ensuite encodés ensemble en 6-and-2.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits(); var format=(byte)Attribute(request,AppleIwmGcrFormat.FormatAttributeName,DefaultFormat);
        foreach(var sector in request.Sectors)
        {
            if(sector.Data.Count!=AppleIwmGcrFormat.SectorByteCount) throw AppleIwmGcrFormat.InvalidSectorSize(sector.Data.Count);
            byte[] header=[(byte)(request.Cylinder&AppleIwmGcrFormat.SixBitMask),(byte)sector.Number,(byte)((request.Cylinder>>AppleIwmGcrFormat.CylinderHighBitShift&AppleIwmGcrFormat.CylinderHighBitMask)|(request.Head<<AppleIwmGcrFormat.HeadBitShift)),format];
            var checksum=(byte)(header.Aggregate(0,(value,item)=>value^item)&AppleIwmGcrFormat.SixBitMask);
            bits.Gap(AppleIwmGcrFormat.AddressLeadingGapBitCount,true); bits.Raw(AppleIwmGcrFormat.AddressMark.ToArray()); bits.Raw(header.Append(checksum).Select(value=>AppleIwmGcrFormat.SixAndTwoTable[value]).ToArray()); bits.Raw(AppleIwmGcrFormat.EpilogueFirstByte,AppleIwmGcrFormat.EpilogueSecondByte,AppleIwmGcrFormat.SyncByte,AppleIwmGcrFormat.SyncByte); bits.Gap(AppleIwmGcrFormat.AddressTrailingGapBitCount,true);
            bits.Raw(AppleIwmGcrFormat.DataMark.ToArray());
            var tags=Enumerable.Range(0,AppleIwmGcrFormat.TagByteCount).Select(index=>(byte)Attribute(sector,$"{AppleIwmGcrFormat.TagAttributePrefix}{index}",0)).ToArray();
            bits.Raw(AppleIwmGcrFormat.SixAndTwoTable[sector.Number & AppleIwmGcrFormat.SixBitMask]); bits.Raw(AppleIwmGcrCodec.Encode(tags.Concat(sector.Data).ToArray())); bits.Raw(AppleIwmGcrFormat.EpilogueFirstByte,AppleIwmGcrFormat.EpilogueSecondByte,AppleIwmGcrFormat.SyncByte); bits.Gap(AppleIwmGcrFormat.DataTrailingGapBitCount,true);
        }
        return bits;
    }
}
