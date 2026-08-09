namespace GWGUI.Scp.SectorImages;

internal interface IIsoScpSectorImagePolicy
{
    IReadOnlyList<string> DecoderIds { get; }

    SectorImage Build(string? formatId, IsoSectorCandidateSet candidates);
}
