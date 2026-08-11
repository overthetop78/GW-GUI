using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.SectorImages;

public sealed class CommodoreScpSectorImageReader(IScpReader scpReader, FluxDecoderRegistry decoders)
{
    public async Task<SectorImage> ReadAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        var scp = await scpReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (formatId == DiskImageFormatIds.Commodore1581) return Read1581(scp, cancellationToken);
        return ReadGcr(scp, formatId, cancellationToken);
    }

    private SectorImage ReadGcr(ScpImage scp, string? requestedFormat, CancellationToken cancellationToken)
    {
        var candidates = new Dictionary<(int Track, int Sector), List<(DecodedSector Sector, int Revolution)>>();
        foreach (var physicalTrack in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < physicalTrack.Revolutions.Count; revolution++)
            {
                var decoded = decoders.Decode(FluxCodecIds.CommodoreGcr, physicalTrack.Revolutions[revolution]);
                foreach (var sector in decoded.Sectors ?? [])
                {
                    if (sector.Data is null || sector.Cylinder is < 1 or > DiskGeometryConstants.EightyTrackCylinderCount || sector.Number < 0) continue;
                    var key = ((int)sector.Cylinder, sector.Number);
                    if (!candidates.TryGetValue(key, out var list)) candidates[key] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        if (candidates.Count == 0) throw new InvalidDataException("No Commodore GCR sectors could be decoded from the SCP image.");
        var maxTrack = candidates.Keys.Max(key => key.Track);
        var is1571 = requestedFormat == DiskImageFormatIds.Commodore1571 || maxTrack > 40 || scp.Tracks.Any(track => track.Head == 1);
        var hasExtendedData = candidates
            .Where(candidate => candidate.Key.Track > 35 && candidate.Key.Track <= 40)
            .SelectMany(candidate => candidate.Value)
            .Any(candidate => candidate.Sector.Data?.Any(value => value != 0) == true);
        var tracksPerSide = is1571 ? 35 : maxTrack > 35 && hasExtendedData ? 40 : 35;
        var sides = is1571 ? 2 : 1;
        var count = CommodoreGeometry.BlocksPer1541Side(tracksPerSide) * sides;
        var blocks = new List<SectorBlock>();
        foreach (var (key, values) in candidates)
        {
            var side = key.Track > tracksPerSide ? 1 : 0;
            var track = side == 0 ? key.Track : key.Track - tracksPerSide;
            if (side >= sides || track < 1 || track > tracksPerSide || key.Sector >= CommodoreGeometry.SectorsFor1541Track(track)) continue;
            var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true).ThenByDescending(value => value.Sector.IntegrityValid is null).First();
            var logical = CommodoreGeometry.To1541LogicalBlock(track, key.Sector, tracksPerSide, side);
            blocks.Add(new(logical, new(track - 1, side, key.Sector), best.Sector.Data!.ToArray(), best.Sector.IntegrityValid, best.Revolution));
        }
        var formatId = is1571 ? DiskImageFormatIds.Commodore1571 : DiskImageFormatIds.Commodore1541;
        return new(formatId, 256, tracksPerSide, sides, 21, blocks, capacity: count * 256L, logicalBlockCount: count);
    }

    private SectorImage Read1581(ScpImage scp, CancellationToken cancellationToken)
    {
        var candidates = new Dictionary<SectorAddress, List<(DecodedSector Sector, int Revolution)>>();
        foreach (var track in scp.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var revolution = 0; revolution < track.Revolutions.Count; revolution++)
            {
                var decoded = decoders.Decode(FluxCodecIds.IsoMfm, track.Revolutions[revolution]);
                foreach (var sector in decoded.Sectors ?? [])
                {
                    if (sector.Data is null || sector.Data.Count != 512 || sector.Number is < 1 or > 10) continue;
                    var address = new SectorAddress(track.Cylinder, track.Head, sector.Number);
                    if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
                    list.Add((sector, revolution + 1));
                }
            }
        }
        if (candidates.Count == 0) throw new InvalidDataException("No Commodore 1581 MFM sectors could be decoded from the SCP image.");
        var blocks = new List<SectorBlock>();
        foreach (var (address, values) in candidates)
        {
            var best = values.OrderByDescending(value => value.Sector.IntegrityValid == true).ThenByDescending(value => value.Sector.IntegrityValid is null).First();
            var physical = best.Sector.Data!.ToArray();
            var logicalBase = (address.Cylinder * 40) + ((address.Head ^ 1) * 10 + address.Number - 1) * 2;
            for (var half = 0; half < 2; half++)
            {
                var logical = logicalBase + half;
                blocks.Add(new(logical, new(logical / 40, 0, logical % 40),
                    physical.AsSpan(half * 256, 256).ToArray(), best.Sector.IntegrityValid, best.Revolution));
            }
        }
        return new(DiskImageFormatIds.Commodore1581, 256, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, 40, blocks);
    }
}
