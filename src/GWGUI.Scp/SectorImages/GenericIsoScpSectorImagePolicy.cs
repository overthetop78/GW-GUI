namespace GWGUI.Scp.SectorImages;

internal sealed class GenericIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = ["iso.fm", "iso.mfm"];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        var measured = IsoSectorImageBuilder.Measure(candidateSet.Addressed);
        return IsoSectorImageBuilder.CreateUniform(formatId, candidateSet.Addressed, measured.SectorSize,
            measured.Cylinders, measured.Heads, measured.SectorsPerTrack,
            address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1);
    }
}
