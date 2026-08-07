namespace GWGUI.Scp.Encoding;

public sealed class DecRx02TrackEncoder : TrackEncoderBase
{
    public override string Id => "dec.rx02";
    public override string DisplayName => "DEC RX02 M²FM";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits();
        foreach(var sector in request.Sectors)
        {
            var m2fm=sector.Data.Count==256; if(!m2fm&&sector.Data.Count!=128) throw new ArgumentException("DEC RX sectors contain 128 or 256 bytes.");
            var sizeCode=sector.SizeCode??TrackEncoding.SizeCode(sector.Data.Count);
            var headerCrc=TrackEncoding.Crc16([0xfe,(byte)request.Cylinder,(byte)request.Head,(byte)sector.Number,sizeCode]);
            bits.RawHex("55111554"); bits.DoubleFm([(byte)request.Cylinder,(byte)request.Head,(byte)sector.Number,sizeCode,(byte)(headerCrc>>8),(byte)headerCrc]); bits.Gap(64,true);
            var mark=m2fm?(sector.Deleted?(byte)0xfd:(byte)0xf9):(sector.Deleted?(byte)0xf8:(byte)0xfb);
            bits.RawHex(mark switch {0xf8=>"55111444",0xf9=>"55111445",0xfb=>"55111455",0xfd=>"55111545",_=>"55111455"});
            var crc=TrackEncoding.Crc16(new[]{mark}.Concat(sector.Data)); var payload=sector.Data.Concat([(byte)(crc>>8),(byte)crc]).ToArray();
            if(m2fm) { bits.Add(false); var encoded=TrackEncoding.Bits(); encoded.Mfm(payload); ReplaceM2Fm(encoded); bits.AddRange(encoded); }
            else bits.DoubleFm(payload);
            bits.Gap(64,true);
        }
        return bits;
    }
    private static void ReplaceM2Fm(List<bool> bits)
    {
        bool[] normal=[false,false,true,false,true,false,true,false,true,false,false];
        bool[] encoded=[false,true,false,false,false,true,false,false,false,true,false];
        for(var offset=1;offset+normal.Length<=bits.Count;offset+=2)
        {
            var match=true; for(var i=0;i<normal.Length;i++) if(bits[offset+i]!=normal[i]) { match=false; break; }
            if(!match) continue; for(var i=0;i<normal.Length;i++) bits[offset+i]=encoded[i]; offset+=normal.Length-3;
        }
    }
}
