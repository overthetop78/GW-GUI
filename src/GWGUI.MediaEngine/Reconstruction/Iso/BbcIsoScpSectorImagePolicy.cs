using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Construit une image BBC DFS depuis des candidats ISO FM.</summary>
internal sealed class BbcIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    /// <summary>Identifiant du décodeur ISO FM utilisé par BBC DFS.</summary>
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm];

    /// <summary>Résout la géométrie BBC demandée ou observée puis construit l'image uniforme correspondante.</summary>
    /// <param name="formatId">Identifiant DFS demandé, ou <see langword="null"/> pour choisir SSD/DSD 40/80 depuis la géométrie mesurée.</param>
    /// <param name="candidateSet">Candidats ISO regroupés et validés avant la construction.</param>
    /// <returns>L'image sectorielle BBC DFS associée à l'une des quatre géométries standard.</returns>
    /// <exception cref="ArgumentException">L'identifiant demandé ne correspond à aucune géométrie BBC DFS prise en charge.</exception>
    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        var measured = IsoSectorImageBuilder.Measure(candidateSet.Addressed);
        var geometry = formatId is null ? BbcDfsGeometry.FromObservedGeometry(measured.Cylinders, measured.Heads) : BbcDfsGeometry.FindByFormatId(formatId) ?? throw ScpReconstructionExceptions.InvalidRequestedFormat(BbcDfsGeometry.FormatFamilyName, formatId);
        return IsoSectorImageBuilder.CreateUniform(geometry.FormatId, candidateSet.Addressed, measured.SectorSize, geometry.Cylinders, geometry.Heads, BbcDfsGeometry.SectorsPerTrack, address => Array.IndexOf(measured.SectorOrder, address.Number));
    }
}
