namespace GWGUI.MediaEngine.SectorImages;

internal static class EpsonQx10SectorImageBuilder
{
    public static SectorImage Create(
        string formatId,
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        var blocks = new List<SectorBlock>();
        var logical = 0;
        long capacity = 0;
        var maximumSectors = 0;
        var sizes = new HashSet<int>();

        for (var cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
        {
            for (var head = 0; head < geometry.Heads; head++)
            {
                var track = geometry.Track(cylinder, head);
                maximumSectors = Math.Max(maximumSectors, track.Count);
                for (var index = 0; index < track.Count; index++, logical++)
                {
                    var sectorNumber = track.FirstSector + index;
                    var address = new SectorAddress(cylinder, head, sectorNumber);
                    capacity += track.SectorSize;
                    sizes.Add(track.SectorSize);
                    if (!candidates.TryGetValue(address, out var values)) continue;

                    var best = values.Where(value => value.Sector.Data?.Count == track.SectorSize)
                        .OrderByDescending(value => value.Sector.IntegrityValid == true)
                        .ThenByDescending(value => value.Sector.IntegrityValid is null)
                        .FirstOrDefault();
                    if (best?.Sector.Data is null) continue;
                    blocks.Add(new(logical, address, best.Sector.Data.ToArray(), best.Sector.IntegrityValid, best.Revolution));
                }
            }
        }

        var blockSize = sizes.GroupBy(size => size).OrderByDescending(group =>
            geometry.AllTracks.Where(track => track.SectorSize == group.Key).Sum(track => track.Count)).First().Key;
        return new(formatId, blockSize, geometry.Cylinders, geometry.Heads, maximumSectors, blocks,
            allowVariableBlockSize: sizes.Count > 1, capacity: capacity, logicalBlockCount: logical);
    }
}
