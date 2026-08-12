using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Construit une image ISO générique depuis sa géométrie mesurée.</summary>
internal sealed class GenericIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    /// <summary>Identifiants des décodeurs ISO FM et MFM acceptés par la politique générique.</summary>
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    /// <summary>Construit une image ISO uniforme depuis la géométrie mesurée.</summary>
    /// <param name="formatId">Identifiant explicite à conserver dans l'image.</param>
    /// <param name="candidateSet">Candidats ISO regroupés par adresse logique et physique.</param>
    /// <returns>L'image sectorielle uniforme mesurée depuis les candidats adressés.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatId"/> est <see langword="null"/>.</exception>
    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        var measured = IsoSectorImageBuilder.Measure(candidateSet.Addressed);
        return IsoSectorImageBuilder.CreateUniform(formatId, candidateSet.Addressed, measured.SectorSize,
            measured.Cylinders, measured.Heads, measured.SectorsPerTrack,
            address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1);
    }
}
