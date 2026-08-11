namespace GWGUI.MediaEngine.Encoding;

public sealed class ArburgTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.Arburg;
    public override string DisplayName => "Arburg system/data";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits();
        foreach(var sector in request.Sectors)
        {
            var system=Attribute(sector,"system",0)!=0;
            var useful=system?0xefe:0x9fe; var total=system?0xf00:0xa00;
            if(sector.Data.Count!=useful&&sector.Data.Count!=total) throw new ArgumentException($"Arburg {(system?"system":"data")} payload must contain {useful} useful bytes or {total} complete bytes.");
            var data=sector.Data.Take(useful).ToArray(); ushort checksum=0; foreach(var value in data) checksum+=value;
            var block=data.Concat([(byte)checksum,(byte)(checksum>>8)]).Concat(Enumerable.Repeat((byte)0,total-useful-2));
            if(system)
            {
                bits.RawHex("5555555555249249");
                foreach(var value in block) for(var bit=0;bit<8;bit++) bits.RawBits(((value>>bit)&1)!=0?"001":"01");
            }
            else { bits.RawHex("4444444455555555"); bits.DoubleFm(block.Select(Primitives.BitPrimitives.ReverseBits)); }
            bits.Gap(64,true);
        }
        return bits;
    }
}
