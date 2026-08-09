namespace GWGUI.Scp.SectorImages;

internal static class EpsonQx10SectorImagePolicy
{
    public static SectorImage CreateImage(
        string formatId,
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates)
    {
        var geometry = GetGeometry(formatId);
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

    public static bool TryDetectFormat(
        IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates,
        out string formatId)
    {
        formatId = string.Empty;
        var tracks = candidates.GroupBy(pair => (pair.Key.Cylinder, pair.Key.Head))
            .Select(group => new DetectedTrack(group.Key.Cylinder, group.Key.Head,
                group.Select(pair => new DetectedSector(pair.Key.Number,
                    pair.Value
                        .Where(value => value.Sector.Data is not null)
                        .GroupBy(value => value.Sector.IntegrityValid == true ? 2 : value.Sector.IntegrityValid is null ? 1 : 0)
                        .OrderByDescending(candidateGroup => candidateGroup.Key)
                        .First()
                        .GroupBy(value => value.Sector.Data!.Count)
                        .OrderByDescending(sizes => sizes.Count())
                        .ThenByDescending(sizes => sizes.Key)
                        .First().Key)).ToArray())).ToArray();
        if (tracks.Length == 0) return false;

        static bool Matches(DetectedTrack track, int first, int count, int size) =>
            track.Sectors.Count == count && track.Sectors.All(sector =>
                sector.Number >= first && sector.Number < first + count && sector.Size == size);

        if (tracks.All(track => Matches(track, 1, 16, 256))) formatId = "epson.qx10.320";
        else if (tracks.All(track => Matches(track, 1, 5, 1024))) formatId = "epson.qx10.400";
        else if (tracks.Length <= 15 && tracks.All(track => track.Head == 0 &&
                     Matches(track, 1, track.Cylinder == 0 ? 16 : 17, 256))) formatId = "epson.qx10.booter";
        else
        {
            var smallTracks = tracks.Where(track => Matches(track, 1, 16, 256)).ToArray();
            var normalTracks = tracks.Where(track => Matches(track, 1, 10, 512)).ToArray();
            if (smallTracks.Length == 1 && smallTracks[0].Cylinder == 0 && smallTracks[0].Head == 0 &&
                smallTracks.Length + normalTracks.Length == tracks.Length) formatId = "epson.qx10.399";
            else if (smallTracks.Length >= 4 && smallTracks.All(track => track.Cylinder <= 1) &&
                     smallTracks.Length + normalTracks.Length == tracks.Length) formatId = "epson.qx10.396";
            else
            {
                var shiftedTracks = tracks.Where(track => Matches(track, 2, 10, 512)).ToArray();
                if (smallTracks.Length >= 6 && smallTracks.All(track => track.Cylinder is 0 or 1 or 4) &&
                    shiftedTracks.All(track => track.Cylinder is 5 or 6) &&
                    smallTracks.Length + normalTracks.Length + shiftedTracks.Length == tracks.Length)
                    formatId = "epson.qx10.logo";
            }
        }
        return formatId.Length > 0;
    }

    private static Geometry GetGeometry(string formatId) => formatId.ToLowerInvariant() switch
    {
        "epson.qx10.320" => Geometry.Uniform(40, 2, new(1, 16, 256)),
        "epson.qx10.400" => Geometry.Uniform(40, 2, new(1, 5, 1024)),
        "epson.qx10.booter" => new(15, 1, (cylinder, _) => cylinder == 0 ? new(1, 16, 256) : new(1, 17, 256)),
        "epson.qx10.399" => new(40, 2, (cylinder, head) => cylinder == 0 && head == 0 ? new(1, 16, 256) : new(1, 10, 512)),
        "epson.qx10.logo" => new(40, 2, (cylinder, _) => cylinder switch
        {
            0 or 1 or 4 => new(1, 16, 256),
            5 or 6 => new(2, 10, 512),
            3 or 7 => default,
            _ => new(1, 10, 512)
        }),
        _ => new(40, 2, (cylinder, _) => cylinder <= 1 ? new(1, 16, 256) : new(1, 10, 512))
    };

    private readonly record struct Track(int FirstSector, int Count, int SectorSize);
    private readonly record struct DetectedSector(int Number, int Size);
    private readonly record struct DetectedTrack(int Cylinder, int Head, IReadOnlyList<DetectedSector> Sectors);

    private sealed record Geometry(int Cylinders, int Heads, Func<int, int, Track> Track)
    {
        public IEnumerable<Track> AllTracks
        {
            get
            {
                for (var cylinder = 0; cylinder < Cylinders; cylinder++)
                    for (var head = 0; head < Heads; head++)
                        yield return Track(cylinder, head);
            }
        }

        public static Geometry Uniform(int cylinders, int heads, Track track) =>
            new(cylinders, heads, (_, _) => track);
    }
}
