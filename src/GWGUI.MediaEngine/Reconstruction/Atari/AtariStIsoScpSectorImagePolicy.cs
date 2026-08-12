using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.Geometries.Atari;

namespace GWGUI.MediaEngine.Reconstruction.Atari;

/// <summary>Construit une image Atari ST depuis des candidats ISO MFM.</summary>
internal sealed class AtariStIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    /// <summary>Identifiant du décodeur ISO MFM utilisé pour les pistes Atari ST.</summary>
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoMfm];

    /// <summary>Mesure les candidats Atari ST et construit l'image uniforme correspondante.</summary>
    /// <param name="formatId">Identifiant demandé, ou <see langword="null"/> pour le déduire de la capacité mesurée.</param>
    /// <param name="candidateSet">Candidats ISO regroupés et validés avant la construction.</param>
    /// <returns>L'image sectorielle Atari ST dont la capacité est exprimée en octets.</returns>
    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        var candidates = candidateSet.Addressed;
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var explicitGeometry = formatId is not null && AtariStGeometry.TryFromFormatId(formatId, out var geometry) ? geometry : (AtariStGeometry?)null;
        var resolvedFormat = explicitGeometry?.FormatId ?? DiskImageFormatIds.AtariStFromCapacity((long)measured.Cylinders * measured.Heads * measured.SectorsPerTrack * measured.SectorSize);
        var cylinders = explicitGeometry?.Cylinders ?? measured.Cylinders;
        var heads = explicitGeometry?.Heads ?? measured.Heads;
        var sectorsPerTrack = explicitGeometry?.SectorsPerTrack ?? measured.SectorsPerTrack;
        return IsoSectorImageBuilder.CreateUniform(resolvedFormat, candidates, measured.SectorSize,
            cylinders, heads, sectorsPerTrack,
            address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1);
    }
}
