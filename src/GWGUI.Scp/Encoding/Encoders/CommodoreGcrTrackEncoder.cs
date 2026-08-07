namespace GWGUI.Scp.Encoding;

public sealed class CommodoreGcrTrackEncoder : TrackEncoderBase
{
    private static readonly int[] Table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
    public override string Id => "commodore.gcr";
    public override string DisplayName => "Commodore GCR";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits(); var id2=(byte)Attribute(request,"id2",0xa1); var id1=(byte)Attribute(request,"id1",0x1a);
        foreach(var sector in request.Sectors)
        {
            if(sector.Data.Count!=256) throw new ArgumentException("Commodore sectors contain 256 bytes.");
            byte[] header=[0x08,(byte)(sector.Number^request.Cylinder^id2^id1),(byte)sector.Number,(byte)request.Cylinder,id2,id1];
            byte checksum=0; foreach(var value in sector.Data) checksum^=value;
            bits.Gap(100,true); bits.RawBits("000"); bits.Gap(20,true); Gcr(bits,header); bits.Gap(6); bits.Gap(20,true);
            Gcr(bits,new byte[]{0x07}.Concat(sector.Data).Append(checksum)); bits.Gap(32);
        }
        return bits;
    }
    private static void Gcr(List<bool> bits,IEnumerable<byte> values)
    {
        foreach(var value in values) foreach(var nibble in new[]{value>>4,value&15}) for(var bit=4;bit>=0;bit--) bits.Add((Table[nibble]&(1<<bit))!=0);
    }
}
