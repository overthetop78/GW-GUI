using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Apple;

/// <summary>Reconstruit une image Apple II RWTS18 depuis des secteurs SCP décodés.</summary>
/// <param name="decoder">Décodeur commun chargé de regrouper les candidats sectoriels Apple.</param>
internal sealed class AppleRwts18ScpSectorReconstructor(AppleScpSectorDecoder decoder)
{
    /// <summary>Sélectionne les secteurs RWTS18 et construit l'image sectorielle.</summary>
    /// <param name="scp">Capture SCP déjà analysée.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le décodage des révolutions.</param>
    /// <returns>L'image Apple II RWTS18 reconstruite à partir des secteurs dont l'adresse est utilisable.</returns>
    /// <exception cref="InvalidDataException">Aucun secteur RWTS18 n'a été décodé ou aucun candidat ne respecte la géométrie acceptée.</exception>
    public SectorImage Decode(ScpImage scp, CancellationToken cancellationToken)
    {
        var candidates = decoder.DecodeCandidates(scp, FluxCodecIds.AppleRwts18, AppleRwts18Format.SectorByteCount, cancellationToken);
        if (candidates.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors(AppleRwts18Format.StructureDescriptionName);
        var blocks = candidates.Where(pair => pair.Key.Cylinder is >= 0 and < AppleIIGeometry.MaximumReconstructedTrackCount && pair.Key.Number is >= 0 and <= AppleRwts18Format.LastSectorNumber)
            .Select(pair => AppleScpSectorDecoder.Select(pair.Key.Cylinder * AppleRwts18Format.SectorCount + pair.Key.Number, pair.Key, pair.Value)).ToArray();
        if (blocks.Length == 0) throw ScpReconstructionExceptions.NoUsableSectors(AppleRwts18Format.StructureDescriptionName);
        var tracks = Math.Max(AppleIIGeometry.TrackCount, blocks.Max(block => block.Address.Cylinder) + 1);
        return new(DiskImageFormatIds.AppleIIRwts18, AppleRwts18Format.SectorByteCount, tracks, DiskGeometryConstants.SingleSidedHeadCount, AppleRwts18Format.SectorCount, blocks);
    }
}
