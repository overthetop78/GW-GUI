using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Geometries.Epson;

/// <summary>Décrit un secteur observé pendant la détection Epson QX-10.</summary>
/// <param name="Cylinder">Numéro de cylindre observé.</param>
/// <param name="Head">Numéro de face observé.</param>
/// <param name="Number">Numéro de secteur observé.</param>
/// <param name="Size">Taille des données observées, en octets.</param>
internal readonly record struct EpsonQx10SectorDescriptor(int Cylinder, int Head, int Number, int Size);

/// <summary>Détecte la disposition Epson QX-10 correspondant aux secteurs observés.</summary>
internal static class EpsonQx10FormatDetector
{
    /// <summary>Tente d'identifier une disposition Epson complète.</summary>
    /// <param name="sectors">Descripteurs des secteurs possédant des données observables.</param>
    /// <param name="formatId">Identifiant Epson détecté lorsque la méthode retourne <see langword="true"/>.</param>
    /// <returns><see langword="true"/> lorsqu'une disposition Epson prise en charge correspond à toutes les pistes ; sinon <see langword="false"/>.</returns>
    public static bool TryDetect(IReadOnlyCollection<EpsonQx10SectorDescriptor> sectors, out string formatId)
    {
        formatId = string.Empty;
        var tracks = sectors.GroupBy(sector => (sector.Cylinder, sector.Head)).Select(group => new DetectedTrack(group.Key.Cylinder, group.Key.Head, group.Select(sector => new DetectedSector(sector.Number, sector.Size)).ToArray())).ToArray();
        if (tracks.Length == 0) return false;

        if (MatchesAll(tracks, DiskImageFormatIds.EpsonQx10_320)) formatId = DiskImageFormatIds.EpsonQx10_320;
        else if (MatchesAll(tracks, DiskImageFormatIds.EpsonQx10_400)) formatId = DiskImageFormatIds.EpsonQx10_400;
        else if (tracks.Length <= EpsonQx10GeometryCatalog.BooterTrackCount && tracks.All(track => track.Head == 0) && MatchesAll(tracks, DiskImageFormatIds.EpsonQx10Booter)) formatId = DiskImageFormatIds.EpsonQx10Booter;
        else if (tracks.Count(track => track.Sectors.All(sector => sector.Size == EpsonQx10GeometryCatalog.Layout320.SectorSize)) == EpsonQx10GeometryCatalog.Format399SmallTrackCount && MatchesAll(tracks, DiskImageFormatIds.EpsonQx10_399)) formatId = DiskImageFormatIds.EpsonQx10_399;
        else if (tracks.Count(track => track.Sectors.All(sector => sector.Size == EpsonQx10GeometryCatalog.Layout320.SectorSize)) >= EpsonQx10GeometryCatalog.Format396MinimumSmallTrackCount && MatchesAll(tracks, DiskImageFormatIds.EpsonQx10_396)) formatId = DiskImageFormatIds.EpsonQx10_396;
        else if (tracks.Count(track => track.Sectors.All(sector => sector.Size == EpsonQx10GeometryCatalog.Layout320.SectorSize)) >= EpsonQx10GeometryCatalog.LogoMinimumSmallTrackCount && MatchesAll(tracks, DiskImageFormatIds.EpsonQx10Logo)) formatId = DiskImageFormatIds.EpsonQx10Logo;
        return formatId.Length > 0;
    }

    /// <summary>Vérifie que toutes les pistes correspondent à la disposition demandée.</summary>
    private static bool MatchesAll(IEnumerable<DetectedTrack> tracks, string formatId)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        return tracks.All(track => track.Cylinder < geometry.Cylinders && track.Head < geometry.Heads && Matches(track, geometry.Track(track.Cylinder, track.Head)));
    }

    /// <summary>Vérifie les secteurs d'une piste contre sa disposition attendue.</summary>
    private static bool Matches(DetectedTrack track, EpsonQx10TrackGeometry expected) => expected.Count > 0 && track.Sectors.Count == expected.Count && track.Sectors.All(sector => sector.Number >= expected.FirstSector && sector.Number < expected.FirstSector + expected.Count && sector.Size == expected.SectorSize);

    /// <summary>Décrit le numéro et la taille en octets d'un secteur retenu.</summary>
    /// <param name="Number">Numéro du secteur observé.</param>
    /// <param name="Size">Taille des données observées, en octets.</param>
    private readonly record struct DetectedSector(int Number, int Size);
    /// <summary>Regroupe les secteurs retenus pour un cylindre et une face.</summary>
    /// <param name="Cylinder">Numéro de cylindre observé.</param>
    /// <param name="Head">Numéro de face observé.</param>
    /// <param name="Sectors">Secteurs possédant des données sur cette piste.</param>
    private readonly record struct DetectedTrack(int Cylinder, int Head, IReadOnlyList<DetectedSector> Sectors);
}
