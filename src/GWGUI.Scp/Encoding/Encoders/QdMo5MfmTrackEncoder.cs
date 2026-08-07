namespace GWGUI.Scp.Encoding;

public sealed class QdMo5MfmTrackEncoder : TrackEncoderBase
{
    public override string Id => "qdmo5.mfm";
    public override string DisplayName => "QD MO5 MFM";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 128) throw new ArgumentException("QD MO5 sectors contain 128 bytes.");
            bits.RawHex("A914A914A914A914A9144491");
            bits.Mfm(new byte[] { (byte)(sector.Number >> 8),(byte)sector.Number }.Concat(new byte[13]));
            bits.Gap(160);
            bits.RawHex("A914A914A914A914A9149144");
            var prefix = (byte)Attribute(sector, "prefix", 0x5a);
            var checksum = (byte)(prefix + sector.Data.Sum(value => value));
            bits.Mfm(sector.Data.Append(checksum));
            bits.Gap(128);
        }
        return bits;
    }
}
