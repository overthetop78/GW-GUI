using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Définit le contrat d'une politique construisant une image depuis des candidats ISO FM ou MFM.</summary>
internal interface IIsoScpSectorImagePolicy
{
    /// <summary>Obtient les identifiants techniques des décodeurs acceptés par la politique.</summary>
    IReadOnlyList<string> DecoderIds { get; }

    /// <summary>Construit une image depuis les candidats décodés.</summary>
    /// <param name="formatId">Identifiant demandé, ou <see langword="null"/> pour une politique autorisant la détection automatique.</param>
    /// <param name="candidates">Candidats ISO regroupés par adresse logique et physique.</param>
    /// <returns>L'image sectorielle construite selon la politique.</returns>
    /// <exception cref="InvalidDataException">Les candidats ne permettent pas de construire une image valide.</exception>
    SectorImage Build(string? formatId, IsoSectorCandidateSet candidates);
}
