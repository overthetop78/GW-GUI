using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

public sealed class IsoFmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.IsoFm;
    public override string DisplayName => "ISO FM";

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var sizeCode = sector.SizeCode ?? TrackEncoding.SizeCode(sector.Data.Count);
            byte[] header = [0xfe, (byte)request.Cylinder, (byte)request.Head, (byte)sector.Number, sizeCode];
            var headerCrc = Primitives.Crc16Calculator.Compute(header);
            bits.RawHex("F57E");
            bits.Fm(header.Skip(1).Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]));
            bits.Gap(160);
            var mark = sector.Deleted ? (byte)0xf8 : (byte)0xfb;
            var dataCrc = Primitives.Crc16Calculator.Compute(new[] { mark }.Concat(sector.Data));
            bits.RawHex(sector.Deleted ? "F56A" : "F56F");
            bits.Fm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(256);
        }
        return bits;
    }
}
