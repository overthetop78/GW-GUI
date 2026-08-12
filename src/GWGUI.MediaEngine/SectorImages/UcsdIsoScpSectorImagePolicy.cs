using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.MediaEngine.SectorImages;

internal sealed class UcsdIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        var candidates = candidateSet.Physical;
        var measured = IsoSectorImageBuilder.Measure(candidates);
        return IsoSectorImageBuilder.CreateUniform(formatId, candidates, measured.SectorSize,
            measured.Cylinders, 1, 8,
            address => Array.IndexOf(candidates.Keys
                .Where(item => item.Cylinder == address.Cylinder && item.Head == address.Head)
                .Select(item => item.Number).Distinct().OrderBy(number => number).ToArray(), address.Number));
    }
}
