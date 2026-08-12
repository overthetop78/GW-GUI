namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Construit une image Epson QX-10 depuis les candidats physiques ISO.</summary>
internal sealed class EpsonQx10IsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidates)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        return EpsonQx10SectorImagePolicy.CreateImage(formatId, candidates.Physical);
    }
}
    /// <summary>Obtient les codecs ISO FM et MFM essayés.</summary>
    /// <summary>Construit l'image Epson dans la disposition demandée.</summary>
