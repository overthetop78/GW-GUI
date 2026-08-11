namespace GWGUI.MediaEngine.Encoding;

public sealed class NorthstarMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.NorthstarMfm;
    public override string DisplayName => FluxCodecDisplayNames.NorthstarMfm;
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 512) throw new ArgumentException("NorthStar sectors contain 512 bytes.");
            bits.Mfm([0,0,0,0,0,0,0,0xfb]);
            bits.Mfm([(byte)(request.Cylinder << 4 | sector.Number & 15)]);
            bits.Mfm(sector.Data);
            bits.Mfm([TrackEncoding.RotatingChecksum(sector.Data)]);
            bits.Gap(128);
        }
        return bits;
    }
}
