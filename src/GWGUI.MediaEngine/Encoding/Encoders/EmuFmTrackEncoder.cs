using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public sealed class EmuFmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.EmuFm;
    public override string DisplayName => FluxCodecDisplayNames.EmuFm;
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits();
        foreach(var sector in request.Sectors)
        {
            if(sector.Data.Count!=EmuFmFormat.SectorSize) throw new ArgumentException("E-mu sectors contain 3584 bytes.");
            var rawTrack=Primitives.BitPrimitives.ReverseBits((byte)(request.Cylinder<<EmuFmFormat.TrackShift|request.Head));
            var headerCrc=Primitives.Crc16Calculator.Compute([rawTrack],EmuFmFormat.CrcPolynomial,EmuFmFormat.CrcInitialValue);
            bits.Raw(EmuFmFormat.SectorMark.ToArray());
            bits.DoubleFm([rawTrack,(byte)(headerCrc>>8),(byte)headerCrc]); bits.Gap(EmuFmFormat.GapBitCount,true);
            var dataCrc=Primitives.Crc16Calculator.Compute(sector.Data,EmuFmFormat.CrcPolynomial,EmuFmFormat.CrcInitialValue);
            bits.Raw(EmuFmFormat.SectorMark.ToArray());
            bits.DoubleFm(sector.Data.Concat([(byte)(dataCrc>>8),(byte)dataCrc])); bits.Gap(EmuFmFormat.GapBitCount,true);
        }
        return bits;
    }
}
