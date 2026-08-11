using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Iso MFM.</summary>
public sealed class IsoMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.IsoMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.IsoMfm;

    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var sizeCode = sector.SizeCode ?? TrackEncoding.SizeCode(sector.Data.Count);
            byte[] header = [IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.IdAddressMark, (byte)request.Cylinder, (byte)request.Head, (byte)sector.Number, sizeCode];
            var headerCrc = Primitives.Crc16Calculator.Compute(header, IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue);
            bits.RawHex(IsoMfmFormat.EncodedSyncHex);
            bits.Mfm(header.Skip(IsoMfmFormat.SyncByteCount).Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]));
            bits.Gap(IsoMfmFormat.HeaderGapBitCount);
            var mark = sector.Deleted ? IsoMfmFormat.DeletedDataAddressMark : IsoMfmFormat.DataAddressMark;
            var dataCrc = Primitives.Crc16Calculator.Compute(new byte[] { IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, mark }.Concat(sector.Data), IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue);
            bits.RawHex(IsoMfmFormat.EncodedSyncHex);
            bits.Mfm(new[] { mark }.Concat(sector.Data).Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(IsoMfmFormat.DataGapBitCount);
        }
        return bits;
    }
}
