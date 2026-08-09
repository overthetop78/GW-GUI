namespace GWGUI.Scp.SectorImages;

internal sealed class BbcIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = ["iso.fm"];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        var measured = IsoSectorImageBuilder.Measure(candidateSet.Addressed);
        var cylinders = formatId.EndsWith("80", StringComparison.OrdinalIgnoreCase) ? 80 : 40;
        var heads = formatId.Contains(".ds", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
        return IsoSectorImageBuilder.CreateUniform(formatId, candidateSet.Addressed, measured.SectorSize,
            cylinders, heads, 10, address => Array.IndexOf(measured.SectorOrder, address.Number));
    }
}
