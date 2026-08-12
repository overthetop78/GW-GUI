using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Apple;

/// <summary>Reconstruit les images Apple II DOS et ProDOS depuis des secteurs SCP décodés.</summary>
/// <param name="decoder">Décodeur commun chargé de regrouper les candidats sectoriels Apple.</param>
internal sealed class AppleIIScpSectorReconstructor(AppleScpSectorDecoder decoder)
{
    /// <summary>Reconstruit une image Apple II dans l'ordre demandé.</summary>
    /// <param name="scp">Capture SCP déjà analysée.</param>
    /// <param name="prodosOrder">Indique si les paires de secteurs doivent être réunies en blocs ProDOS.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le décodage des révolutions.</param>
    /// <returns>L'image Apple II reconstruite dans l'ordre DOS ou ProDOS demandé.</returns>
    /// <exception cref="InvalidDataException">Aucun secteur Apple II n'a été décodé ou aucun candidat ne respecte la géométrie retenue.</exception>
    public SectorImage Decode(ScpImage scp, bool prodosOrder, CancellationToken cancellationToken)
    {
        var candidates = decoder.DecodeCandidates(scp, FluxCodecIds.AppleIIGcr, AppleIIGcrFormat.SectorSize, cancellationToken);
        if (candidates.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors(AppleIIGcrFormat.StructureDescriptionName);
        if (prodosOrder) return CreateProDosImage(candidates);
        var sectorsPerTrack = candidates.Keys.Any(address => address.Number >= AppleIIGcrFormat.FiveAndThreeSectorsPerTrack) ? AppleIIGcrFormat.SixAndTwoSectorsPerTrack : AppleIIGcrFormat.FiveAndThreeSectorsPerTrack;
        var blocks = candidates.Where(pair => pair.Key.Cylinder < AppleIIGeometry.MaximumReconstructedTrackCount && pair.Key.Number >= 0 && pair.Key.Number < sectorsPerTrack)
            .Select(pair => AppleScpSectorDecoder.Select(
                pair.Key.Cylinder * sectorsPerTrack + (sectorsPerTrack == AppleIIGeometry.SectorsPerTrack ? AppleIIGeometry.PhysicalToDos[pair.Key.Number] : pair.Key.Number), pair.Key, pair.Value)).ToArray();
        if (blocks.Length == 0) throw ScpReconstructionExceptions.NoUsableSectors(AppleIIGcrFormat.StructureDescriptionName);
        var formatId = sectorsPerTrack == AppleIIGeometry.Dos32SectorsPerTrack ? DiskImageFormatIds.AppleIIDos32 : DiskImageFormatIds.AppleIIDos33;
        return new(formatId, AppleIIGeometry.SectorSize, Math.Max(AppleIIGeometry.TrackCount, blocks.Max(block => block.Address.Cylinder) + 1), DiskGeometryConstants.SingleSidedHeadCount, sectorsPerTrack, blocks);
    }

    /// <summary>Réunit les paires de secteurs physiques en blocs ProDOS.</summary>
    /// <param name="candidates">Candidats Apple II regroupés par adresse physique.</param>
    /// <returns>L'image Apple II dont les paires de secteurs complètes sont réunies en blocs ProDOS.</returns>
    /// <exception cref="InvalidDataException">Aucune piste candidate ne respecte la limite Apple II ou aucun bloc ProDOS complet ne peut être construit.</exception>
    private static SectorImage CreateProDosImage(Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> candidates)
    {
        var usableTracks = candidates.Keys.Where(key => key.Cylinder < AppleIIGeometry.MaximumReconstructedTrackCount).Select(key => key.Cylinder).ToArray();
        if (usableTracks.Length == 0) throw ScpReconstructionExceptions.NoUsableSectors(AppleScpReconstructionDefinitions.AppleIIProDosReconstructorName);
        var tracks = Math.Max(AppleIIGeometry.TrackCount, usableTracks.Max() + 1);
        var blocks = new List<SectorBlock>();
        for (var track = 0; track < tracks; track++)
        for (var blockOnTrack = 0; blockOnTrack < AppleIIGeometry.ProDosBlocksPerTrack; blockOnTrack++)
        {
            var data = new byte[AppleIIGeometry.ProDosBlockSize];
            var integrity = true;
            var revolution = 0;
            var complete = true;
            for (var half = 0; half < AppleIIGeometry.SectorsPerProDosBlock; half++)
            {
                var logicalSector = blockOnTrack * AppleIIGeometry.SectorsPerProDosBlock + half;
                var address = new SectorAddress(track, AppleIIGcrFormat.LogicalHead, AppleIIGeometry.ProDosToPhysical[logicalSector]);
                if (!candidates.TryGetValue(address, out var values))
                {
                    complete = false;
                    break;
                }
                var selected = AppleScpSectorDecoder.Select(0, address, values);
                selected.Data.ToArray().CopyTo(data, half * AppleIIGeometry.SectorSize);
                integrity &= selected.IntegrityValid == true;
                revolution = Math.Max(revolution, selected.Revolution);
            }
            if (complete)
                blocks.Add(new(track * AppleIIGeometry.ProDosBlocksPerTrack + blockOnTrack, new(track, AppleIIGcrFormat.LogicalHead, blockOnTrack), data, integrity, revolution));
        }
        if (blocks.Count == 0) throw ScpReconstructionExceptions.NoUsableSectors(AppleScpReconstructionDefinitions.AppleIIProDosReconstructorName);
        return new(DiskImageFormatIds.AppleIIProDos, AppleIIGeometry.ProDosBlockSize, tracks, DiskGeometryConstants.SingleSidedHeadCount, AppleIIGeometry.ProDosBlocksPerTrack, blocks);
    }
}
