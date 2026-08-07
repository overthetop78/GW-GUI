namespace GWGUI.Scp.Encoding;

public sealed class HeathkitFmTrackEncoder : TrackEncoderBase
{
    public override string Id => "heathkit.fm";
    public override string DisplayName => "Heathkit hard-sectored FM";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        var volume = (byte)Attribute(request, "volume", 0);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 256) throw new ArgumentException("Heathkit sectors contain 256 bytes.");
            byte[] identity = [volume, (byte)request.Cylinder, (byte)sector.Number];
            bits.Fm([0,0,0,0xbf]);
            bits.Fm(identity.Append(TrackEncoding.RotatingChecksum(identity)).Select(TrackEncoding.ReverseBits));
            bits.Gap(160);
            bits.Fm([0,0,0,0xbf]);
            bits.Fm(sector.Data.Append(TrackEncoding.RotatingChecksum(sector.Data)).Select(TrackEncoding.ReverseBits));
            bits.Gap(128);
        }
        return bits;
    }
}
