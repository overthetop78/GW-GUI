using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

public sealed class Aed6200pMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => "aed6200p.mfm";
    public override string DisplayName => "AED 6200P MFM";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var size = sector.Data.Count;
            byte[] header = [0xc6,(byte)request.Cylinder,(byte)size,(byte)sector.Number,(byte)(size >> BitPrimitives.BitsPerByte)];
            var headerCrc = TrackEncoding.Crc16(header);
            bits.RawHex("5094");
            bits.Mfm(header.Skip(1).Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte),(byte)headerCrc]));
            bits.Gap(64);
            var mark = sector.Deleted ? (byte)0xc0 : (byte)0xc3;
            var dataCrc = TrackEncoding.Crc16(new[] { mark }.Concat(sector.Data));
            bits.RawHex(sector.Deleted ? "508A" : "5085");
            bits.Mfm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte),(byte)dataCrc]));
            bits.Gap(128);
        }
        return bits;
    }
}
