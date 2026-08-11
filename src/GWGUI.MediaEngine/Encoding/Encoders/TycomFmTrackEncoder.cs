using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public sealed class TycomFmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.TycomFm;
    public override string DisplayName => FluxCodecDisplayNames.TycomFm;
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != TycomFmFormat.SectorSize) throw TycomFmFormat.InvalidSectorSize(sector.Data.Count);
            var headerCrc = Primitives.Crc16Calculator.Compute([TycomFmFormat.HeaderAddressMark,(byte)request.Cylinder,(byte)sector.Number],TycomFmFormat.CrcPolynomial,TycomFmFormat.CrcInitialValue);
            bits.Raw(TycomFmFormat.HeaderMark.ToArray());
            bits.DoubleFm([(byte)request.Cylinder,(byte)sector.Number,(byte)(headerCrc >> BitPrimitives.BitsPerByte),(byte)headerCrc]);
            bits.Gap(TycomFmFormat.GapBitCount, true);
            var mark = sector.Deleted ? TycomFmFormat.DeletedDataMark : TycomFmFormat.DataMark;
            var dataCrc = Primitives.Crc16Calculator.Compute(new[] { mark }.Concat(sector.Data),TycomFmFormat.CrcPolynomial,TycomFmFormat.CrcInitialValue);
            bits.Raw(TycomFmFormat.DataMarks.Single(item=>item.Mark==mark).Pattern.ToArray());
            bits.DoubleFm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte),(byte)dataCrc]));
            bits.Gap(TycomFmFormat.GapBitCount, true);
        }
        return bits;
    }
}
