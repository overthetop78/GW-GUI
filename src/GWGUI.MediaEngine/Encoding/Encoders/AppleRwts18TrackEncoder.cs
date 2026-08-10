namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encodes the standard Brøderbund RWTS18 six-sector track layout.</summary>
public sealed class AppleRwts18TrackEncoder : TrackEncoderBase
{
    private static readonly byte[] Nibbles =
    [
        0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,
        0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,
        0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,
        0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff
    ];

    public override string Id => "apple2.rwts18";
    public override string DisplayName => "Apple II Brøderbund RWTS18";

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors.OrderByDescending(sector => sector.Number))
        {
            if (sector.Number is < 0 or >= 6 || sector.Data.Count != 768)
                throw new ArgumentException("RWTS18 tracks contain six sectors of 768 bytes.");
            bits.Gap(sector.Number == 5 ? 200 : 4, true);
            bits.Raw(0xd5, 0x9d, Nibbles[request.Cylinder & 0x3f], Nibbles[sector.Number],
                Nibbles[(request.Cylinder ^ sector.Number) & 0x3f], 0xaa, 0xff, 0xff);
            bits.Raw((byte)Attribute(request, "id", 0xa4));
            bits.Raw(EncodePayload(sector.Data));
            bits.Raw(0xd4, 0xff);
        }
        return bits;
    }

    private static byte[] EncodePayload(IReadOnlyList<byte> data)
    {
        var encoded = new byte[1_025]; byte checksum = 0;
        for (var index = 0; index < 256; index++)
        {
            var one = data[index]; var two = data[256 + index]; var three = data[512 + index];
            var high = (byte)(((one >> 6) << 4) | ((two >> 6) << 2) | (three >> 6));
            var values = new[] { high, (byte)(one & 0x3f), (byte)(two & 0x3f), (byte)(three & 0x3f) };
            for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                var value = values[valueIndex];
                checksum ^= value;
                encoded[index * 4 + valueIndex] = Nibbles[value];
            }
        }
        encoded[1_024] = Nibbles[checksum & 0x3f];
        return encoded;
    }
}
