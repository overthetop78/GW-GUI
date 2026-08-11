namespace GWGUI.MediaEngine.Encoding;

public sealed class AppleIIGcrTrackEncoder : TrackEncoderBase
{
    private static readonly byte[] SixAndTwo = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
    private static readonly byte[] FiveAndThree = [0xab,0xad,0xae,0xaf,0xb5,0xb6,0xb7,0xba,0xbb,0xbd,0xbe,0xbf,0xd6,0xd7,0xda,0xdb,0xdd,0xde,0xdf,0xea,0xeb,0xed,0xee,0xef,0xf5,0xf6,0xf7,0xfa,0xfb,0xfd,0xfe,0xff];
    public override string Id => FluxCodecIds.AppleIIGcr;
    public override string DisplayName => "Apple II GCR";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits(); var volume = (byte)Attribute(request, "volume", 254);
        var useFiveAndThree = Attribute(request, "sectorsPerTrack", request.Sectors.Count) == 13;
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 256) throw new ArgumentException("Apple II sectors contain 256 bytes.");
            bits.Gap(100, true); bits.Raw(0xd5,0xaa,useFiveAndThree ? (byte)0xb5 : (byte)0x96);
            foreach (var value in new[] { volume,(byte)request.Cylinder,(byte)sector.Number,(byte)(volume ^ request.Cylinder ^ sector.Number) }) bits.Raw((byte)((value >> 1) | 0xaa),(byte)(value | 0xaa));
            bits.Raw(0xde,0xaa,0xeb,0xff,0xff,0xff,0xd5,0xaa,0xad);
            bits.Raw(useFiveAndThree ? EncodeFiveAndThree(sector.Data) : EncodeSixAndTwo(sector.Data)); bits.Raw(0xde,0xaa,0xeb); bits.Gap(32);
        }
        return bits;
    }
    private static byte[] EncodeSixAndTwo(IReadOnlyList<byte> source)
    {
        var buffer = new byte[300]; for (var i=0;i<source.Count;i++) buffer[i]=source[i];
        var encoded = new List<byte>(343); byte checksum = 0;
        for (var index = 0; index < 86; index++)
        {
            var value = (byte)(((buffer[index]&1)<<1)|((buffer[index]&2)>>1)|((buffer[index+86]&1)<<3)|((buffer[index+86]&2)<<1)|((buffer[index+172]&1)<<5)|((buffer[index+172]&2)<<3));
            encoded.Add(SixAndTwo[value ^ checksum]); checksum = value;
        }
        for (var index=0;index<256;index++) { var value=(byte)(source[index]>>2); encoded.Add(SixAndTwo[value^checksum]); checksum=value; }
        encoded.Add(SixAndTwo[checksum]); return encoded.ToArray();
    }

    private static byte[] EncodeFiveAndThree(IReadOnlyList<byte> source)
    {
        const int chunkSize = 51; const int threeSize = 154;
        var top = new byte[256]; var threes = new byte[threeSize]; var chunk = chunkSize - 1; var sourceOffset = 0;
        for (var index = 0; index < chunkSize * 5; index += 5)
        {
            var zero = source[sourceOffset++]; var one = source[sourceOffset++]; var two = source[sourceOffset++];
            var three = source[sourceOffset++]; var four = source[sourceOffset++];
            top[chunk] = (byte)(zero >> 3); top[chunk + chunkSize] = (byte)(one >> 3);
            top[chunk + chunkSize * 2] = (byte)(two >> 3); top[chunk + chunkSize * 3] = (byte)(three >> 3);
            top[chunk + chunkSize * 4] = (byte)(four >> 3);
            threes[chunk] = (byte)(((zero & 7) << 2) | ((three & 4) >> 1) | ((four & 4) >> 2));
            threes[chunk + chunkSize] = (byte)(((one & 7) << 2) | (three & 2) | ((four & 2) >> 1));
            threes[chunk + chunkSize * 2] = (byte)(((two & 7) << 2) | ((three & 1) << 1) | (four & 1));
            chunk--;
        }
        var last = source[sourceOffset]; top[255] = (byte)(last >> 3); threes[^1] = (byte)(last & 7);
        var encoded = new List<byte>(411); byte checksum = 0;
        for (var index = threeSize - 1; index >= 0; index--) { encoded.Add(FiveAndThree[threes[index] ^ checksum]); checksum = threes[index]; }
        for (var index = 0; index < 256; index++) { encoded.Add(FiveAndThree[top[index] ^ checksum]); checksum = top[index]; }
        encoded.Add(FiveAndThree[checksum]);
        return encoded.ToArray();
    }
}
