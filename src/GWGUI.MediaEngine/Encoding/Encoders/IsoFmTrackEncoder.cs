using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Iso FM.</summary>
public sealed class IsoFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.IsoFm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.IsoFm;

    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var sizeCode = sector.SizeCode ?? TrackEncoding.SizeCode(sector.Data.Count);
            byte[] header = [IsoFmFormat.IdAddressMark, (byte)request.Cylinder, (byte)request.Head, (byte)sector.Number, sizeCode];
            var headerCrc = Primitives.Crc16Calculator.Compute(header, IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue);
            bits.Raw((byte)(IsoFmFormat.EncodedIdAddressMark >> BitPrimitives.BitsPerByte), (byte)(IsoFmFormat.EncodedIdAddressMark & byte.MaxValue));
            bits.Fm(header.Skip(1).Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]));
            bits.Gap(IsoFmFormat.HeaderGapBitCount);
            var mark = sector.Deleted ? IsoFmFormat.DeletedDataAddressMark : IsoFmFormat.DataAddressMark;
            var dataCrc = Primitives.Crc16Calculator.Compute(new[] { mark }.Concat(sector.Data), IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue);
            var encodedMark = sector.Deleted ? IsoFmFormat.EncodedDeletedDataAddressMark : IsoFmFormat.EncodedDataAddressMark;
            bits.Raw((byte)(encodedMark >> BitPrimitives.BitsPerByte), (byte)encodedMark);
            bits.Fm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(IsoFmFormat.DataGapBitCount);
        }
        return bits;
    }
}
