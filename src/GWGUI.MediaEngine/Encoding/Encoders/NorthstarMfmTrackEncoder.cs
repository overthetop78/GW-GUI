using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Northstar MFM.</summary>
public sealed class NorthstarMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.NorthstarMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.NorthstarMfm;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
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
