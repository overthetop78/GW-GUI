using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Centurion MFM.</summary>
public sealed class CenturionMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => CenturionMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => CenturionMfmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            byte[] identity = [(byte)request.Cylinder,(byte)sector.Number];
            var headerCrc = Primitives.Crc16Calculator.Compute(identity, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue);
            bits.Raw(CenturionMfmFormat.SectorMark.ToArray());
            bits.Mfm(identity.Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte),(byte)headerCrc]));
            bits.Gap(CenturionMfmFormat.HeaderGapBitCount);
            var blocks = Math.Max(1, (sector.Data.Count + CenturionMfmFormat.AllocationBlockSize - 1) / CenturionMfmFormat.AllocationBlockSize);
            var payload = sector.Data.Concat(Enumerable.Repeat((byte)0, blocks * CenturionMfmFormat.AllocationBlockSize - sector.Data.Count)).ToArray();
            var dataCrc = Primitives.Crc16Calculator.Compute(new byte[] { (byte)blocks, CenturionMfmFormat.SupportedDataKey }.Concat(payload), CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue);
            bits.Raw(CenturionMfmFormat.DataMark.ToArray());
            bits.Mfm(new byte[] { CenturionMfmFormat.SupportedDataKey,(byte)blocks,CenturionMfmFormat.SupportedDataKey }.Concat(payload).Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte),(byte)dataCrc]));
            bits.Gap(CenturionMfmFormat.DataGapBitCount);
        }
        return bits;
    }
}
