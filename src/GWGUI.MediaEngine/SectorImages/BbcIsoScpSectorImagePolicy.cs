using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Geometries.Acorn;

namespace GWGUI.MediaEngine.SectorImages;

internal sealed class BbcIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        var measured = IsoSectorImageBuilder.Measure(candidateSet.Addressed);
        var cylinders = formatId.EndsWith("80", StringComparison.OrdinalIgnoreCase) ? DiskGeometryConstants.EightyTrackCylinderCount : DiskGeometryConstants.FortyTrackCylinderCount;
        var heads = formatId.Contains(".ds", StringComparison.OrdinalIgnoreCase) ? DiskGeometryConstants.DoubleSidedHeadCount : DiskGeometryConstants.SingleSidedHeadCount;
        return IsoSectorImageBuilder.CreateUniform(formatId, candidateSet.Addressed, measured.SectorSize,
            cylinders, heads, BbcDfsGeometry.SectorsPerTrack, address => Array.IndexOf(measured.SectorOrder, address.Number));
    }
}
