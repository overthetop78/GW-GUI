using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public class AppleMacGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id=>FluxCodecIds.AppleMacGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName=>FluxCodecDisplayNames.AppleMacGcr;
    /// <summary>Conserve la définition « Default Format » utilisée par ce codec.</summary>
    protected virtual byte DefaultFormat => AppleIwmGcrFormat.DefaultFormat;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
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
