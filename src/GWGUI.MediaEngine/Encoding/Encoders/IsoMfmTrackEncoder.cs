using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

public sealed class IsoMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.IsoMfm;
    public override string DisplayName => "ISO MFM";

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var sizeCode = sector.SizeCode ?? TrackEncoding.SizeCode(sector.Data.Count);
            byte[] header = [0xa1, 0xa1, 0xa1, 0xfe, (byte)request.Cylinder, (byte)request.Head, (byte)sector.Number, sizeCode];
            var headerCrc = Primitives.Crc16Calculator.Compute(header);
            bits.RawHex("448944894489");
            bits.Mfm(header.Skip(3).Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]));
            bits.Gap(160);
            var mark = sector.Deleted ? (byte)0xf8 : (byte)0xfb;
            var dataCrc = Primitives.Crc16Calculator.Compute(new byte[] { 0xa1, 0xa1, 0xa1, mark }.Concat(sector.Data));
            bits.RawHex("448944894489");
            bits.Mfm(new[] { mark }.Concat(sector.Data).Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(256);
        }
        return bits;
    }
}
