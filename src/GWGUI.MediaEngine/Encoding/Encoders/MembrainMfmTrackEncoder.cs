using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public sealed class MembrainMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.MembrainMfm;
    public override string DisplayName => FluxCodecDisplayNames.MembrainMfm;
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MembrainMfmFormat.SectorSize) throw MembrainMfmFormat.InvalidSectorSize(sector.Data.Count);
            var cylinderHigh = (byte)(request.Cylinder >> MembrainMfmFormat.CylinderLowBitCount);
            var packed = (byte)((request.Cylinder & MembrainMfmFormat.CylinderLowValueMask) << MembrainMfmFormat.CylinderLowShift | request.Head << MembrainMfmFormat.HeadShift | sector.Number & MembrainMfmFormat.SectorMask);
            byte[] header = [MembrainMfmFormat.SyncByte, MembrainMfmFormat.HeaderAddressMark, cylinderHigh, packed];
            var headerCrc = Primitives.Crc16Calculator.Compute(header, MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue);
            bits.Raw(MembrainMfmFormat.SectorHeader.ToArray());
            bits.Mfm([header[2], header[3], (byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]);
            bits.Gap(MembrainMfmFormat.HeaderGapBitCount);
            const byte mark = MembrainMfmFormat.DataAddressMark;
            var dataCrc = Primitives.Crc16Calculator.Compute(new[] { MembrainMfmFormat.SyncByte, mark }.Concat(sector.Data), MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue);
            bits.Raw(MembrainMfmFormat.SectorData.ToArray());
            bits.Mfm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(MembrainMfmFormat.DataGapBitCount);
        }
        return bits;
    }
}
