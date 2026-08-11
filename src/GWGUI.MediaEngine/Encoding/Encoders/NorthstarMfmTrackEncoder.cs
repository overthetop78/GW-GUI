using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public sealed class NorthstarMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.NorthstarMfm;
    public override string DisplayName => FluxCodecDisplayNames.NorthstarMfm;
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != NorthstarMfmFormat.SectorSize) throw NorthstarMfmFormat.InvalidSectorSize(sector.Data.Count);
            bits.Raw(NorthstarMfmFormat.SectorMark.ToArray());
            bits.Mfm([(byte)(request.Cylinder << NorthstarMfmFormat.CylinderShift | sector.Number & NorthstarMfmFormat.SectorMask)]);
            bits.Mfm(sector.Data);
            bits.Mfm([TrackEncoding.RotatingChecksum(sector.Data)]);
            bits.Gap(NorthstarMfmFormat.GapBitCount);
        }
        return bits;
    }
}
