using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Reconstruction;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Reconstruit les images sectorielles Commodore GCR et 1581 MFM depuis une capture SCP.</summary>
public sealed class CommodoreScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    /// <summary>Lit la capture et sélectionne la reconstruction Commodore adaptée.</summary>
    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId == DiskImageFormatIds.Commodore1581) return Read1581(scp, cancellationToken);
        return ReadGcr(scp, formatId, cancellationToken);
    }

    /// <summary>Reconstruit une image 1541 ou 1571 depuis les secteurs GCR.</summary>
    private SectorImage ReadGcr(ScpImage scp, string? requestedFormat, CancellationToken cancellationToken)
    {
        var candidates = new Dictionary<(int Track, int Sector), List<(DecodedSector Sector, int Revolution)>>();
        foreach (var physicalTrack in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < physicalTrack.Revolutions.Count; revolution++)
            {
                var decoded = decoders.Decode(FluxCodecIds.CommodoreGcr, physicalTrack.Revolutions[revolution].Flux);
                foreach (var sector in decoded.Sectors)
                {
                    if (sector.Data is null || sector.Cylinder is < 1 or > DiskGeometryConstants.EightyTrackCylinderCount || sector.Number < 0) continue;
                    var key = ((int)sector.Cylinder, sector.Number);
                    if (!candidates.TryGetValue(key, out var list)) candidates[key] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        if (candidates.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors(CommodoreGcrFormat.StructureDescriptionName);
        var maxTrack = candidates.Keys.Max(key => key.Track);
        var is1571 = requestedFormat == DiskImageFormatIds.Commodore1571 || maxTrack > Commodore1541Geometry.ExtendedTrackCount || scp.Tracks.Any(track => track.Head == 1);
        var hasExtendedData = candidates
            .Where(candidate => candidate.Key.Track > Commodore1541Geometry.StandardTrackCount && candidate.Key.Track <= Commodore1541Geometry.ExtendedTrackCount)
            .SelectMany(candidate => candidate.Value)
            .Any(candidate => candidate.Sector.Data?.Any(value => value != 0) == true);
        var tracksPerSide = is1571 ? Commodore1541Geometry.StandardTrackCount : maxTrack > Commodore1541Geometry.StandardTrackCount && hasExtendedData ? Commodore1541Geometry.ExtendedTrackCount : Commodore1541Geometry.StandardTrackCount;
        var sides = is1571 ? Commodore1571Geometry.SideCount : DiskGeometryConstants.SingleSidedHeadCount;
        var count = Commodore1541Geometry.BlocksPerSide(tracksPerSide) * sides;
        var blocks = new List<SectorBlock>();
        foreach (var (key, values) in candidates)
        {
            var side = key.Track > tracksPerSide ? 1 : 0;
            var track = side == 0 ? key.Track : key.Track - tracksPerSide;
            if (side >= sides || track < 1 || track > tracksPerSide || key.Sector >= Commodore1541Geometry.SectorsPerTrack(track)) continue;
            var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true).ThenByDescending(value => value.Sector.IntegrityValid is null).First();
            var logical = is1571 ? Commodore1571Geometry.ToLogicalBlock(track, key.Sector, tracksPerSide, side) : Commodore1541Geometry.ToSideLogicalBlock(track, key.Sector, tracksPerSide);
            blocks.Add(new(logical, new(track - 1, side, key.Sector), best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
        }
        var formatId = is1571 ? DiskImageFormatIds.Commodore1571 : DiskImageFormatIds.Commodore1541;
        return new(formatId, CommodoreGcrFormat.SectorByteCount, tracksPerSide, sides, Commodore1541Geometry.MaximumSectorsPerTrack, blocks, capacity: count * (long)CommodoreGcrFormat.SectorByteCount, logicalBlockCount: count);
    }

    /// <summary>Reconstruit les blocs logiques d'une image 1581 depuis les secteurs MFM.</summary>
    private SectorImage Read1581(ScpImage scp, CancellationToken cancellationToken)
    {
        var candidates = new Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                var decoded = decoders.Decode(FluxCodecIds.IsoMfm, track.Revolutions[revolution].Flux);
                foreach (var sector in decoded.Sectors)
                {
                    if (sector.Data is null || sector.Data.Count != Commodore1581Geometry.PhysicalSectorSize || sector.Number is < 1 or > Commodore1581Geometry.PhysicalSectorsPerTrack) continue;
                    var address = new SectorAddress(track.Cylinder, track.Head, sector.Number);
                    if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        if (candidates.Count == 0) throw ScpReconstructionExceptions.NoDecodedSectors("Commodore 1581 MFM");
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true).ThenByDescending(value => value.Sector.IntegrityValid is null).First();
            var physical = best.Sector.Data!.ToArray();
            var logicalBase = Commodore1581Geometry.PhysicalSectorToLogicalBlock(address.Cylinder, address.Head, address.Number);
            for (var half = 0; half < Commodore1581Geometry.LogicalBlocksPerPhysicalSector; half++)
            {
                var logical = logicalBase + half;
                blocks.Add(new(logical, new(logical / Commodore1581Geometry.LogicalBlocksPerTrack, 0, logical % Commodore1581Geometry.LogicalBlocksPerTrack), physical.AsSpan(half * Commodore1581Geometry.LogicalBlockSize, Commodore1581Geometry.LogicalBlockSize).ToArray(), best.Sector.IntegrityValid, best.Revolution));
            }
        }
        return new(DiskImageFormatIds.Commodore1581, Commodore1581Geometry.LogicalBlockSize, Commodore1581Geometry.LogicalCylinderCount, Commodore1581Geometry.LogicalHeadCount, Commodore1581Geometry.LogicalBlocksPerTrack, blocks);
    }
}
