namespace GWGUI.Scp.SectorImages;

internal sealed class AtariStIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = ["iso.mfm"];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        var candidates = candidateSet.Addressed;
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var resolvedFormat = formatId ??
            $"atarist.{(measured.Cylinders * measured.Heads * measured.SectorsPerTrack * measured.SectorSize) / 1024}";
        return IsoSectorImageBuilder.CreateUniform(resolvedFormat, candidates, measured.SectorSize,
            measured.Cylinders, measured.Heads, measured.SectorsPerTrack,
            address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1);
    }
}
