namespace GWGUI.MediaEngine.Encoding;

public sealed class CenturionMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => "centurion.mfm";
    public override string DisplayName => "Centurion MFM";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            byte[] identity = [(byte)request.Cylinder,(byte)sector.Number];
            var headerCrc = TrackEncoding.Crc16(identity, 0x1021, 0);
            bits.RawHex("91224489");
            bits.Mfm(identity.Concat([(byte)(headerCrc >> 8),(byte)headerCrc]));
            bits.Gap(400);
            var blocks = Math.Max(1, (sector.Data.Count + 255) / 256);
            var payload = sector.Data.Concat(Enumerable.Repeat((byte)0, blocks * 256 - sector.Data.Count)).ToArray();
            var dataCrc = TrackEncoding.Crc16(new byte[] { (byte)blocks, 0 }.Concat(payload), 0x1021, 0);
            bits.RawHex("AAAAAAA9");
            bits.Mfm(new byte[] { 0,(byte)blocks,0 }.Concat(payload).Concat([(byte)(dataCrc >> 8),(byte)dataCrc]));
            bits.Gap(128);
        }
        return bits;
    }
}
