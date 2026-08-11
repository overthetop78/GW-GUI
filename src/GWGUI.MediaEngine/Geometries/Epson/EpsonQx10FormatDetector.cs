using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Geometries.Epson;

internal readonly record struct EpsonQx10SectorDescriptor(int Cylinder, int Head, int Number, int Size);

internal static class EpsonQx10FormatDetector
{
    public static bool TryDetect(IReadOnlyCollection<EpsonQx10SectorDescriptor> sectors, out string formatId)
    {
        formatId = string.Empty;
        var tracks = sectors.GroupBy(sector => (sector.Cylinder, sector.Head)).Select(group => new DetectedTrack(group.Key.Cylinder, group.Key.Head, group.Select(sector => new DetectedSector(sector.Number, sector.Size)).ToArray())).ToArray();
        if (tracks.Length == 0) return false;

        if (MatchesAll(tracks, DiskImageFormatIds.EpsonQx10_320)) formatId = DiskImageFormatIds.EpsonQx10_320;
        else if (MatchesAll(tracks, DiskImageFormatIds.EpsonQx10_400)) formatId = DiskImageFormatIds.EpsonQx10_400;
        else if (tracks.Length <= 15 && tracks.All(track => track.Head == 0) && MatchesAll(tracks, DiskImageFormatIds.EpsonQx10Booter)) formatId = DiskImageFormatIds.EpsonQx10Booter;
        else if (tracks.Count(track => track.Sectors.All(sector => sector.Size == 256)) == 1 && MatchesAll(tracks, DiskImageFormatIds.EpsonQx10_399)) formatId = DiskImageFormatIds.EpsonQx10_399;
        else if (tracks.Count(track => track.Sectors.All(sector => sector.Size == 256)) >= 4 && MatchesAll(tracks, DiskImageFormatIds.EpsonQx10_396)) formatId = DiskImageFormatIds.EpsonQx10_396;
        else if (tracks.Count(track => track.Sectors.All(sector => sector.Size == 256)) >= 6 && MatchesAll(tracks, DiskImageFormatIds.EpsonQx10Logo)) formatId = DiskImageFormatIds.EpsonQx10Logo;
        return formatId.Length > 0;
    }

    private static bool MatchesAll(IEnumerable<DetectedTrack> tracks, string formatId)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        return tracks.All(track => track.Cylinder < geometry.Cylinders && track.Head < geometry.Heads && Matches(track, geometry.Track(track.Cylinder, track.Head)));
    }

    private static bool Matches(DetectedTrack track, EpsonQx10TrackGeometry expected) => expected.Count > 0 && track.Sectors.Count == expected.Count && track.Sectors.All(sector => sector.Number >= expected.FirstSector && sector.Number < expected.FirstSector + expected.Count && sector.Size == expected.SectorSize);

    private readonly record struct DetectedSector(int Number, int Size);
    private readonly record struct DetectedTrack(int Cylinder, int Head, IReadOnlyList<DetectedSector> Sectors);
}
