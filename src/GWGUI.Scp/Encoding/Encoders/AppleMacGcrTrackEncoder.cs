namespace GWGUI.Scp.Encoding;

public sealed class AppleMacGcrTrackEncoder : TrackEncoderBase
{
    private static readonly byte[] Table=[0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
    public override string Id=>"applemac.gcr";
    public override string DisplayName=>"Apple Macintosh GCR";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits(); var format=(byte)Attribute(request,"format",0x12);
        foreach(var sector in request.Sectors)
        {
            if(sector.Data.Count!=512) throw new ArgumentException("Apple Macintosh sectors contain 512 bytes.");
            byte[] header=[(byte)(request.Cylinder&0x3f),(byte)sector.Number,(byte)((request.Cylinder>>6&3)|(request.Head<<5)),format];
            var checksum=(byte)(header.Aggregate(0,(value,item)=>value^item)&0x3f);
            bits.Gap(100,true); bits.DoubledCells([0xd5,0xaa,0x96]); bits.DoubledCells(header.Append(checksum).Select(value=>Table[value])); bits.Gap(32);
            bits.DoubledCells([0xd5,0xaa,0xad]);
            var tags=Enumerable.Range(0,12).Select(index=>(byte)Attribute(sector,$"tag{index}",0)).ToArray();
            bits.DoubledCells(EncodeData(tags.Concat(sector.Data).ToArray())); bits.Gap(64);
        }
        return bits;
    }
    private static byte[] EncodeData(byte[] source)
    {
        var b1=new byte[175]; var b2=new byte[175]; var b3=new byte[175]; uint c1=0,c2=0,c3=0; var position=0;
        for(var index=0;;index++)
        {
            c1=(c1&0xff)<<1; if((c1&0x100)!=0)c1++;
            var value=source[position++]; b1[index]=(byte)(value^c1); c3+=value; if((c1&0x100)!=0){c3++;c1&=0xff;}
            value=source[position++]; b2[index]=(byte)(value^c3); c2+=value; if(c3>0xff){c2++;c3&=0xff;}
            if(position==source.Length)break;
            value=source[position++]; b3[index]=(byte)(value^c2); c1+=value; if(c2>0xff){c1++;c2&=0xff;}
        }
        var symbols=new List<byte>(704){0};
        for(var index=0;index<=174;index++)
        {
            symbols.Add((byte)(((b1[index]>>2)&48)|((b2[index]>>4)&12)|((b3[index]>>6)&3)));
            symbols.Add((byte)(b1[index]&0x3f)); symbols.Add((byte)(b2[index]&0x3f)); if(index!=174)symbols.Add((byte)(b3[index]&0x3f));
        }
        symbols.Add((byte)(((c1&0xc0)>>6)|((c2&0xc0)>>4)|((c3&0xc0)>>2)));
        symbols.Add((byte)(c3&0x3f)); symbols.Add((byte)(c2&0x3f)); symbols.Add((byte)(c1&0x3f));
        return symbols.Select(value=>Table[value]).ToArray();
    }
}
