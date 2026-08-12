namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Construit une image ISO générique depuis sa géométrie mesurée.</summary>
internal sealed class GenericIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        var measured = IsoSectorImageBuilder.Measure(candidateSet.Addressed);
        return IsoSectorImageBuilder.CreateUniform(formatId, candidateSet.Addressed, measured.SectorSize,
            measured.Cylinders, measured.Heads, measured.SectorsPerTrack,
            address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1);
    }
}
    /// <summary>Obtient les codecs ISO FM et MFM essayés.</summary>
    /// <summary>Construit l'image uniforme correspondant à l'identifiant demandé.</summary>
