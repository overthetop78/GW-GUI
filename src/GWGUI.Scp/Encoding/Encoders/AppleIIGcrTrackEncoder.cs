namespace GWGUI.Scp.Encoding;

public sealed class AppleIIGcrTrackEncoder : TrackEncoderBase
{
    private static readonly byte[] Table = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
    public override string Id => "apple2.gcr";
    public override string DisplayName => "Apple II GCR";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits(); var volume = (byte)Attribute(request, "volume", 254);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 256) throw new ArgumentException("Apple II sectors contain 256 bytes.");
            bits.Gap(100, true); bits.Raw(0xd5,0xaa,0x96);
            foreach (var value in new[] { volume,(byte)request.Cylinder,(byte)sector.Number,(byte)(volume ^ request.Cylinder ^ sector.Number) }) bits.Raw((byte)((value >> 1) | 0xaa),(byte)(value | 0xaa));
            bits.Raw(0xde,0xaa,0xeb,0xff,0xff,0xff,0xd5,0xaa,0xad);
            bits.Raw(EncodeData(sector.Data)); bits.Raw(0xde,0xaa,0xeb); bits.Gap(32);
        }
        return bits;
    }
    private static byte[] EncodeData(IReadOnlyList<byte> source)
    {
        var buffer = new byte[300]; for (var i=0;i<source.Count;i++) buffer[i]=source[i];
        var encoded = new List<byte>(343); byte checksum = 0;
        for (var index = 0; index < 86; index++)
        {
            var value = (byte)(((buffer[index]&1)<<1)|((buffer[index]&2)>>1)|((buffer[index+86]&1)<<3)|((buffer[index+86]&2)<<1)|((buffer[index+172]&1)<<5)|((buffer[index+172]&2)<<3));
            encoded.Add(Table[value ^ checksum]); checksum = value;
        }
        for (var index=0;index<256;index++) { var value=(byte)(source[index]>>2); encoded.Add(Table[value^checksum]); checksum=value; }
        encoded.Add(Table[checksum]); return encoded.ToArray();
    }
}
