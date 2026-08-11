using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public sealed class HpMmfmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.HpMmfm;
    public override string DisplayName => FluxCodecDisplayNames.HpMmfm;

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != HpMmfmFormat.SectorSize) throw new ArgumentException("HP MMFM sectors contain 256 bytes.");
            var encodedSector = (byte)(sector.Number | request.Head << HpMmfmFormat.HeadShift);
            byte[] identity = [Primitives.BitPrimitives.ReverseBits((byte)request.Cylinder), Primitives.BitPrimitives.ReverseBits(encodedSector)];
            bits.Raw(HpMmfmFormat.SectorSync.ToArray());
            bits.Mfm(TrackEncoding.WithCrc(identity));
            bits.Gap(HpMmfmFormat.HeaderGapBitCount);
            var payload = sector.Data.ToArray();
            for (var index = 0; index < payload.Length; index += 2) (payload[index], payload[index + 1]) = (payload[index + 1], payload[index]);
            for (var index = 0; index < payload.Length; index++) payload[index] = Primitives.BitPrimitives.ReverseBits(payload[index]);
            bits.Raw(HpMmfmFormat.DataSync.ToArray());
            bits.Mfm(TrackEncoding.WithCrc(payload));
            bits.Gap(HpMmfmFormat.DataGapBitCount);
        }
        return bits;
    }
}
