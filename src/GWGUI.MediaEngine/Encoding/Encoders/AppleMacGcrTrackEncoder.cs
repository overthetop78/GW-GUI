using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public class AppleMacGcrTrackEncoder : TrackEncoderBase
{
    public override string Id=>FluxCodecIds.AppleMacGcr;
    public override string DisplayName=>FluxCodecDisplayNames.AppleMacGcr;
    protected virtual byte DefaultFormat => AppleMacGcrFormat.DefaultFormat;
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits(); var format=(byte)Attribute(request,AppleMacGcrFormat.FormatAttributeName,DefaultFormat);
        foreach(var sector in request.Sectors)
        {
            if(sector.Data.Count!=AppleMacGcrFormat.SectorByteCount) throw AppleMacGcrFormat.InvalidSectorSize(sector.Data.Count);
            byte[] header=[(byte)(request.Cylinder&AppleMacGcrFormat.SixBitMask),(byte)sector.Number,(byte)((request.Cylinder>>AppleMacGcrFormat.CylinderHighBitShift&AppleMacGcrFormat.CylinderHighBitMask)|(request.Head<<AppleMacGcrFormat.HeadBitShift)),format];
            var checksum=(byte)(header.Aggregate(0,(value,item)=>value^item)&AppleMacGcrFormat.SixBitMask);
            bits.Gap(AppleMacGcrFormat.AddressLeadingGapBitCount,true); bits.Raw(AppleMacGcrFormat.AddressMark.ToArray()); bits.Raw(header.Append(checksum).Select(value=>AppleMacGcrFormat.SixAndTwoTable[value]).ToArray()); bits.Raw(AppleMacGcrFormat.EpilogueFirstByte,AppleMacGcrFormat.EpilogueSecondByte,AppleMacGcrFormat.SyncByte,AppleMacGcrFormat.SyncByte); bits.Gap(AppleMacGcrFormat.AddressTrailingGapBitCount,true);
            bits.Raw(AppleMacGcrFormat.DataMark.ToArray());
            var tags=Enumerable.Range(0,AppleMacGcrFormat.TagByteCount).Select(index=>(byte)Attribute(sector,$"{AppleMacGcrFormat.TagAttributePrefix}{index}",0)).ToArray();
            bits.Raw(AppleMacGcrFormat.SixAndTwoTable[sector.Number & AppleMacGcrFormat.SixBitMask]); bits.Raw(EncodeData(tags.Concat(sector.Data).ToArray())); bits.Raw(AppleMacGcrFormat.EpilogueFirstByte,AppleMacGcrFormat.EpilogueSecondByte,AppleMacGcrFormat.SyncByte); bits.Gap(AppleMacGcrFormat.DataTrailingGapBitCount,true);
        }
        return bits;
    }
    private static byte[] EncodeData(byte[] source)
    {
        var b1=new byte[AppleMacGcrFormat.GroupByteCount]; var b2=new byte[AppleMacGcrFormat.GroupByteCount]; var b3=new byte[AppleMacGcrFormat.GroupByteCount]; uint c1=0,c2=0,c3=0; var position=0;
        for(var index=0;;index++)
        {
            c1=(c1&AppleMacGcrFormat.ChecksumByteMask)<<1; if((c1&AppleMacGcrFormat.ChecksumCarryBit)!=0)c1++;
            var value=source[position++]; b1[index]=(byte)(value^c1); c3+=value; if((c1&AppleMacGcrFormat.ChecksumCarryBit)!=0){c3++;c1&=AppleMacGcrFormat.ChecksumByteMask;}
            value=source[position++]; b2[index]=(byte)(value^c3); c2+=value; if(c3>AppleMacGcrFormat.ChecksumByteMask){c2++;c3&=AppleMacGcrFormat.ChecksumByteMask;}
            if(position==source.Length)break;
            value=source[position++]; b3[index]=(byte)(value^c2); c1+=value; if(c2>AppleMacGcrFormat.ChecksumByteMask){c1++;c2&=AppleMacGcrFormat.ChecksumByteMask;}
        }
        var symbols=new List<byte>(AppleMacGcrFormat.EncodedPayloadSymbolCount+AppleMacGcrFormat.ChecksumSymbolCount);
        for(var index=0;index<=AppleMacGcrFormat.LastGroupIndex;index++)
        {
            symbols.Add((byte)(((b1[index]>>AppleMacGcrFormat.ThirdChecksumShift)&AppleMacGcrFormat.FirstPackedChecksumMask)|((b2[index]>>AppleMacGcrFormat.SecondChecksumShift)&AppleMacGcrFormat.SecondPackedChecksumMask)|((b3[index]>>AppleMacGcrFormat.FirstChecksumShift)&AppleMacGcrFormat.ThirdPackedChecksumMask)));
            symbols.Add((byte)(b1[index]&AppleMacGcrFormat.SixBitMask)); symbols.Add((byte)(b2[index]&AppleMacGcrFormat.SixBitMask)); if(index!=AppleMacGcrFormat.LastGroupIndex)symbols.Add((byte)(b3[index]&AppleMacGcrFormat.SixBitMask));
        }
        symbols.Add((byte)(((c1&AppleMacGcrFormat.ChecksumHighBitsMask)>>AppleMacGcrFormat.FirstChecksumShift)|((c2&AppleMacGcrFormat.ChecksumHighBitsMask)>>AppleMacGcrFormat.SecondChecksumShift)|((c3&AppleMacGcrFormat.ChecksumHighBitsMask)>>AppleMacGcrFormat.ThirdChecksumShift)));
        symbols.Add((byte)(c3&AppleMacGcrFormat.SixBitMask)); symbols.Add((byte)(c2&AppleMacGcrFormat.SixBitMask)); symbols.Add((byte)(c1&AppleMacGcrFormat.SixBitMask));
        return symbols.Select(value=>AppleMacGcrFormat.SixAndTwoTable[value]).ToArray();
    }
}
