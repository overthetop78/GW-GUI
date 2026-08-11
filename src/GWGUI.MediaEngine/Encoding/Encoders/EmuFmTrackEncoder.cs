using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format E-mu FM.</summary>
public sealed class EmuFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => EmuFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => EmuFmFormat.CodecDisplayName;

    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != EmuFmFormat.SectorSize) throw EmuFmFormat.InvalidSectorSize(sector.Data.Count);
            var rawTrack = BitPrimitives.ReverseBits((byte)(request.Cylinder << EmuFmFormat.TrackShift | request.Head));
            var headerCrc = Crc16Calculator.Compute([rawTrack], EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue);
            bits.Raw(EmuFmFormat.SectorMark.ToArray());
            bits.DoubleFm([rawTrack, (byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]);
            bits.Gap(EmuFmFormat.GapBitCount, true);
            var dataCrc = Crc16Calculator.Compute(sector.Data, EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue);
            bits.Raw(EmuFmFormat.SectorMark.ToArray());
            bits.DoubleFm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(EmuFmFormat.GapBitCount, true);
        }
        return bits;
    }
}
