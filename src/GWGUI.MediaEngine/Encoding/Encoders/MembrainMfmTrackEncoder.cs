using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Membrain MFM.</summary>
public sealed class MembrainMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => MembrainMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => MembrainMfmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MembrainMfmFormat.SectorSize) throw MembrainMfmFormat.InvalidSectorSize(sector.Data.Count);
            var (cylinderHigh, packed) = MembrainMfmAddress.Pack(request.Cylinder, request.Head, sector.Number);
            byte[] header = [MembrainMfmFormat.SyncByte, MembrainMfmFormat.HeaderAddressMark, cylinderHigh, packed];
            var headerCrc = Primitives.Crc16Calculator.Compute(header, MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue);
            bits.Raw(MembrainMfmFormat.HeaderPattern.ToArray());
            bits.Mfm([header[2], header[3], (byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]);
            bits.Gap(MembrainMfmFormat.HeaderGapBitCount);
            const byte mark = MembrainMfmFormat.DataAddressMark;
            var dataCrc = Primitives.Crc16Calculator.Compute(new[] { MembrainMfmFormat.SyncByte, mark }.Concat(sector.Data), MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue);
            bits.Raw(MembrainMfmFormat.DataPattern.ToArray());
            bits.Mfm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(MembrainMfmFormat.DataGapBitCount);
        }
        return bits;
    }
}
