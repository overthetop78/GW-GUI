namespace GWGUI.MediaEngine.Encoding;

public sealed class TycomFmTrackEncoder : TrackEncoderBase
{
    public override string Id => "tycom.fm";
    public override string DisplayName => "TYCOM FM";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 128) throw new ArgumentException("TYCOM sectors contain 128 bytes.");
            var headerCrc = TrackEncoding.Crc16([0xfe,(byte)request.Cylinder,(byte)sector.Number]);
            bits.RawHex("55111554");
            bits.DoubleFm([(byte)request.Cylinder,(byte)sector.Number,(byte)(headerCrc >> 8),(byte)headerCrc]);
            bits.Gap(64, true);
            var mark = sector.Deleted ? (byte)0xf8 : (byte)0xfb;
            var dataCrc = TrackEncoding.Crc16(new[] { mark }.Concat(sector.Data));
            bits.RawHex(sector.Deleted ? "55111444" : "55111455");
            bits.DoubleFm(sector.Data.Concat([(byte)(dataCrc >> 8),(byte)dataCrc]));
            bits.Gap(64, true);
        }
        return bits;
    }
}
