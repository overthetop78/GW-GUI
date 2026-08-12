using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Construit une image sectorielle Amstrad uniforme à partir de candidats ISO FM ou MFM.</summary>
internal sealed class AmstradIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    /// <summary>Identifiants des décodeurs ISO FM et MFM susceptibles de produire les secteurs Amstrad.</summary>
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    /// <summary>Mesure les secteurs Amstrad décodés et construit l'image sectorielle uniforme correspondante.</summary>
    /// <param name="formatId">Identifiant technique du format Amstrad à attribuer à l'image.</param>
    /// <param name="candidateSet">Candidats ISO regroupés et validés avant la construction.</param>
    /// <returns>L'image sectorielle dont la géométrie est déduite des candidats adressés.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatId"/> est <see langword="null"/>.</exception>
    /// <remarks>Les numéros de secteurs situés au-delà de la géométrie mesurée restent autorisés afin de préserver les dispositions Amstrad particulières.</remarks>
    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        var measured = IsoSectorImageBuilder.Measure(candidateSet.Addressed);
        return IsoSectorImageBuilder.CreateUniform(formatId, candidateSet.Addressed, measured.SectorSize, measured.Cylinders, measured.Heads, measured.SectorsPerTrack,
            address => Array.IndexOf(measured.SectorOrder, address.Number), allowSectorNumbersBeyondGeometry: true);
    }
}
