namespace GWGUI.MediaEngine.Encoding;

public sealed class MicralNFmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.MicralNFm;
    public override string DisplayName => FluxCodecDisplayNames.MicralNFm;
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 128) throw new ArgumentException("Micral N sectors contain 128 bytes.");
            byte checksum = 0;
            foreach (var value in sector.Data) checksum = Update(checksum, value);
            bits.Fm(new byte[] { 0,0,0,0xff,(byte)sector.Number,(byte)request.Cylinder }.Concat(sector.Data).Append(checksum));
            bits.Gap(128);
        }
        return bits;
    }
    private static byte Update(byte checksum, byte data)
    {
        var carrySource = ((data ^ checksum) ^ 0xff) & ((data + checksum) ^ data);
        return (byte)(checksum + data + ((carrySource & 0x80) != 0 ? 1 : 0));
    }
}
