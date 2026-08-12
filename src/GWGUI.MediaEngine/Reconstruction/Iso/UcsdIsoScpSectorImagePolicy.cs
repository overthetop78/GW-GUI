using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Geometries.Ucsd;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Construit une image UCSD IBM MFM depuis des candidats ISO FM ou MFM.</summary>
internal sealed class UcsdIsoScpSectorImagePolicy : IIsoScpSectorImagePolicy
{
    /// <summary>Obtient les identifiants des décodeurs ISO acceptés.</summary>
    public IReadOnlyList<string> DecoderIds { get; } = [FluxCodecIds.IsoFm, FluxCodecIds.IsoMfm];

    /// <summary>Construit une image UCSD IBM MFM depuis les candidats physiques décodés.</summary>
    /// <param name="formatId">Identifiant du format UCSD demandé.</param>
    /// <param name="candidateSet">Candidats ISO regroupés par adresse.</param>
    /// <returns>L'image sectorielle UCSD construite selon sa géométrie logique.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="formatId"/> est nul.</exception>
    /// <exception cref="InvalidDataException">Aucun candidat physique ne permet de mesurer l'image.</exception>
    public SectorImage Build(string? formatId, IsoSectorCandidateSet candidateSet)
    {
        ArgumentNullException.ThrowIfNull(formatId);
        var candidates = candidateSet.Physical;
        var measured = IsoSectorImageBuilder.Measure(candidates);
        return IsoSectorImageBuilder.CreateUniform(formatId, candidates, measured.SectorSize, measured.Cylinders, UcsdIbmMfmGeometry.HeadCount, UcsdIbmMfmGeometry.LogicalSectorsPerCylinder, address => Array.IndexOf(candidates.Keys.Where(item => item.Cylinder == address.Cylinder && item.Head == address.Head).Select(item => item.Number).Distinct().OrderBy(number => number).ToArray(), address.Number));
    }
}
