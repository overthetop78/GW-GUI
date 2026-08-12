using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.MediaEngine.Reconstruction.EpsonQx10;

/// <summary>Construit une image Epson QX-10 depuis les candidats physiques ISO.</summary>
internal sealed class EpsonQx10IsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    /// <summary>Identifiants des décodeurs ISO FM et MFM acceptés par la politique Epson.</summary>
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    /// <summary>Construit l'image Epson selon la disposition explicitement demandée.</summary>
    /// <param name="formatId">Identifiant obligatoire de la disposition Epson QX-10.</param>
    /// <param name="candidates">Candidats ISO regroupés par adresse physique et logique.</param>
    /// <returns>L'image Epson construite depuis les candidats physiques.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatId"/> est <see langword="null"/>.</exception>
    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidates)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        return EpsonQx10SectorImageBuilder.Create(formatId, candidates.Physical);
    }
}
