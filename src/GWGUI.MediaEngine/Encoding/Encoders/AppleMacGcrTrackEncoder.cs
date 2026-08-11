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
            bits.Raw(AppleIwmGcrFormat.SixAndTwoTable[sector.Number & AppleIwmGcrFormat.SixBitMask]); bits.Raw(EncodeData(tags.Concat(sector.Data).ToArray())); bits.Raw(AppleIwmGcrFormat.EpilogueFirstByte,AppleIwmGcrFormat.EpilogueSecondByte,AppleIwmGcrFormat.SyncByte); bits.Gap(AppleIwmGcrFormat.DataTrailingGapBitCount,true);
        }
        return bits;
    }
    /// <summary>Exécute le traitement « Encode Data » propre à ce format.</summary>
    private static byte[] EncodeData(byte[] source)
    {
        var b1=new byte[AppleIwmGcrFormat.GroupByteCount]; var b2=new byte[AppleIwmGcrFormat.GroupByteCount]; var b3=new byte[AppleIwmGcrFormat.GroupByteCount]; uint c1=0,c2=0,c3=0; var position=0;
        for(var index=0;;index++)
        {
            c1=(c1&AppleIwmGcrFormat.ChecksumByteMask)<<1; if((c1&AppleIwmGcrFormat.ChecksumCarryBit)!=0)c1++;
            var value=source[position++]; b1[index]=(byte)(value^c1); c3+=value; if((c1&AppleIwmGcrFormat.ChecksumCarryBit)!=0){c3++;c1&=AppleIwmGcrFormat.ChecksumByteMask;}
            value=source[position++]; b2[index]=(byte)(value^c3); c2+=value; if(c3>AppleIwmGcrFormat.ChecksumByteMask){c2++;c3&=AppleIwmGcrFormat.ChecksumByteMask;}
            if(position==source.Length)break;
            value=source[position++]; b3[index]=(byte)(value^c2); c1+=value; if(c2>AppleIwmGcrFormat.ChecksumByteMask){c1++;c2&=AppleIwmGcrFormat.ChecksumByteMask;}
        }
        var symbols=new List<byte>(AppleIwmGcrFormat.EncodedPayloadSymbolCount+AppleIwmGcrFormat.ChecksumSymbolCount);
        for(var index=0;index<=AppleIwmGcrFormat.LastGroupIndex;index++)
        {
            symbols.Add((byte)(((b1[index]>>AppleIwmGcrFormat.ThirdChecksumShift)&AppleIwmGcrFormat.FirstPackedChecksumMask)|((b2[index]>>AppleIwmGcrFormat.SecondChecksumShift)&AppleIwmGcrFormat.SecondPackedChecksumMask)|((b3[index]>>AppleIwmGcrFormat.FirstChecksumShift)&AppleIwmGcrFormat.ThirdPackedChecksumMask)));
            symbols.Add((byte)(b1[index]&AppleIwmGcrFormat.SixBitMask)); symbols.Add((byte)(b2[index]&AppleIwmGcrFormat.SixBitMask)); if(index!=AppleIwmGcrFormat.LastGroupIndex)symbols.Add((byte)(b3[index]&AppleIwmGcrFormat.SixBitMask));
        }
        symbols.Add((byte)(((c1&AppleIwmGcrFormat.ChecksumHighBitsMask)>>AppleIwmGcrFormat.FirstChecksumShift)|((c2&AppleIwmGcrFormat.ChecksumHighBitsMask)>>AppleIwmGcrFormat.SecondChecksumShift)|((c3&AppleIwmGcrFormat.ChecksumHighBitsMask)>>AppleIwmGcrFormat.ThirdChecksumShift)));
        symbols.Add((byte)(c3&AppleIwmGcrFormat.SixBitMask)); symbols.Add((byte)(c2&AppleIwmGcrFormat.SixBitMask)); symbols.Add((byte)(c1&AppleIwmGcrFormat.SixBitMask));
        return symbols.Select(value=>AppleIwmGcrFormat.SixAndTwoTable[value]).ToArray();
    }
}
