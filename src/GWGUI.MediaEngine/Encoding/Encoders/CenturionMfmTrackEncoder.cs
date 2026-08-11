using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

public sealed class CenturionMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.CenturionMfm;
    public override string DisplayName => FluxCodecDisplayNames.CenturionMfm;
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            byte[] identity = [(byte)request.Cylinder,(byte)sector.Number];
            var headerCrc = Primitives.Crc16Calculator.Compute(identity, Primitives.Crc16Calculator.CcittPolynomial, Primitives.Crc16Calculator.ZeroInitialValue);
            bits.RawHex("91224489");
            bits.Mfm(identity.Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte),(byte)headerCrc]));
            bits.Gap(400);
            var blocks = Math.Max(1, (sector.Data.Count + 255) / 256);
            var payload = sector.Data.Concat(Enumerable.Repeat((byte)0, blocks * 256 - sector.Data.Count)).ToArray();
            var dataCrc = Primitives.Crc16Calculator.Compute(new byte[] { (byte)blocks, 0 }.Concat(payload), Primitives.Crc16Calculator.CcittPolynomial, Primitives.Crc16Calculator.ZeroInitialValue);
            bits.RawHex("AAAAAAA9");
            bits.Mfm(new byte[] { 0,(byte)blocks,0 }.Concat(payload).Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte),(byte)dataCrc]));
            bits.Gap(128);
        }
        return bits;
    }
}
