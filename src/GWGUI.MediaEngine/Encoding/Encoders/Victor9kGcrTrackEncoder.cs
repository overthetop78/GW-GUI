namespace GWGUI.MediaEngine.Encoding;

public sealed class Victor9kGcrTrackEncoder : TrackEncoderBase
{
    private static readonly int[] Table=[0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
    public override string Id=>FluxCodecIds.Victor9kGcr;
    public override string DisplayName=>"Victor 9000 GCR";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits();
        foreach(var sector in request.Sectors)
        {
            if(sector.Data.Count!=512) throw new ArgumentException("Victor 9000 sectors contain 512 bytes.");
            byte[] header=[0x06,(byte)request.Cylinder,(byte)sector.Number,(byte)(request.Cylinder+sector.Number),0xa1,0x1a];
            ushort checksum=0; foreach(var value in sector.Data) checksum+=value;
            AddBlock(bits,"5555555555551111",header); bits.Gap(20);
            AddBlock(bits,"5555555555551104",new byte[]{0}.Concat(sector.Data).Concat([(byte)checksum,(byte)(checksum>>8)])); bits.Gap(64);
        }
        return bits;
    }
    private static void AddBlock(List<bool> target,string markerHex,IEnumerable<byte> values)
    {
        var marker=new List<bool>(); marker.RawHex(markerHex); var encoded=new List<bool>();
        foreach(var value in values) foreach(var nibble in new[]{value>>4,value&15}) for(var bit=4;bit>=0;bit--) encoded.Add((Table[nibble]&(1<<bit))!=0);
        while(marker.Count<49+encoded.Count*2) marker.Add(false);
        for(var index=0;index<encoded.Count;index++) marker[49+index*2]=encoded[index];
        target.AddRange(marker);
    }
}
