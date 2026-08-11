namespace GWGUI.MediaEngine.SectorImages;

internal sealed class EpsonQx10IsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidates)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        return EpsonQx10SectorImagePolicy.CreateImage(formatId, candidates.Physical);
    }
}
