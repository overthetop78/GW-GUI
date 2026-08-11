using GWGUI.MediaEngine.Primitives;
using Aed6200pMfmFormat = GWGUI.MediaEngine.Encoding.Definitions.Aed6200pMfmFormat;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Aed6200p MFM.</summary>
public sealed class Aed6200pMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.Aed6200pMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.Aed6200pMfm;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var size = sector.Data.Count;
            byte[] header = [Aed6200pMfmFormat.HeaderAddressMark,(byte)request.Cylinder,(byte)size,(byte)sector.Number,(byte)(size >> BitPrimitives.BitsPerByte)];
            var headerCrc = Primitives.Crc16Calculator.Compute(header);
            bits.Raw(Aed6200pMfmFormat.HeaderPattern.ToArray());
            bits.Mfm(header.Skip(1).Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte),(byte)headerCrc]));
            bits.Gap(Aed6200pMfmFormat.FirstGapBitCount);
            var mark = sector.Deleted ? Aed6200pMfmFormat.DeletedDataMark : Aed6200pMfmFormat.DataMark;
            var dataCrc = Primitives.Crc16Calculator.Compute(new[] { mark }.Concat(sector.Data));
            bits.Raw(Aed6200pMfmFormat.DataPatterns[sector.Deleted ? 0 : 3].ToArray());
            bits.Mfm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte),(byte)dataCrc]));
            bits.Gap(Aed6200pMfmFormat.SecondGapBitCount);
        }
        return bits;
    }
}
