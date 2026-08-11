using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.SectorImages;

internal static class EpsonQx10FormatDetector
{
    public static bool TryDetect(IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates, out string formatId)
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

        if (tracks.All(track => Matches(track, 1, 16, 256))) formatId = DiskImageFormatIds.EpsonQx10_320;
        else if (tracks.All(track => Matches(track, 1, 5, 1024))) formatId = DiskImageFormatIds.EpsonQx10_400;
        else if (tracks.Length <= 15 && tracks.All(track => track.Head == 0 &&
                     Matches(track, 1, track.Cylinder == 0 ? 16 : 17, 256))) formatId = DiskImageFormatIds.EpsonQx10Booter;
        else
        {
            var smallTracks = tracks.Where(track => Matches(track, 1, 16, 256)).ToArray();
            var normalTracks = tracks.Where(track => Matches(track, 1, 10, 512)).ToArray();
            if (smallTracks.Length == 1 && smallTracks[0].Cylinder == 0 && smallTracks[0].Head == 0 && smallTracks.Length + normalTracks.Length == tracks.Length) formatId = DiskImageFormatIds.EpsonQx10_399;
            else if (smallTracks.Length >= 4 && smallTracks.All(track => track.Cylinder <= 1) &&
                     smallTracks.Length + normalTracks.Length == tracks.Length) formatId = DiskImageFormatIds.EpsonQx10_396;
            else
            {
                var shiftedTracks = tracks.Where(track => Matches(track, 2, 10, 512)).ToArray();
                if (smallTracks.Length >= 6 && smallTracks.All(track => track.Cylinder is 0 or 1 or 4) &&
                    shiftedTracks.All(track => track.Cylinder is 5 or 6) &&
                    smallTracks.Length + normalTracks.Length + shiftedTracks.Length == tracks.Length)
                    formatId = DiskImageFormatIds.EpsonQx10Logo;
            }
        }
        return formatId.Length > 0;
    }

    private static bool Matches(DetectedTrack track, int first, int count, int size) =>
        track.Sectors.Count == count && track.Sectors.All(sector => sector.Number >= first && sector.Number < first + count && sector.Size == size);

    private readonly record struct DetectedSector(int Number, int Size);
    private readonly record struct DetectedTrack(int Cylinder, int Head, IReadOnlyList<DetectedSector> Sectors);
}
