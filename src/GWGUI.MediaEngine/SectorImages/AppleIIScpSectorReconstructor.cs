using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reconstruit les images Apple II DOS et ProDOS depuis des secteurs SCP décodés.</summary>
internal sealed class AppleIIScpSectorReconstructor(AppleScpSectorDecoder decoder)
{
    /// <summary>Reconstruit une image Apple II dans l'ordre demandé.</summary>
    public SectorImage Decode(ScpImage scp, bool prodosOrder, CancellationToken cancellationToken)
    {
        var candidates = decoder.DecodeCandidates(scp, FluxCodecIds.AppleIIGcr, AppleIIGcrFormat.SectorSize, cancellationToken);
        if (candidates.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors(AppleIIGcrFormat.StructureDescriptionName);
        if (prodosOrder) return CreateProDosImage(candidates);
        var sectorsPerTrack = candidates.Keys.Any(address => address.Number >= AppleIIGcrFormat.FiveAndThreeSectorsPerTrack) ? AppleIIGcrFormat.SixAndTwoSectorsPerTrack : AppleIIGcrFormat.FiveAndThreeSectorsPerTrack;
        var blocks = candidates.Where(pair => pair.Key.Cylinder < 50 && pair.Key.Number >= 0 && pair.Key.Number < sectorsPerTrack)
            .Select(pair => AppleScpSectorDecoder.Select(
                pair.Key.Cylinder * sectorsPerTrack + (sectorsPerTrack == AppleIIGeometry.SectorsPerTrack ? AppleIIGeometry.PhysicalToDos[pair.Key.Number] : pair.Key.Number), pair.Key, pair.Value)).ToArray();
        var formatId = sectorsPerTrack == 13 ? DiskImageFormatIds.AppleIIDos32 : DiskImageFormatIds.AppleIIDos33;
        return new(formatId, 256, Math.Max(35, blocks.Max(block => block.Address.Cylinder) + 1),
            1, sectorsPerTrack, blocks);
    }

    /// <summary>Réunit les paires de secteurs physiques en blocs ProDOS.</summary>
    private static SectorImage CreateProDosImage(Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>> candidates)
    {
        var tracks = Math.Max(35, candidates.Keys.Where(key => key.Cylinder < 50)
            .Max(key => key.Cylinder) + 1);
        var blocks = new List<SectorBlock>();
        for (var track = 0; track < tracks; track++)
        for (var blockOnTrack = 0; blockOnTrack < 8; blockOnTrack++)
        {
            var data = new byte[512];
            var integrity = true;
            var revolution = 0;
            var complete = true;
            for (var half = 0; half < 2; half++)
            {
                var logicalSector = blockOnTrack * 2 + half;
                var address = new SectorAddress(track, 0, AppleIIGeometry.ProDosToPhysical[logicalSector]);
                if (!candidates.TryGetValue(address, out var values))
                {
                    complete = false;
                    break;
                }
                var selected = AppleScpSectorDecoder.Select(0, address, values);
                selected.Data.ToArray().CopyTo(data, half * 256);
                integrity &= selected.IntegrityValid == true;
                revolution = Math.Max(revolution, selected.Revolution);
            }
            if (complete)
                blocks.Add(new(track * 8 + blockOnTrack, new(track, 0, blockOnTrack), data,
                    integrity, revolution));
        }
        if (blocks.Count == 0) throw ScpReconstructionExceptions.NoUsableSectors("Apple II ProDOS");
        return new(DiskImageFormatIds.AppleIIProDos, 512, tracks, DiskGeometryConstants.SingleSidedHeadCount, 8, blocks);
    }
}
